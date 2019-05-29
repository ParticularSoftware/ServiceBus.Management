﻿namespace ServiceBus.Management.AcceptanceTests.Audit
{
    using System;
    using System.Threading.Tasks;
    using EndpointTemplates;
    using NServiceBus;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTesting.Customization;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.Settings;
    using NUnit.Framework;
    using ServiceControl.Audit.Auditing.MessagesView;

    class When_processed_message_searched_by_messageBody : AcceptanceTest
    {
        [Test]
        public async Task Should_be_found_()
        {
            await Define<MyContext>(ctx => { ctx.PropertyToSearchFor = Guid.NewGuid().ToString(); })
                .WithEndpoint<Sender>(b => b.When((bus, c) => bus.Send(new MyMessage
                {
                    PropertyToSearchFor = c.PropertyToSearchFor
                })))
                .WithEndpoint<Receiver>()
                .Done(async c => await this.TryGetMany<MessagesView>("/api/messages/search/" + c.PropertyToSearchFor))
                .Run();
        }

        public class Sender : EndpointConfigurationBuilder
        {
            public Sender()
            {
                EndpointSetup<DefaultServerWithoutAudit>(c =>
                {
                    var routing = c.ConfigureTransport().Routing();
                    routing.RouteToEndpoint(typeof(MyMessage), typeof(Receiver));
                });
            }
        }

        public class Receiver : EndpointConfigurationBuilder
        {
            public Receiver()
            {
                EndpointSetup<DefaultServerWithAudit>();
            }

            public class MyMessageHandler : IHandleMessages<MyMessage>
            {
                public MyContext Context { get; set; }

                public ReadOnlySettings Settings { get; set; }

                public Task Handle(MyMessage message, IMessageHandlerContext context)
                {
                    Context.EndpointNameOfReceivingEndpoint = Settings.EndpointName();
                    Context.MessageId = context.MessageId;
                    return Task.FromResult(0);
                }
            }
        }

        public class MyMessage : ICommand
        {
            public string PropertyToSearchFor { get; set; }
        }

        public class MyContext : ScenarioContext
        {
            public string MessageId { get; set; }

            public string EndpointNameOfReceivingEndpoint { get; set; }

            public string PropertyToSearchFor { get; set; }
        }
    }
}