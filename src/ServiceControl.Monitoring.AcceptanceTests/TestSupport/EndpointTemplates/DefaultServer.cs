﻿namespace NServiceBus.AcceptanceTests.EndpointTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using NServiceBus;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using NServiceBus.AcceptanceTesting.Support;
    using Configuration.AdvancedExtensibility;
    using Features;
    using ServiceBus.Management.AcceptanceTests;
    using ServiceControl.Monitoring;

    public class DefaultServer : IEndpointSetupTemplate
    {
        public async Task<EndpointConfiguration> GetConfiguration(RunDescriptor runDescriptor, EndpointCustomizationConfiguration endpointConfiguration, Action<EndpointConfiguration> configurationBuilderCustomization)
        {
            ServicePointManager.DefaultConnectionLimit = 100;

            var typesToInclude = new List<Type>();

            var builder = new EndpointConfiguration(endpointConfiguration.EndpointName);
            typesToInclude.AddRange(endpointConfiguration.GetTypesScopedByTestClass<Bootstrapper>().Concat(new[]
            {
                typeof(TraceIncomingBehavior),
                typeof(TraceOutgoingBehavior)
            }));

            builder.Pipeline.Register(new StampDispatchBehavior(runDescriptor.ScenarioContext), "Stamps outgoing messages with session ID");
            builder.Pipeline.Register(new DiscardMessagesBehavior(runDescriptor.ScenarioContext), "Discards messages based on session ID");

            builder.SendFailedMessagesTo("error");

            builder.TypesToIncludeInScan(typesToInclude);
            builder.EnableInstallers();

            await builder.DefineTransport(runDescriptor, endpointConfiguration).ConfigureAwait(false);
            builder.RegisterComponentsAndInheritanceHierarchy(runDescriptor);
            await builder.DefinePersistence(runDescriptor, endpointConfiguration).ConfigureAwait(false);

            typeof(ScenarioContext).GetProperty("CurrentEndpoint", BindingFlags.Static | BindingFlags.NonPublic).SetValue(runDescriptor.ScenarioContext, endpointConfiguration.EndpointName);

            
            builder.DisableFeature<Audit>();

            // will work on all the cloud transports
            builder.UseSerialization<NewtonsoftSerializer>();
            builder.DisableFeature<AutoSubscribe>();
            builder.Conventions().DefiningEventsAs(t => typeof(IEvent).IsAssignableFrom(t) || IsExternalContract(t));
            
            builder.RegisterComponents(r => { builder.GetSettings().Set("SC.ConfigureComponent", r); });
            builder.Pipeline.Register<TraceIncomingBehavior.Registration>();
            builder.Pipeline.Register<TraceOutgoingBehavior.Registration>();

            builder.GetSettings().Set("SC.ScenarioContext", runDescriptor.ScenarioContext);

            configurationBuilderCustomization(builder);

            return builder;
        }

        static bool IsExternalContract(Type t)
        {
            return t.Namespace != null && t.Namespace.StartsWith("ServiceControl.Contracts")
                                       && t.Assembly.GetName().Name == "ServiceControl.Contracts";
        }
}
}