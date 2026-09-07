using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DataGo.Application;
using Microsoft.Extensions.DependencyInjection;

namespace DataGo.Infrastructure;

public sealed record OpenRouterOptions(
    string? ApiKey,
    string BaseUrl = "https://openrouter.ai/api/v1",
    string Model = "openai/gpt-4o-mini",
    int TimeoutSeconds = 30);

public static class AssistantDependencyInjection
{
    public static IServiceCollection AddAssistantInfrastructure(this IServiceCollection services, OpenRouterOptions options)
    {
        services.AddSingleton(options);
        services.AddSingleton<IPendingAssistantActions, MemoryPendingAssistantActions>();
        services.AddHttpClient<IAssistantModel, OpenRouterAssistantModel>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 5, 120));
        });
        return services;
    }
}

internal sealed class MemoryPendingAssistantActions(TimeProvider timeProvider) : IPendingAssistantActions
{
    private readonly ConcurrentDictionary<string, StoredAssistantAction> actions = new();

    public void Add(StoredAssistantAction action)
    {
        foreach (var item in actions.Where(x => x.Value.Preview.ExpiresAt <= timeProvider.GetUtcNow()).ToArray())
            actions.TryRemove(item.Key, out _);
        actions[action.Preview.Token] = action;
    }

    public StoredAssistantAction? Take(string token)
    {
        if (!actions.TryRemove(token, out var action) || action.Preview.ExpiresAt <= timeProvider.GetUtcNow()) return null;
        return action;
    }
}

internal sealed class OpenRouterAssistantModel(HttpClient httpClient, OpenRouterOptions options) : IAssistantModel
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private const string SystemPrompt = """
        Eres un parser de intención de DataGo. Responde exclusivamente JSON válido según el schema.
        No inventes clientes, IDs ni datos faltantes. No escribas SQL ni decidas reglas de negocio.
        Solo extrae intent, filters, customerReference y fields. Las mutaciones no se ejecutan aquí.
        Usa SEARCH_CUSTOMERS, COUNT_CUSTOMERS, GET_CUSTOMER, CREATE_CUSTOMER, UPDATE_CUSTOMER,
        RETIRE_CUSTOMER, HELP o UNKNOWN. Marca requiresClarification cuando falte información.
        Para fields usa nombres: mobilePhone, email, stratum, businessName.
        Una ubicación expresada como "de Aranjuez", "en Laureles" o "de El Poblado" va en
        filters.neighborhood; un centro expresado como "del centro Medellín Norte" va en filters.center.
        No copies esas ubicaciones a searchText. "Empresas" corresponde a filters.treatment = "Empresa".
        "activos" y "bloqueados" corresponden a filters.status.
        "mis clientes" significa todos los clientes residenciales de DataGo.
        "este cliente" puede usar el customerId del contexto sin inventar otra referencia.
        "solo activos/bloqueados" conserva filtros del contexto y cambia status.
        """;

    public async Task<AssistantInterpretation> InterpretAsync(AssistantModelRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new AssistantProviderException("not_configured", "El asistente IA no está configurado.");

        for (var attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                using var message = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
                message.Content = JsonContent.Create(BuildPayload(request, attempt > 0));
                using var response = await httpClient.SendAsync(message, cancellationToken);
                if (!response.IsSuccessStatusCode)
                    throw await CreateProviderExceptionAsync(response, cancellationToken);
                var envelope = await response.Content.ReadFromJsonAsync<OpenRouterResponse>(JsonOptions, cancellationToken);
                var content = envelope?.Choices?.FirstOrDefault()?.Message?.Content;
                if (TryParse(content, out var interpretation)) return interpretation!;
            }
            catch (AssistantProviderException) { throw; }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new AssistantProviderException("unavailable", "El proveedor de IA agotó el tiempo de respuesta.");
            }
            catch (HttpRequestException)
            {
                throw new AssistantProviderException("unavailable", "El proveedor de IA no está disponible temporalmente.");
            }
        }
        throw new AssistantProviderException("invalid_response", "El proveedor de IA devolvió una respuesta inválida.");
    }

    private object BuildPayload(AssistantModelRequest request, bool retry) => new
    {
        model = options.Model,
        temperature = 0,
        max_tokens = 600,
        messages = new object[]
        {
            new { role = "system", content = SystemPrompt + (retry ? " Corrige la salida anterior: devuelve únicamente el JSON del schema." : "") },
            new { role = "user", content = JsonSerializer.Serialize(new { request.Message, request.Context }, JsonOptions) }
        },
        response_format = new
        {
            type = "json_schema",
            json_schema = new { name = "datago_assistant_interpretation", strict = true, schema = Schema }
        }
    };

    private static readonly object Schema = new
    {
        type = "object",
        additionalProperties = false,
        properties = new
        {
            intent = new { type = "string", @enum = Enum.GetNames<AssistantIntent>() },
            filters = NullableObject(new Dictionary<string, object>
            {
                ["searchText"] = NullableString(), ["status"] = NullableString(), ["neighborhood"] = NullableString(),
                ["center"] = NullableString(), ["stratum"] = new { type = new[] { "integer", "null" } },
                ["treatment"] = NullableString(), ["documentType"] = NullableString()
            }),
            customerReference = NullableObject(new Dictionary<string, object>
                { ["code"] = NullableString(), ["documentNumber"] = NullableString(), ["name"] = NullableString() }),
            fields = new { type = new[] { "array", "null" }, items = new { type = "object", additionalProperties = false,
                properties = new { field = new { type = "string" }, value = NullableString() }, required = new[] { "field", "value" } } },
            requiresClarification = new { type = "boolean" },
            clarification = NullableString()
        },
        required = new[] { "intent", "filters", "customerReference", "fields", "requiresClarification", "clarification" }
    };

    private static object NullableString() => new { type = new[] { "string", "null" } };
    private static object NullableObject(Dictionary<string, object> properties) => new
        { type = new[] { "object", "null" }, additionalProperties = false, properties, required = properties.Keys.ToArray() };

    private static bool TryParse(string? content, out AssistantInterpretation? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(content)) return false;
        try
        {
            result = JsonSerializer.Deserialize<AssistantInterpretation>(content, JsonOptions);
            return result is not null && Enum.IsDefined(result.Intent);
        }
        catch (JsonException) { return false; }
    }

    private static async Task<AssistantProviderException> CreateProviderExceptionAsync(
        HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var status = (int)response.StatusCode;
        var providerMessage = "";
        try
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var json = JsonDocument.Parse(body);
            providerMessage = json.RootElement.TryGetProperty("error", out var error) &&
                error.TryGetProperty("message", out var message) ? message.GetString() ?? "" : "";
        }
        catch (JsonException) { }

        var suffix = string.IsNullOrWhiteSpace(providerMessage)
            ? $" (HTTP {status})."
            : $" (HTTP {status}: {providerMessage[..Math.Min(providerMessage.Length, 240)]}).";
        return status switch
        {
            401 or 403 => new("invalid_credentials", "OpenRouter rechazó la credencial configurada" + suffix),
            402 => new("quota_exhausted", "OpenRouter no tiene crédito disponible" + suffix),
            429 => new("rate_limited", "OpenRouter limitó temporalmente las solicitudes" + suffix),
            400 or 404 or 422 => new("invalid_request", "OpenRouter rechazó la configuración de la solicitud" + suffix),
            _ => new("unavailable", "OpenRouter no está disponible temporalmente" + suffix)
        };
    }

    private sealed record OpenRouterResponse(IReadOnlyList<Choice>? Choices);
    private sealed record Choice(Message? Message);
    private sealed record Message(string? Content);
}
