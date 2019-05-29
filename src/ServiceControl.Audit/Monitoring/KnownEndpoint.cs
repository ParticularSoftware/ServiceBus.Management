﻿namespace ServiceControl.Audit.Monitoring
{
    using System;

    public class KnownEndpoint
    {
        public Guid Id { get; set; }
        public string HostDisplayName { get; set; }
        public bool Monitored { get; set; }
        public EndpointDetails EndpointDetails { get; set; }
        public bool HasTemporaryId { get; set; }
    }
}