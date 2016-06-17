namespace ServiceControl.MessageRedirects.Api
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Nancy;
    using Raven.Client.Linq;

    static class RavenQueryableExtensions
    {
        public static IRavenQueryable<TSource> Sort<TSource>(this IRavenQueryable<TSource> source, Request request,
            Expression<Func<TSource, object>> defaultKeySelector = null, string defaultSortDirection = "desc")
            where TSource : MessageRedirect
        {
            var direction = defaultSortDirection;

            if (request.Query.direction.HasValue)
            {
                direction = (string) request.Query.direction;
            }

            if (direction != "asc" && direction != "desc")
            {
                direction = defaultSortDirection;
            }

            var sortOptions = new[]
            {
                "from_physicaladdress",
                "to_physicaladdress",
                "created",
                "lastused"
            };

            var sort = "created";
            Expression<Func<TSource, object>> keySelector;

            if (request.Query.sort.HasValue)
            {
                sort = (string) request.Query.sort;
            }

            if (!sortOptions.Contains(sort))
            {
                sort = "created";
            }

            switch (sort)
            {
                case "id":
                case "messageredirectid":
                    keySelector = m => m.Id;
                    break;

                case "from_physicaladdress":
                    keySelector = m => m.FromPhysicalAddress;
                    break;

                case "to_physicaladdress":
                    keySelector = m => m.ToPhysicalAddress;
                    break;

                case "created":
                    keySelector = m => m.Created;
                    break;

                case "lastused":
                    keySelector = m => m.LastUsed;
                    break;

                default:
                    if (defaultKeySelector == null)
                    {
                        keySelector = m => m.Created;
                    }
                    else
                    {
                        keySelector = defaultKeySelector;
                    }
                    break;
            }

            return direction == "asc" ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
        }
    }
}