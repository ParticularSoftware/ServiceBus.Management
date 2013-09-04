namespace ServiceControl.HeartbeatMonitoring
{
    using ServiceBus.Management.Infrastructure.Nancy.Modules;
    using Nancy;
    using System.Linq;

    public class ApiModule : BaseModule
    {
        public HeartbeatMonitor HeartbeatMonitor { get; set; }

        public ApiModule()
        {
            Get["/heartbeats/stats"] = _ =>
            {
                var endpointStatus = HeartbeatMonitor.CurrentStatus();

                return Negotiate.WithModel(new
                {
                    ActiveEndpoints = endpointStatus.Count(s => s.Failing.HasValue && !s.Failing.Value),
                    NumberOfFailingEndpoints = endpointStatus.Count(s => s.Failing.HasValue && s.Failing.Value)
                }).WithHeader("Cache-Control", "private, max-age=0, must-revalidate");
            };

            Get["/heartbeats"] = _ =>
            {
                var endpointStatus = HeartbeatMonitor.CurrentStatus();

                return Negotiate.WithModel(endpointStatus)
                    .WithHeader("Cache-Control", "private, max-age=0, must-revalidate");
            };
        }
    }
}