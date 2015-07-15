namespace ServiceControl.Recoverability
{
    using System.Collections.Generic;

    public class RetryBatch
    {
        public string Id { get; set; }
        public string RetrySessionId { get; set; }
        public RetryBatchStatus Status { get; set; }
        public IList<string> FailureRetries { get; set; }

        public RetryBatch()
        {
            FailureRetries = new List<string>();
        }

        public static string MakeDocumentId(string messageUniqueId)
        {
            return "RetryBatches/" + messageUniqueId;
        }
    }
}