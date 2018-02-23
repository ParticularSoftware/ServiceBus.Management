﻿namespace NServiceBus.AcceptanceTesting.Support
{
    using System;
    using System.Collections.Generic;

    public class EndpointConfiguration
    {
        public EndpointConfiguration()
        {
            UserDefinedConfigSections = new Dictionary<Type, object>();
            TypesToExclude = new List<Type>();
            TypesToInclude = new List<Type>();
            GetBus = () => null;
            StopBus = null;
        }

        public IDictionary<Type, Type> EndpointMappings { get; set; }

        public List<Type> TypesToExclude { get; set; }

        public List<Type> TypesToInclude { get; set; }

        public Func<RunDescriptor, IDictionary<Type, string>, BusConfiguration> GetConfiguration { get; set; }

        internal Func<IStartableBus> GetBus { get; set; }

        internal Action StopBus { get; set; }

        public string EndpointName
        {
            get
            {
                if (!string.IsNullOrEmpty(CustomEndpointName))
                {
                    return CustomEndpointName;
                }
                return endpointName;
            }
            set { endpointName = value; }
        }

        public Type BuilderType { get; set; }

        public Address AddressOfAuditQueue { get; set; }

        public IDictionary<Type, object> UserDefinedConfigSections { get; }

        public string CustomMachineName { get; set; }

        public string CustomEndpointName { get; set; }

        public Type AuditEndpoint { get; set; }
        public bool SendOnly { get; set; }

        public void SelfHost(Func<IStartableBus> getBus, Action stopBus)
        {
            GetBus = getBus;
            StopBus = stopBus;
        }

        string endpointName;
    }
}