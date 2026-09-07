using System.Text.Json;
using DataGo.Application;
using DataGo.Domain;
using Npgsql;
using NpgsqlTypes;

namespace DataGo.Infrastructure;

internal sealed class ResidentialCustomerRepository(NpgsqlDataSource dataSource) : IResidentialCustomerRepository
{
    public async Task AddAsync(ResidentialCustomer customer, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand("CALL sp_residential_customer_create(@p_customer)");
        command.Parameters.AddWithValue("p_customer", NpgsqlDbType.Jsonb, RoutineCustomerMapper.Serialize(customer));
        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException("El código o documento ya existe");
        }
    }

    public Task<ResidentialCustomer?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        ReadOneAsync(id, cancellationToken);

    public Task<ResidentialCustomer?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
        ReadOneAsync(id, cancellationToken);

    public async Task<IReadOnlyList<ResidentialCustomer>> SearchAsync(SearchResidentialCustomerCriteria criteria,
        CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand(
            "SELECT result::text FROM fn_residential_customer_search_advanced(@p_search, @p_status, @p_neighborhood, @p_center, @p_stratum, @p_treatment, @p_document_type)");
        AddSearchParameters(command, criteria);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var customers = new List<ResidentialCustomer>();
        while (await reader.ReadAsync(cancellationToken))
            customers.Add(RoutineCustomerMapper.Deserialize(reader.GetString(0)));
        return customers;
    }

    public async Task<int> CountAsync(SearchResidentialCustomerCriteria criteria, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand(
            "SELECT fn_residential_customer_count_advanced(@p_search, @p_status, @p_neighborhood, @p_center, @p_stratum, @p_treatment, @p_document_type)");
        AddSearchParameters(command, criteria);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    private static void AddSearchParameters(NpgsqlCommand command, SearchResidentialCustomerCriteria criteria)
    {
        command.Parameters.Add(new NpgsqlParameter("p_search", NpgsqlDbType.Text)
            { Value = string.IsNullOrWhiteSpace(criteria.SearchText) ? DBNull.Value : criteria.SearchText.Trim() });
        command.Parameters.AddWithValue("p_status", NpgsqlDbType.Text, criteria.Status.ToString().ToLowerInvariant());
        command.Parameters.AddWithValue("p_neighborhood", NpgsqlDbType.Uuid, (object?)criteria.NeighborhoodId ?? DBNull.Value);
        command.Parameters.AddWithValue("p_center", NpgsqlDbType.Uuid, (object?)criteria.CenterId ?? DBNull.Value);
        command.Parameters.AddWithValue("p_stratum", NpgsqlDbType.Smallint, (object?)criteria.Stratum ?? DBNull.Value);
        command.Parameters.AddWithValue("p_treatment", NpgsqlDbType.Integer, criteria.Treatment.HasValue ? (object)(int)criteria.Treatment.Value : DBNull.Value);
        command.Parameters.AddWithValue("p_document_type", NpgsqlDbType.Integer, criteria.DocumentType.HasValue ? (object)(int)criteria.DocumentType.Value : DBNull.Value);
    }

    public async Task UpdateAsync(ResidentialCustomer customer, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand("CALL sp_residential_customer_update(@p_customer)");
        command.Parameters.AddWithValue("p_customer", NpgsqlDbType.Jsonb, RoutineCustomerMapper.SerializeUpdate(customer));
        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (PostgresException exception) when
            (exception.SqlState == "P0001" && exception.MessageText == "customer_not_updatable")
        {
            throw new DomainConflictException("El cliente está bloqueado y no puede modificarse.");
        }
    }

    public async Task RetireAsync(ResidentialCustomer customer, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand("CALL sp_residential_customer_retire(@p_id, @p_blocked_at, @p_updated_at)");
        command.Parameters.AddWithValue("p_id", customer.Id);
        command.Parameters.AddWithValue("p_blocked_at", customer.BlockedAt!.Value);
        command.Parameters.AddWithValue("p_updated_at", customer.UpdatedAt);
        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (PostgresException exception) when
            (exception.SqlState == "P0001" && exception.MessageText == "customer_already_blocked")
        {
            throw new DomainConflictException("El cliente ya se encuentra bloqueado.");
        }
    }

    private async Task<ResidentialCustomer?> ReadOneAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand(
            "SELECT result::text FROM fn_residential_customer_get(@p_id)");
        command.Parameters.AddWithValue("p_id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is string json ? RoutineCustomerMapper.Deserialize(json) : null;
    }
}

internal sealed class CenterRepository(NpgsqlDataSource dataSource) : ICenterRepository
{
    public async Task<bool> ExistsActiveAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand(
            "SELECT EXISTS (SELECT 1 FROM fn_centers_get() WHERE id = @p_id)");
        command.Parameters.AddWithValue("p_id", id);
        return (bool)(await command.ExecuteScalarAsync(cancellationToken) ?? false);
    }

    public async Task<IReadOnlyList<Center>> GetActiveAsync(CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand("SELECT id, code, name FROM fn_centers_get()");
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var centers = new List<Center>();
        while (await reader.ReadAsync(cancellationToken))
            centers.Add(new Center(reader.GetGuid(0), reader.GetString(1), reader.GetString(2)));
        return centers;
    }
}

internal sealed class NeighborhoodRepository(NpgsqlDataSource dataSource) : INeighborhoodRepository
{
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand(
            "SELECT EXISTS (SELECT 1 FROM fn_neighborhoods_search(NULL) WHERE id = @p_id)");
        command.Parameters.AddWithValue("p_id", id);
        return (bool)(await command.ExecuteScalarAsync(cancellationToken) ?? false);
    }

    public async Task<IReadOnlyList<NeighborhoodLookup>> SearchAsync(string? query, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand(
            "SELECT id, name, municipality, department, country, transport_zone FROM fn_neighborhoods_search(@p_query)");
        command.Parameters.Add(new NpgsqlParameter("p_query", NpgsqlDbType.Text)
            { Value = string.IsNullOrWhiteSpace(query) ? DBNull.Value : query.Trim() });
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var neighborhoods = new List<NeighborhoodLookup>();
        while (await reader.ReadAsync(cancellationToken))
            neighborhoods.Add(new NeighborhoodLookup(reader.GetGuid(0), reader.GetString(1), reader.GetString(2),
                reader.GetString(3), reader.GetString(4), reader.GetString(5)));
        return neighborhoods;
    }
}

internal sealed class ModernChannelCustomerRepository(NpgsqlDataSource dataSource) : IModernChannelCustomerRepository
{
    public async Task<bool> ExistsAsync(DocumentType documentType, string documentNumber, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand(
            "SELECT fn_modern_channel_customer_exists(@p_type, @p_number)");
        command.Parameters.AddWithValue("p_type", (int)documentType);
        command.Parameters.AddWithValue("p_number", documentNumber);
        return (bool)(await command.ExecuteScalarAsync(cancellationToken) ?? false);
    }
}

internal static class RoutineCustomerMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string Serialize(ResidentialCustomer customer) => JsonSerializer.Serialize(new
    {
        customer.Id,
        customer.Code,
        treatment = (int)customer.Treatment,
        customer.BusinessName,
        customer.ExtendedLegalName,
        customer.FullName,
        customer.FirstNames,
        customer.LastNames,
        customer.Phone,
        customer.PhoneExtension,
        customer.MobilePhone,
        customer.Email,
        documentType = (int)customer.DocumentType,
        customer.DocumentNumber,
        customer.VerificationDigit,
        taxClass = (int)customer.TaxClass,
        customer.PaymentCondition,
        customer.Stratum,
        customer.CenterId,
        customer.IsBlocked,
        customer.BlockedAt,
        customer.CreatedAt,
        customer.UpdatedAt,
        address = new
        {
            customer.Address.Id,
            customer.Address.NeighborhoodId,
            customer.Address.IsRural,
            customer.Address.RuralAddress,
            customer.Address.MainRoadType,
            customer.Address.MainRoadNumber,
            customer.Address.MainRoadLetter,
            customer.Address.MainRoadCardinality,
            customer.Address.SecondaryRoadNumber1,
            customer.Address.SecondaryRoadLetter,
            customer.Address.SecondaryRoadCardinality1,
            customer.Address.SecondaryRoadNumber2,
            customer.Address.SecondaryRoadCardinality2,
            customer.Address.FormattedAddress
        }
    }, JsonOptions);

    public static string SerializeUpdate(ResidentialCustomer customer) => JsonSerializer.Serialize(new
    {
        customer.Id,
        customer.BusinessName,
        customer.ExtendedLegalName,
        customer.FullName,
        customer.FirstNames,
        customer.LastNames,
        customer.Phone,
        customer.PhoneExtension,
        customer.MobilePhone,
        customer.Email,
        customer.Stratum,
        customer.CenterId,
        customer.UpdatedAt,
        address = new
        {
            customer.Address.NeighborhoodId,
            customer.Address.IsRural,
            customer.Address.RuralAddress,
            customer.Address.MainRoadType,
            customer.Address.MainRoadNumber,
            customer.Address.MainRoadLetter,
            customer.Address.MainRoadCardinality,
            customer.Address.SecondaryRoadNumber1,
            customer.Address.SecondaryRoadLetter,
            customer.Address.SecondaryRoadCardinality1,
            customer.Address.SecondaryRoadNumber2,
            customer.Address.SecondaryRoadCardinality2,
            customer.Address.FormattedAddress
        }
    }, JsonOptions);

    public static ResidentialCustomer Deserialize(string json)
    {
        var row = JsonSerializer.Deserialize<CustomerRoutineRow>(json, JsonOptions)
            ?? throw new InvalidOperationException("The customer routine returned an empty payload");
        var address = CustomerAddress.Restore(row.Address.Id, row.Id, row.Address.NeighborhoodId,
            row.Address.IsRural, row.Address.RuralAddress, row.Address.MainRoadType, row.Address.MainRoadNumber,
            row.Address.MainRoadLetter, row.Address.MainRoadCardinality, row.Address.SecondaryRoadNumber1,
            row.Address.SecondaryRoadLetter, row.Address.SecondaryRoadCardinality1,
            row.Address.SecondaryRoadNumber2, row.Address.SecondaryRoadCardinality2, row.Address.FormattedAddress);
        return ResidentialCustomer.Restore(row.Id, row.Code, row.Treatment, row.BusinessName,
            row.ExtendedLegalName, row.FullName, row.FirstNames, row.LastNames, row.Phone, row.PhoneExtension,
            row.MobilePhone, row.Email, row.DocumentType, row.DocumentNumber, row.VerificationDigit,
            row.TaxClass, row.PaymentCondition, row.Stratum, row.CenterId, row.IsBlocked, row.BlockedAt,
            row.CreatedAt, row.UpdatedAt, address);
    }

    private sealed record CustomerRoutineRow(Guid Id, string Code, Treatment Treatment, string BusinessName,
        string ExtendedLegalName, string FullName, string FirstNames, string LastNames, string? Phone,
        string? PhoneExtension, string? MobilePhone, string? Email, DocumentType DocumentType,
        string DocumentNumber, string? VerificationDigit, TaxClass TaxClass, string PaymentCondition,
        short Stratum, Guid CenterId, bool IsBlocked, DateTimeOffset? BlockedAt, DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt, AddressRoutineRow Address);

    private sealed record AddressRoutineRow(Guid Id, Guid NeighborhoodId, bool IsRural, string? RuralAddress,
        string? MainRoadType, string? MainRoadNumber, string? MainRoadLetter, string? MainRoadCardinality,
        string? SecondaryRoadNumber1, string? SecondaryRoadLetter, string? SecondaryRoadCardinality1,
        string? SecondaryRoadNumber2, string? SecondaryRoadCardinality2, string FormattedAddress);
}
