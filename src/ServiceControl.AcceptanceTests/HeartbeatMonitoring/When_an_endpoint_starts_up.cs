﻿namespace ServiceBus.Management.AcceptanceTests.HeartbeatMonitoring
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Contexts;
    using NServiceBus;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.Configuration.AdvancedExtensibility;
    using NUnit.Framework;
    using ServiceBus.Management.Infrastructure.Settings;
    using ServiceControl.Contracts.EndpointControl;
    using ServiceControl.EventLog;

    public class When_an_endpoint_starts_up : AcceptanceTest
    {
        static Guid hostIdentifier = Guid.NewGuid();
        
        [Test]
        public async Task Should_result_in_a_startup_event()
        {
            EventLogItem entry = null;

            await Define<ScenarioContext>()
                .WithEndpoint<StartingEndpoint>()
                .Done(async c =>
                {
                    var result = await TryGetSingle<EventLogItem>("/api/eventlogitems/", e => e.RelatedTo.Any(r => r.Contains(typeof(StartingEndpoint).Name)) && e.EventType == typeof(EndpointStarted).Name);
                    entry = result;
                    return result;
                })
                .Run();

            Assert.AreEqual(Severity.Info, entry.Severity, "Endpoint startup should be treated as info");
            Assert.IsTrue(entry.RelatedTo.Any(item => item == "/host/" + hostIdentifier));
        }

        public class StartingEndpoint : EndpointConfigurationBuilder
        {
            public StartingEndpoint()
            {
                EndpointSetup<DefaultServerWithoutAudit>(c =>
                {
                    c.SendHeartbeatTo(Settings.DEFAULT_SERVICE_NAME);
                    
                    c.GetSettings().Set("ServiceControl.CustomHostIdentifier", hostIdentifier);
                    c.UniquelyIdentifyRunningInstance().UsingCustomIdentifier(hostIdentifier);
                });
            }
        }
    }
}