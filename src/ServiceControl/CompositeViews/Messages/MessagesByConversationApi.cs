namespace ServiceControl.CompositeViews.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Infrastructure.Extensions;
    using Raven.Client;
    using Raven.Client.Linq;
    using ServiceBus.Management.Infrastructure.Settings;

    class MessagesByConversationApi : ScatterGatherApiMessageView<string>
    {
        public MessagesByConversationApi(IDocumentStore documentStore, Settings settings, Func<HttpClient> httpClientFactory) : base(documentStore, settings, httpClientFactory)
        {
        }

        protected override async Task<QueryResult<IList<MessagesView>>> LocalQuery(HttpRequestMessage request, string input)
        {
            using (var session = Store.OpenAsyncSession())
            {
                var results = await session.Query<MessagesViewIndex.SortAndFilterOptions, MessagesViewIndex>()
                    .Statistics(out var stats)
                    .Where(m => m.ConversationId == input)
                    .Sort(request)
                    .Paging(request)
                    .TransformWith<MessagesViewTransformer, MessagesView>()
                    .ToListAsync()
                    .ConfigureAwait(false);

                return new QueryResult<IList<MessagesView>>(results, stats.ToQueryStatsInfo());
            }
        }
    }
}