﻿namespace ServiceControl.Recoverability
{
    using System.Collections.Generic;
    using System.Linq;

    public class RetryHistory : IOperationHistory<HistoricRetryOperation>
    {
        public string Id { get; set; }
        public List<HistoricRetryOperation> HistoricOperations { get; set; }
        public List<UnacknowledgedOperation> UnacknowledgedOperations { get; set; }

        public static string MakeId()
        {
            return "RetryOperations/History";
        }

        public static RetryHistory CreateNew()
        {
            return new RetryHistory
            {
                HistoricOperations = new List<HistoricRetryOperation>(),
                UnacknowledgedOperations = new List<UnacknowledgedOperation>(),
                Id = MakeId()
            };
        }

        public void AddToHistory(HistoricRetryOperation historicOperation, int historyDepth)
        {
            HistoricOperations = HistoricOperations.Union(new[]
                {
                    historicOperation
                })
                .OrderByDescending(retry => retry.CompletionTime)
                .Take(historyDepth)
                .ToList();
        }

        public string GetHistoryOperationsUniqueIdentifier()
        {
            return string.Join(",", HistoricOperations.Select(x => x.RequestId));
        }

        public void AddToUnacknowledged(UnacknowledgedOperation unacknowledgedRetryOperation)
        {
            UnacknowledgedOperations.Add(unacknowledgedRetryOperation);
        }

        public UnacknowledgedOperation[] GetUnacknowledgedByClassifier(string classifier)
        {
            return UnacknowledgedOperations.Where(x => x.Classifier == classifier).ToArray();
        }
        
        public void Acknowledge(string requestId, RetryType type)
        {
            UnacknowledgedOperations.RemoveAll(x => x.RequestId == requestId && (RetryType)x.OperationType == type);
        }
    }
}