

using CleanAspire.Application.Common.Interfaces.FusionCache;

namespace CleanAspire.Application.Pipeline;

public class FusionCacheRefreshBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IFusionCacheRefreshRequest<TResponse>
{
    private readonly IFusionCache _cache;
    private readonly ILogger<FusionCacheRefreshBehaviour<TRequest, TResponse>> _logger;

    public FusionCacheRefreshBehaviour(
        IFusionCache cache,
        ILogger<FusionCacheRefreshBehaviour<TRequest, TResponse>> logger
    )
    {
        _cache = cache;
        _logger = logger;
    }

    public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogTrace("Handling request of type {RequestType} with details {@Request}", nameof(request), request);
        var response = await next(request, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(request.CacheKey))
        {
            await _cache.RemoveAsync(request.CacheKey);
            _logger.LogTrace("Cache key {CacheKey} removed from cache", request.CacheKey);
        }
        if (request.Tags != null && request.Tags.Any())
        {
            foreach (var tag in request.Tags)
            {
                await _cache.RemoveByTagAsync(tag);
                _logger.LogTrace("Cache tag {CacheTag} removed from cache", tag);
            }
        }
        return response;
    }
}
