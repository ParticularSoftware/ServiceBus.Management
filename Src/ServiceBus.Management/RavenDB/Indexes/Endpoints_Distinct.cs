namespace ServiceBus.Management.RavenDB.Indexes
{
    using System.Linq;
    using Raven.Client.Indexes;

    public class Endpoints_Distinct : AbstractMultiMapIndexCreationTask<Endpoints_Distinct.Result>
    {
        public class Result
        {
            public EndpointDetails Endpoint { get; set; }
        }

        public Endpoints_Distinct()
        {
            AddMap<Message>( messages => from message in messages
                              select new
                                  {
                                      Endpoint = message.OriginatingEndpoint
                                  });
            AddMap<Message>(messages => from message in messages
                                        select new
                                        {
                                            Endpoint = message.ReceivingEndpoint
                                        });
          
            Reduce = results => from result in results
                                group result by result.Endpoint
                                into g
                                select new
                                    {
                                        Endpoint = g.Key
                                    };
        }
    }
}