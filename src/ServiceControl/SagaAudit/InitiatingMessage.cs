namespace ServiceControl.SagaAudit
{
    using System;

    public class InitiatingMessage
    {
        public string InitiatingMessageId { get; set; }
        public bool IsSagaTimeoutMessage { get; set; }
        public string OriginatingEndpoint { get; set; }
        public string OriginatingMachine { get; set; }
        public DateTime TimeSent { get; set; }
        public string MessageType { get; set; }
    }
}