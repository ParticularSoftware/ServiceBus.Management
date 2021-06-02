﻿namespace ServiceControl.Infrastructure.WebApi
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http.Controllers;
    using Autofac;
    using Autofac.Core.Activators.Reflection;
    using Autofac.Features.ResolveAnything;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using ServiceBus.Management.Infrastructure.OWIN;
    using ApisModule = MessageFailures.Api.ApisModule;

    static class WebApiHostBuilderExtensions
    {
        public static IHostBuilder UseWebApi(this IHostBuilder hostBuilder,
            List<Action<ContainerBuilder>> registrations, List<Assembly> apiAssemblies, string rootUrl, bool startOwinHost)
        {
            foreach (var apiAssembly in apiAssemblies)
            {
                registrations.Add(cb => RegisterAssemblyInternalWebApiControllers(cb, apiAssembly));
                registrations.Add(cb => cb.RegisterModule(new ApisModule(apiAssembly)));
                registrations.Add(cb => cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource(type => type.Assembly == apiAssembly && type.GetInterfaces().Any() == false)));
            }

            Startup startup = null;

            registrations.Add(cb =>
            {
                cb.RegisterBuildCallback(c => { startup = new Startup(c, apiAssemblies); });
            });

            if (startOwinHost)
            {
                hostBuilder.ConfigureServices((ctx, serviceCollection) =>
                {
                    serviceCollection.AddHostedService(sp => new WebApiHostedService(rootUrl, startup));
                });
            }

            return hostBuilder;
        }

        static void RegisterAssemblyInternalWebApiControllers(ContainerBuilder containerBuilder, Assembly assembly)
        {
            var controllerTypes = assembly.DefinedTypes
                .Where(t => typeof(IHttpController).IsAssignableFrom(t) && t.Name.EndsWith("Controller", StringComparison.Ordinal));

            foreach (var controllerType in controllerTypes)
            {
                containerBuilder.RegisterType(controllerType).FindConstructorsWith(new AllConstructorFinder());
            }
        }

        class AllConstructorFinder : IConstructorFinder
        {
            public ConstructorInfo[] FindConstructors(Type targetType)
            {
                var result = Cache.GetOrAdd(targetType, t => t.GetTypeInfo().DeclaredConstructors.ToArray());

                return result.Length > 0 ? result : throw new Exception($"No constructor found for type {targetType.FullName}");
            }

            static readonly ConcurrentDictionary<Type, ConstructorInfo[]> Cache = new ConcurrentDictionary<Type, ConstructorInfo[]>();
        }
    }
}
