namespace DataGo.Application;

public sealed class GetCentersHandler(ICenterRepository centers)
{
    public async Task<IReadOnlyList<CenterResponse>> HandleAsync(CancellationToken cancellationToken) =>
        (await centers.GetActiveAsync(cancellationToken)).Select(x => new CenterResponse(x.Id, x.Code, x.Name)).ToList();
}

public sealed class SearchNeighborhoodsHandler(INeighborhoodRepository neighborhoods)
{
    public async Task<IReadOnlyList<NeighborhoodResponse>> HandleAsync(string? query, CancellationToken cancellationToken) =>
        (await neighborhoods.SearchAsync(query, cancellationToken)).Select(x => new NeighborhoodResponse(x.Id, x.Name)).ToList();
}
