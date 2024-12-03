// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace CleanAspire.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(FusionCacheBehaviour<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(FusionCacheRefreshBehaviour<,>));
        services.AddMediator(options=>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });
        return services;
    }
}

