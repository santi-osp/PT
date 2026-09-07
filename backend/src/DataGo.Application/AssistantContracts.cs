namespace DataGo.Application;

public enum AssistantIntent
{
    HELP, SEARCH_CUSTOMERS, COUNT_CUSTOMERS, GET_CUSTOMER,
    CREATE_CUSTOMER, UPDATE_CUSTOMER, RETIRE_CUSTOMER, UNKNOWN
}

public enum AssistantResponseType { Text, CustomerList, CustomerDetail, Count, Confirmation, Clarification, Error }

// The provider receives language and conversation context, never database records.
public sealed record AssistantModelRequest(string Message, AssistantConversationContext? Context = null);
public interface IAssistantModel
{
    Task<AssistantInterpretation> InterpretAsync(AssistantModelRequest request, CancellationToken cancellationToken);
}

public sealed record AssistantFilters(
    string? SearchText = null, string? Status = null, string? Neighborhood = null,
    string? Center = null, short? Stratum = null, string? Treatment = null, string? DocumentType = null);

public sealed record AssistantCustomerReference(string? Code = null, string? DocumentNumber = null, string? Name = null);

// Presence is explicit: absent differs from clearing an optional field.
public sealed record AssistantFieldChange(string Field, string? Value);
public sealed record AssistantInterpretation(
    AssistantIntent Intent,
    AssistantFilters? Filters = null,
    AssistantCustomerReference? CustomerReference = null,
    IReadOnlyList<AssistantFieldChange>? Fields = null,
    bool RequiresClarification = false,
    string? Clarification = null);

public sealed record AssistantMessageRequest(string Message, AssistantConversationContext? Context = null);
public sealed record AssistantConversationContext(Guid? CustomerId = null, AssistantFilters? Filters = null);
public sealed record AssistantChangePreview(string Field, string? PreviousValue, string? ProposedValue);
public sealed record PendingAssistantAction(
    string Token, AssistantIntent ActionType, Guid TargetCustomerId, string CustomerCode,
    string CustomerName, string Document, string Summary,
    IReadOnlyList<AssistantChangePreview> Changes, DateTimeOffset ExpiresAt);
public sealed record AssistantConfirmRequest(string Token);
public sealed record AssistantCustomerCard(ResidentialCustomerResponse Customer, string Neighborhood);
public sealed record AssistantMessageResponse(
    AssistantResponseType ResponseType,
    string Message,
    IReadOnlyList<AssistantCustomerCard>? Customers = null,
    int? Count = null,
    bool IsTruncated = false,
    PendingAssistantAction? PendingAction = null,
    AssistantConversationContext? Context = null,
    bool OpenCreateForm = false);

public sealed class AssistantProviderException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

// Server-owned proposals. Confirmation accepts an opaque token, never a client payload.
public sealed record StoredAssistantAction(
    PendingAssistantAction Preview, UpdateResidentialCustomerRequest? Update,
    DateTimeOffset ExpectedUpdatedAt);
public interface IPendingAssistantActions
{
    void Add(StoredAssistantAction action);
    StoredAssistantAction? Take(string token);
}
