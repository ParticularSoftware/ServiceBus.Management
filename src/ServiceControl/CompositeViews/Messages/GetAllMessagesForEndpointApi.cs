namespace ServiceControl.CompositeViews.Messages
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Nancy;

    public class GetAllMessagesForEndpointApi : ScatterGatherApiMessageView<string>
    {
        public override async Task<QueryResult<List<MessagesView>>> LocalQuery(Request request, string input)
        {
            using (var session = Store.OpenAsyncSession())
            {
                var results = await session.Query<MessagesViewIndex.SortAndFilterOptions, MessagesViewIndex>()
                    .IncludeSystemMessagesWhere(request)
                    .Where(m => m.ReceivingEndpointName == input)
                    .Statistics(out var stats)
                    .Sort(request)
                    .Paging(request)
                    .TransformWith<MessagesViewTransformer, MessagesView>()
                    .ToListAsync()
                    .ConfigureAwait(false);

                return new QueryResult<List<MessagesView>>(results.ToList(), stats.ToQueryStatsInfo());
            }
        }
    }
}