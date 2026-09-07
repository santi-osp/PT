using DataGo.Application;
using Microsoft.AspNetCore.Mvc;

namespace DataGo.Api.Controllers;

[ApiController]
[Route("api/catalogs")]
public sealed class CatalogsController(GetCentersHandler centers, SearchNeighborhoodsHandler neighborhoods) : ControllerBase
{
    [HttpGet("centers")]
    public Task<IReadOnlyList<CenterResponse>> GetCenters(CancellationToken cancellationToken) => centers.HandleAsync(cancellationToken);

    [HttpGet("neighborhoods")]
    public Task<IReadOnlyList<NeighborhoodResponse>> GetNeighborhoods([FromQuery] string? query, CancellationToken cancellationToken) =>
        neighborhoods.HandleAsync(query, cancellationToken);
}
