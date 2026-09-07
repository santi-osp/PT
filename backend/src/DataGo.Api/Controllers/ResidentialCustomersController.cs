using DataGo.Application;
using DataGo.Domain;
using Microsoft.AspNetCore.Mvc;

namespace DataGo.Api.Controllers;

[ApiController]
[Route("api/residential-customers")]
public sealed class ResidentialCustomersController(
    CreateResidentialCustomerHandler createHandler,
    GetResidentialCustomerHandler getHandler,
    SearchResidentialCustomersHandler searchHandler,
    UpdateResidentialCustomerHandler updateHandler,
    RetireResidentialCustomerHandler retireHandler) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ResidentialCustomerResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateResidentialCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await createHandler.HandleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpGet]
    public Task<IReadOnlyList<ResidentialCustomerResponse>> Search([FromQuery] string? search,
        [FromQuery] CustomerStatusFilter status = CustomerStatusFilter.All,
        [FromQuery] Guid? neighborhoodId = null, [FromQuery] Guid? centerId = null,
        [FromQuery] short? stratum = null, [FromQuery] Treatment? treatment = null,
        [FromQuery] DocumentType? documentType = null, CancellationToken cancellationToken = default) =>
        searchHandler.HandleAsync(new(search, status, neighborhoodId, centerId, stratum, treatment, documentType), cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<ResidentialCustomerResponse> Get(Guid id, CancellationToken cancellationToken) =>
        getHandler.HandleAsync(id, cancellationToken);

    [HttpPut("{id:guid}")]
    public Task<ResidentialCustomerResponse> Update(Guid id, UpdateResidentialCustomerRequest request, CancellationToken cancellationToken) =>
        updateHandler.HandleAsync(id, request, cancellationToken);

    [HttpPatch("{id:guid}/retire")]
    public Task<ResidentialCustomerResponse> Retire(Guid id, CancellationToken cancellationToken) =>
        retireHandler.HandleAsync(id, cancellationToken);
}
