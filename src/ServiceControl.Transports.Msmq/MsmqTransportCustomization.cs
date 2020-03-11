﻿namespace ServiceControl.Transports.Msmq
{
    using NServiceBus;
    using NServiceBus.Raw;

    public class MsmqTransportCustomization : TransportCustomizationBase
    {
        public override void CustomizeEndpoint(EndpointConfiguration endpointConfig, TransportSettings transportSettings)
        {
            var transport = endpointConfig.UseTransport<MsmqTransport>();
            transport.Transactions(TransportTransactionMode.SendsAtomicWithReceive);
        }

        public override void CustomizeRawEndpoint(RawEndpointConfiguration endpointConfig, TransportSettings transportSettings)
        {
            var transport = endpointConfig.UseTransport<MsmqTransport>();
            transport.Transactions(TransportTransactionMode.SendsAtomicWithReceive);
        }

        public override IProvideQueueLength CreateQueueLengthProvider()
        {
            return new QueueLengthProvider();
        }
    }
}