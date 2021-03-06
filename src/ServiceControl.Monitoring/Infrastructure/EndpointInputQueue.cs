﻿namespace ServiceControl.Monitoring.Infrastructure
{
    using System;

    public readonly struct EndpointInputQueue : IEquatable<EndpointInputQueue>
    {
        public EndpointInputQueue(string endpointName, string inputQueue)
        {
            EndpointName = endpointName;
            InputQueue = inputQueue;
        }

        public string EndpointName { get; }
        public string InputQueue { get; }

        public bool Equals(EndpointInputQueue other) => string.Equals(EndpointName, other.EndpointName) && string.Equals(InputQueue, other.InputQueue);

        public override bool Equals(object obj) => obj is EndpointInputQueue inputQueue && Equals(inputQueue);

        public override int GetHashCode() => (EndpointName, InputQueue).GetHashCode();
    }
}