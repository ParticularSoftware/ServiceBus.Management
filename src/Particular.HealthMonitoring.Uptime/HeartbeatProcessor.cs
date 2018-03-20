﻿namespace Particular.HealthMonitoring.Uptime
{
    using System.Threading.Tasks;
    using Particular.Operations.Heartbeats.Api;

    class HeartbeatProcessor : IProcessHeartbeats
    {
        EndpointInstanceMonitoring monitoring;

        public HeartbeatProcessor(EndpointInstanceMonitoring monitoring)
        {
            this.monitoring = monitoring;
        }

        public Task Handle(RegisterEndpointStartup endpointStartup)
        {
            return monitoring.EndpointDetected(endpointStartup.Endpoint, endpointStartup.Host, endpointStartup.HostId);
        }

        public Task Handle(EndpointHeartbeat heartbeat)
        {
            monitoring.RecordHeartbeat(heartbeat.EndpointName, heartbeat.Host, heartbeat.HostId, heartbeat.ExecutedAt);
            return Task.FromResult(0);
        }
    }
}