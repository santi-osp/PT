using DataGo.Application;
using Microsoft.AspNetCore.Mvc;

namespace DataGo.Api.Controllers;

[ApiController]
[Route("api/assistant")]
public sealed class AssistantController(
    AssistantMessageHandler messages,
    ConfirmAssistantActionHandler confirmations) : ControllerBase
{
    [HttpPost("message")]
    public Task<AssistantMessageResponse> Message(AssistantMessageRequest request, CancellationToken cancellationToken) =>
        messages.HandleAsync(request, cancellationToken);

    [HttpPost("confirm")]
    public Task<AssistantMessageResponse> Confirm(AssistantConfirmRequest request, CancellationToken cancellationToken) =>
        confirmations.HandleAsync(request, cancellationToken);
}
