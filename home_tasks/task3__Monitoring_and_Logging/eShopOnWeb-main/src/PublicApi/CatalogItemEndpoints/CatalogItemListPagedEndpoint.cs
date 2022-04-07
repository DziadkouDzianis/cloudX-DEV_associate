using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.Extensions.Logging;
using MinimalApi.Endpoint;

namespace Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

/// <summary>
/// List Catalog Items (paged)
/// </summary>
public class CatalogItemListPagedEndpoint : IEndpoint<IResult, ListPagedCatalogItemRequest>
{
    private IRepository<CatalogItem> _itemRepository;
    private readonly IUriComposer _uriComposer;
    private readonly IMapper _mapper;
    private readonly ILogger<CatalogItemListPagedEndpoint> _logger;
    //private readonly TelemetryClient _telemetryClient;

    public CatalogItemListPagedEndpoint(IUriComposer uriComposer, IMapper mapper, ILogger<CatalogItemListPagedEndpoint> logger/*, TelemetryClient telemetryClient*/)
    {
        _uriComposer = uriComposer;
        _mapper = mapper;
        _logger = logger;
        //_telemetryClient = telemetryClient;
    }

    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapGet("api/catalog-items",
            async (int? pageSize, int? pageIndex, int? catalogBrandId, int? catalogTypeId, IRepository<CatalogItem> itemRepository) =>
            {
                _itemRepository = itemRepository;
                return await HandleAsync(new ListPagedCatalogItemRequest(pageSize, pageIndex, catalogBrandId, catalogTypeId));
            })
            .Produces<ListPagedCatalogItemResponse>()
            .WithTags("CatalogItemEndpoints");
    }

    public async Task<IResult> HandleAsync(ListPagedCatalogItemRequest request)
    {
        var response = new ListPagedCatalogItemResponse(request.CorrelationId());

        var filterSpec = new CatalogFilterSpecification(request.CatalogBrandId, request.CatalogTypeId);
        int totalItems = await _itemRepository.CountAsync(filterSpec);

        var pagedSpec = new CatalogFilterPaginatedSpecification(
            skip: request.PageIndex.Value * request.PageSize.Value,
            take: request.PageSize.Value,
            brandId: request.CatalogBrandId,
            typeId: request.CatalogTypeId);

        var items = await _itemRepository.ListAsync(pagedSpec);

        response.CatalogItems.AddRange(items.Select(_mapper.Map<CatalogItemDto>));
        foreach (CatalogItemDto item in response.CatalogItems)
        {
            item.PictureUri = _uriComposer.ComposePicUri(item.PictureUri);
        }

        if (request.PageSize > 0)
        {
            response.PageCount = int.Parse(Math.Ceiling((decimal)totalItems / request.PageSize.Value).ToString());
        }
        else
        {
            response.PageCount = totalItems > 0 ? 1 : 0;
        }

        _logger.LogWarning("Warning test Dzianis");
        _logger.LogError("Error test Dzianis");
        _logger.LogInformation($"test catalog items: {items.Count}");

        if (items.Count > 0)
        {
            try
            {
                throw new Exception("Cannot move further");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot move further. Catch block");
            }
            //_telemetryClient.TrackException(new Exception("Cannot move further"));
            //_telemetryClient.TrackException(new ExceptionTelemetry(new Exception("Cannot move further")));
        }
        return Results.Ok(response);
    }
}
