using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace NosMoon.Module.Bazaar.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBazaarCore(this IServiceCollection services)
        {
            Console.WriteLine("Initializing bazaar module");
            services.AddMediatR(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}
