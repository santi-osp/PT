using DataGo.Application;
using Microsoft.AspNetCore.Mvc;

namespace DataGo.Api.Controllers;

[ApiController]
[Route("api/residential-customers")]
public sealed class ResidentialCustomersController(
    CreateResidentialCustomerHandler createHandler,
    GetResidentialCustomerHandler getHandler,
    SearchResidentialCustomersHandler searchHandler) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ResidentialCustomerResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateResidentialCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await createHandler.HandleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpGet]
    public Task<IReadOnlyList<ResidentialCustomerResponse>> Search([FromQuery] string? search, CancellationToken cancellationToken) =>
        searchHandler.HandleAsync(search, cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<ResidentialCustomerResponse> Get(Guid id, CancellationToken cancellationToken) =>
        getHandler.HandleAsync(id, cancellationToken);
}
