using DataGo.Domain;

namespace DataGo.Application;

public sealed class AssistantMessageHandler(
    IAssistantModel model,
    SearchResidentialCustomersHandler searchCustomers,
    CountResidentialCustomersHandler countCustomers,
    GetResidentialCustomerHandler getCustomer,
    SearchNeighborhoodsHandler searchNeighborhoods,
    GetCentersHandler getCenters,
    IPendingAssistantActions pendingActions,
    TimeProvider timeProvider)
{
    public async Task<AssistantMessageResponse> HandleAsync(AssistantMessageRequest request, CancellationToken cancellationToken)
    {
        var message = Normalize(request.Message);
        if (message.Length is < 2 or > 1000)
            throw new DomainValidationException("El mensaje debe tener entre 2 y 1000 caracteres.");

        var interpretation = await model.InterpretAsync(new(message, request.Context), cancellationToken);
        if (interpretation.RequiresClarification)
            return Response(AssistantResponseType.Clarification,
                Normalize(interpretation.Clarification) is { Length: > 0 } clarification
                    ? clarification : "Necesito un poco más de información para continuar.", request.Context);

        try
        {
            return interpretation.Intent switch
            {
                AssistantIntent.HELP => Help(request.Context),
                AssistantIntent.SEARCH_CUSTOMERS => await SearchAsync(interpretation, request.Context, cancellationToken),
                AssistantIntent.COUNT_CUSTOMERS => await CountAsync(interpretation, request.Context, cancellationToken),
                AssistantIntent.GET_CUSTOMER => await GetAsync(interpretation, request.Context, cancellationToken),
                AssistantIntent.UPDATE_CUSTOMER => await ProposeUpdateAsync(interpretation, request.Context, cancellationToken),
                AssistantIntent.RETIRE_CUSTOMER => await ProposeRetireAsync(interpretation, request.Context, cancellationToken),
                AssistantIntent.CREATE_CUSTOMER => Response(AssistantResponseType.Text,
                    "Puedo abrir el formulario de creación para que completes los datos obligatorios sin inventar información.",
                    request.Context, openCreateForm: true),
                _ => Response(AssistantResponseType.Clarification,
                    "No pude identificar la operación. Puedes pedirme buscar, contar, consultar, actualizar o retirar clientes.", request.Context)
            };
        }
        catch (ResolutionException exception)
        {
            return Response(exception.Type, exception.Message, request.Context, exception.Customers);
        }
    }

    private async Task<AssistantMessageResponse> SearchAsync(AssistantInterpretation interpretation,
        AssistantConversationContext? context, CancellationToken cancellationToken)
    {
        var (criteria, resolvedFilters) = await ResolveCriteriaAsync(interpretation.Filters, context?.Filters, cancellationToken);
        var customers = await searchCustomers.HandleAsync(criteria, cancellationToken);
        var cards = await CardsAsync(customers.Take(100), cancellationToken);
        var message = customers.Count == 0 ? "No encontré clientes con esos criterios."
            : $"Encontré {Math.Min(customers.Count, 100)} {(customers.Count == 1 ? "cliente" : "clientes")}{Describe(resolvedFilters)}.";
        return new(AssistantResponseType.CustomerList, message, cards, IsTruncated: customers.Count > 100,
            Context: new(null, resolvedFilters));
    }

    private async Task<AssistantMessageResponse> CountAsync(AssistantInterpretation interpretation,
        AssistantConversationContext? context, CancellationToken cancellationToken)
    {
        var (criteria, resolvedFilters) = await ResolveCriteriaAsync(interpretation.Filters, context?.Filters, cancellationToken);
        var count = await countCustomers.HandleAsync(criteria, cancellationToken);
        return new(AssistantResponseType.Count,
            $"Hay {count} {(count == 1 ? "cliente" : "clientes")}{Describe(resolvedFilters)}.",
            Count: count, Context: new(null, resolvedFilters));
    }

    private async Task<AssistantMessageResponse> GetAsync(AssistantInterpretation interpretation,
        AssistantConversationContext? context, CancellationToken cancellationToken)
    {
        var customer = await ResolveCustomerAsync(interpretation.CustomerReference, context, cancellationToken);
        var cards = await CardsAsync([customer], cancellationToken);
        return new(AssistantResponseType.CustomerDetail, $"Encontré a {customer.FullName} ({customer.Code}).",
            cards, Context: new(customer.Id, context?.Filters));
    }

    private async Task<AssistantMessageResponse> ProposeUpdateAsync(AssistantInterpretation interpretation,
        AssistantConversationContext? context, CancellationToken cancellationToken)
    {
        var customer = await ResolveCustomerAsync(interpretation.CustomerReference, context, cancellationToken);
        if (customer.IsBlocked) throw new ResolutionException(AssistantResponseType.Error,
            $"{customer.Code} está bloqueado y no puede modificarse.");
        if (interpretation.Fields is not { Count: > 0 }) throw new ResolutionException(
            AssistantResponseType.Clarification, "Indica qué dato quieres cambiar y su nuevo valor.");

        var businessName = customer.BusinessName;
        var mobilePhone = customer.MobilePhone;
        var email = customer.Email;
        var stratum = customer.Stratum;
        var changes = new List<AssistantChangePreview>();
        foreach (var change in interpretation.Fields)
        {
            var value = NormalizeNullable(change.Value);
            switch (change.Field.Trim().ToLowerInvariant())
            {
                case "mobilephone":
                    changes.Add(new("Celular", mobilePhone, value)); mobilePhone = value; break;
                case "email":
                    changes.Add(new("Correo", email, value)); email = value; break;
                case "stratum":
                    if (!short.TryParse(value, out var parsed) || parsed is < 1 or > 6)
                        throw new ResolutionException(AssistantResponseType.Clarification, "El estrato debe ser un número entre 1 y 6.");
                    changes.Add(new("Estrato", stratum.ToString(), parsed.ToString())); stratum = parsed; break;
                case "businessname":
                    if (value is null) throw new ResolutionException(AssistantResponseType.Clarification,
                        "El nombre de negocio no puede quedar vacío.");
                    changes.Add(new("Nombre negocio", businessName, value)); businessName = value; break;
                default:
                    throw new ResolutionException(AssistantResponseType.Error,
                        $"El campo '{change.Field}' no se puede modificar desde el asistente.");
            }
        }

        var update = new UpdateResidentialCustomerRequest(businessName, customer.FirstNames, customer.LastNames,
            customer.Phone, customer.PhoneExtension, mobilePhone, email, stratum, customer.CenterId,
            ToAddressRequest(customer.Address));
        var preview = CreatePending(AssistantIntent.UPDATE_CUSTOMER, customer,
            $"Actualizar {string.Join(", ", changes.Select(x => x.Field.ToLowerInvariant()))} de {customer.Code}.", changes);
        pendingActions.Add(new(preview, update, customer.UpdatedAt));
        return new(AssistantResponseType.Confirmation, "Revisa el cambio antes de confirmarlo.",
            PendingAction: preview, Context: new(customer.Id, context?.Filters));
    }

    private async Task<AssistantMessageResponse> ProposeRetireAsync(AssistantInterpretation interpretation,
        AssistantConversationContext? context, CancellationToken cancellationToken)
    {
        var customer = await ResolveCustomerAsync(interpretation.CustomerReference, context, cancellationToken);
        if (customer.IsBlocked) throw new ResolutionException(AssistantResponseType.Error,
            $"{customer.Code} ya está bloqueado.");
        var preview = CreatePending(AssistantIntent.RETIRE_CUSTOMER, customer,
            "Esta operación realiza una baja lógica. El cliente continuará siendo consultable, pero quedará bloqueado para modificaciones.", []);
        pendingActions.Add(new(preview, null, customer.UpdatedAt));
        return new(AssistantResponseType.Confirmation, "Confirma el retiro lógico para continuar.",
            PendingAction: preview, Context: new(customer.Id, context?.Filters));
    }

    private PendingAssistantAction CreatePending(AssistantIntent intent, ResidentialCustomerResponse customer,
        string summary, IReadOnlyList<AssistantChangePreview> changes) => new(
        Guid.NewGuid().ToString("N"), intent, customer.Id, customer.Code, customer.FullName,
        $"{customer.DocumentType} {customer.DocumentNumber}", summary, changes, timeProvider.GetUtcNow().AddMinutes(10));

    private async Task<ResidentialCustomerResponse> ResolveCustomerAsync(AssistantCustomerReference? reference,
        AssistantConversationContext? context, CancellationToken cancellationToken)
    {
        if (reference is null || string.IsNullOrWhiteSpace(reference.Code) &&
            string.IsNullOrWhiteSpace(reference.DocumentNumber) && string.IsNullOrWhiteSpace(reference.Name))
        {
            if (context?.CustomerId is Guid id) return await getCustomer.HandleAsync(id, cancellationToken);
            throw new ResolutionException(AssistantResponseType.Clarification,
                "Indica el código, documento o nombre del cliente.");
        }
        var search = First(reference.Code, reference.DocumentNumber, reference.Name)!;
        var matches = await searchCustomers.HandleAsync(new(search), cancellationToken);
        var exact = matches.Where(x => Equal(reference.Code, x.Code) || Equal(reference.DocumentNumber, x.DocumentNumber)).ToList();
        if (exact.Count == 1) return exact[0];
        if (matches.Count == 0) throw new ResolutionException(AssistantResponseType.Error,
            $"No encontré un cliente para '{search}'.");
        if (matches.Count == 1) return matches[0];
        var cards = await CardsAsync(matches.Take(8), cancellationToken);
        throw new ResolutionException(AssistantResponseType.Clarification,
            "Encontré varios clientes. Selecciona uno de estos candidatos.", cards);
    }

    private async Task<(SearchResidentialCustomerCriteria, AssistantFilters)> ResolveCriteriaAsync(
        AssistantFilters? requested, AssistantFilters? previous, CancellationToken cancellationToken)
    {
        var filters = Merge(requested, previous);
        Guid? neighborhoodId = null;
        if (!string.IsNullOrWhiteSpace(filters.Neighborhood))
        {
            var matches = await searchNeighborhoods.HandleAsync(filters.Neighborhood, cancellationToken);
            var exact = matches.Where(x => Equal(x.Name, filters.Neighborhood)).ToList();
            if (exact.Count == 1) neighborhoodId = exact[0].Id;
            else if (matches.Count == 0) throw new ResolutionException(AssistantResponseType.Error,
                $"El barrio {filters.Neighborhood} no fue encontrado.");
            else throw new ResolutionException(AssistantResponseType.Clarification,
                $"Encontré varios barrios para '{filters.Neighborhood}'. Especifica el nombre completo.");
        }
        Guid? centerId = null;
        if (!string.IsNullOrWhiteSpace(filters.Center))
        {
            var centers = await getCenters.HandleAsync(cancellationToken);
            var matches = centers.Where(x => x.Name.Contains(filters.Center, StringComparison.OrdinalIgnoreCase)
                || x.Code.Contains(filters.Center, StringComparison.OrdinalIgnoreCase)).ToList();
            var exact = matches.Where(x => Equal(x.Name, filters.Center) || Equal(x.Code, filters.Center)).ToList();
            if (exact.Count == 1) centerId = exact[0].Id;
            else if (matches.Count == 1) centerId = matches[0].Id;
            else if (matches.Count == 0) throw new ResolutionException(AssistantResponseType.Error,
                $"El centro {filters.Center} no fue encontrado.");
            else throw new ResolutionException(AssistantResponseType.Clarification,
                $"Encontré varios centros para '{filters.Center}'. Especifica el nombre completo.");
        }
        var status = filters.Status?.Trim().ToLowerInvariant() switch
        {
            null or "" or "all" or "todos" => CustomerStatusFilter.All,
            "active" or "activo" or "activos" => CustomerStatusFilter.Active,
            "blocked" or "bloqueado" or "bloqueados" => CustomerStatusFilter.Blocked,
            _ => throw new ResolutionException(AssistantResponseType.Clarification, "El estado debe ser activos o bloqueados.")
        };
        var treatment = ParseEnum<Treatment>(filters.Treatment, "tratamiento");
        var documentType = ParseEnum<DocumentType>(filters.DocumentType, "tipo de documento");
        return (new(filters.SearchText, status, neighborhoodId, centerId, filters.Stratum, treatment, documentType), filters);
    }

    private async Task<IReadOnlyList<AssistantCustomerCard>> CardsAsync(IEnumerable<ResidentialCustomerResponse> customers,
        CancellationToken cancellationToken)
    {
        var neighborhoods = await searchNeighborhoods.HandleAsync(null, cancellationToken);
        var names = neighborhoods.ToDictionary(x => x.Id, x => x.Name);
        return customers.Select(x => new AssistantCustomerCard(x, names.GetValueOrDefault(x.Address.NeighborhoodId, "Sin barrio"))).ToList();
    }

    private static T? ParseEnum<T>(string? value, string label) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (Enum.TryParse<T>(value.Replace(" ", ""), true, out var result) && Enum.IsDefined(result)) return result;
        throw new ResolutionException(AssistantResponseType.Clarification, $"No reconozco el {label} '{value}'.");
    }

    private static AddressRequest ToAddressRequest(AddressResponse address) => new(address.NeighborhoodId,
        address.IsRural, address.RuralAddress, address.MainRoadType, address.MainRoadNumber,
        address.MainRoadLetter, address.MainRoadCardinality, address.SecondaryRoadNumber1,
        address.SecondaryRoadLetter, address.SecondaryRoadCardinality1, address.SecondaryRoadNumber2,
        address.SecondaryRoadCardinality2);
    private static AssistantFilters Merge(AssistantFilters? current, AssistantFilters? previous) => new(
        current?.SearchText ?? previous?.SearchText, current?.Status ?? previous?.Status,
        current?.Neighborhood ?? previous?.Neighborhood, current?.Center ?? previous?.Center,
        current?.Stratum ?? previous?.Stratum, current?.Treatment ?? previous?.Treatment,
        current?.DocumentType ?? previous?.DocumentType);
    private static string Describe(AssistantFilters filters)
    {
        var parts = new[] { filters.Status, filters.Neighborhood, filters.Center,
            filters.Stratum.HasValue ? $"estrato {filters.Stratum}" : null, filters.Treatment };
        var values = parts.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        return values.Length == 0 ? "" : " para " + string.Join(", ", values);
    }
    private static AssistantMessageResponse Help(AssistantConversationContext? context) => Response(
        AssistantResponseType.Text,
        "Puedo buscar y contar clientes, consultar uno por código o documento y preparar cambios de celular, correo, estrato o nombre de negocio. También puedo preparar un retiro; siempre pediré confirmación antes de modificar datos.", context);
    private static AssistantMessageResponse Response(AssistantResponseType type, string message,
        AssistantConversationContext? context, IReadOnlyList<AssistantCustomerCard>? customers = null,
        bool openCreateForm = false) => new(type, message, customers, Context: context, OpenCreateForm: openCreateForm);
    private static string Normalize(string? value) => string.Join(' ', (value ?? "").Split(
        (char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    private static string? NormalizeNullable(string? value) => Normalize(value) is { Length: > 0 } text ? text : null;
    private static string? First(params string?[] values) => values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))?.Trim();
    private static bool Equal(string? left, string? right) => string.Equals(left?.Trim(), right?.Trim(), StringComparison.OrdinalIgnoreCase);

    private sealed class ResolutionException(AssistantResponseType type, string message,
        IReadOnlyList<AssistantCustomerCard>? customers = null) : Exception(message)
    {
        public AssistantResponseType Type { get; } = type;
        public IReadOnlyList<AssistantCustomerCard>? Customers { get; } = customers;
    }
}

public sealed class ConfirmAssistantActionHandler(
    IPendingAssistantActions pendingActions,
    GetResidentialCustomerHandler getCustomer,
    UpdateResidentialCustomerHandler updateCustomer,
    RetireResidentialCustomerHandler retireCustomer,
    SearchNeighborhoodsHandler searchNeighborhoods)
{
    public async Task<AssistantMessageResponse> HandleAsync(AssistantConfirmRequest request, CancellationToken cancellationToken)
    {
        var action = pendingActions.Take(request.Token?.Trim() ?? "")
            ?? throw new NotFoundException("La confirmación no existe o expiró. Solicita la operación nuevamente.");
        var current = await getCustomer.HandleAsync(action.Preview.TargetCustomerId, cancellationToken);
        if (current.UpdatedAt != action.ExpectedUpdatedAt)
            throw new ConflictException("El cliente cambió después de preparar la confirmación. Revisa los datos y solicita la operación nuevamente.");

        var updated = action.Preview.ActionType switch
        {
            AssistantIntent.UPDATE_CUSTOMER when action.Update is not null =>
                await updateCustomer.HandleAsync(current.Id, action.Update, cancellationToken),
            AssistantIntent.RETIRE_CUSTOMER => await retireCustomer.HandleAsync(current.Id, cancellationToken),
            _ => throw new DomainValidationException("La acción pendiente no es válida.")
        };
        var message = action.Preview.ActionType == AssistantIntent.RETIRE_CUSTOMER
            ? $"{updated.Code} fue retirado y quedó bloqueado."
            : $"Los cambios de {updated.Code} fueron guardados.";
        var neighborhoods = await searchNeighborhoods.HandleAsync(null, cancellationToken);
        var neighborhood = neighborhoods.FirstOrDefault(x => x.Id == updated.Address.NeighborhoodId)?.Name ?? "Sin barrio";
        return new(AssistantResponseType.CustomerDetail, message,
            [new(updated, neighborhood)], Context: new(updated.Id));
    }
}
