namespace ServiceControl.Operations
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    interface IEnrichImportedAuditMessages
    {
        Task Enrich(IReadOnlyDictionary<string, string> headers, IDictionary<string, object> metadata);
    }
}