﻿namespace ServiceControl.Monitoring
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;
    using Autofac;
    using Autofac.Core.Activators.Reflection;
    using Autofac.Features.ResolveAnything;
    using Licensing;
    using Messaging;
    using Microsoft.Owin.Hosting;
    using NServiceBus;
    using NServiceBus.Configuration.AdvancedExtensibility;
    using NServiceBus.Features;
    using NServiceBus.Logging;
    using NServiceBus.Pipeline;
    using QueueLength;
    using ServiceBus.Management.Infrastructure.OWIN;
    using Module = Autofac.Module;

    public class Bootstrapper
    {
        // Windows Service
        public Bootstrapper(Settings settings)
        {
            this.settings = settings;
        }

        internal Startup Startup { get; set; }

        internal async Task<IEndpointInstance> CreateAndStartReceiver(EndpointConfiguration config = null, string explicitConnectionStringValue = null)
        {
            var endpointConfiguration = config ?? new EndpointConfiguration(settings.EndpointName);

            CreateReceiver(endpointConfiguration, explicitConnectionStringValue);

            bus = await Endpoint.Start(endpointConfiguration);

            return bus;
        }

        public void CreateReceiver(EndpointConfiguration config, string explicitConnectionStringValue = null)
        {
            var transportType = DetermineTransportType(settings);

            var buildQueueLengthProvider = QueueLengthProviderBuilder(explicitConnectionStringValue, transportType);

            var containerBuilder = CreateContainer(settings, buildQueueLengthProvider);

            var transport = config.UseTransport(transportType)
                .Transactions(TransportTransactionMode.ReceiveOnly);

            if (explicitConnectionStringValue != null)
            {
                transport.ConnectionString(explicitConnectionStringValue);
            }
            else
            {
#pragma warning disable 618
                transport.ConnectionStringName("NServiceBus/Transport");
#pragma warning restore 618
            }

            if (settings.EnableInstallers)
            {
                config.EnableInstallers(settings.Username);
            }

            config.DefineCriticalErrorAction(c =>
            {
                Environment.FailFast("NServiceBus Critical Error", c.Exception);
                return TaskEx.Completed;
            });

            config.GetSettings().Set(settings);

            config.UseSerialization<NewtonsoftSerializer>();
            config.UsePersistence<InMemoryPersistence>();
            config.SendFailedMessagesTo(settings.ErrorQueue);
            config.DisableFeature<AutoSubscribe>();

            config.AddDeserializer<TaggedLongValueWriterOccurrenceSerializerDefinition>();
            config.Pipeline.Register(typeof(MessagePoolReleasingBehavior), "Releases pooled message.");
            config.EnableFeature<QueueLength.QueueLength>();

            config.EnableFeature<LicenseCheckFeature>();

            containerBuilder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource(type => type.Assembly == typeof(Bootstrapper).Assembly && type.GetInterfaces().Any() == false));
            containerBuilder.RegisterInstance(settings);

            RegisterInternalWebApiControllers(containerBuilder);

            container = containerBuilder.Build();

            config.UseContainer<AutofacBuilder>(
                c => c.ExistingLifetimeScope(container)
            );

            Startup = new Startup(container);
        }

        static void RegisterInternalWebApiControllers(ContainerBuilder containerBuilder)
        {
            var controllerTypes = Assembly.GetExecutingAssembly().DefinedTypes
                .Where(t => typeof(IHttpController).IsAssignableFrom(t) && t.Name.EndsWith("Controller", StringComparison.Ordinal));

            foreach (var controllerType in controllerTypes)
            {
                containerBuilder.RegisterType(controllerType).FindConstructorsWith(new AllConstructorFinder());
            }
        }

        static Func<QueueLengthStore, IProvideQueueLength> QueueLengthProviderBuilder(string explicitConnectionStringValue, Type transportType)
        {
            return qls =>
            {
                var connectionString = explicitConnectionStringValue ?? ConfigurationManager.ConnectionStrings["NServiceBus/Transport"]?.ConnectionString;

                var queueLengthProviderType = transportType.Assembly.GetTypes()
                    .SingleOrDefault(p => typeof(IProvideQueueLength).IsAssignableFrom(p));

                var queueLengthProvider = queueLengthProviderType != null
                    ? (IProvideQueueLength)Activator.CreateInstance(queueLengthProviderType)
                    : new DefaultQueueLengthProvider();

                queueLengthProvider.Initialize(connectionString, qls);

                return queueLengthProvider;
            };
        }

        static ContainerBuilder CreateContainer(Settings settings, Func<QueueLengthStore, IProvideQueueLength> buildQueueLengthProvider)
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterModule<ApplicationModule>();
            containerBuilder.RegisterInstance(settings).As<Settings>().SingleInstance();
            containerBuilder.Register(c => buildQueueLengthProvider(c.Resolve<QueueLengthStore>())).As<IProvideQueueLength>().SingleInstance();

            return containerBuilder;
        }

        static Type DetermineTransportType(Settings settings)
        {
            var transportTypeName = legacyTransportTypeNames.ContainsKey(settings.TransportType)
                ? legacyTransportTypeNames[settings.TransportType]
                : settings.TransportType;

            var transportType = Type.GetType(transportTypeName);

            if (transportType != null)
            {
                return transportType;
            }

            var errorMsg = $"Configuration of transport failed. Could not resolve type `{settings.TransportType}`";
            Logger.Error(errorMsg);
            throw new Exception(errorMsg);
        }

        public async Task<IEndpointInstance> Start()
        {
            bus = await CreateAndStartReceiver();

            StartWebApi();

            return bus;
        }

        public void StartWebApi()
        {
            var startOptions = new StartOptions(settings.RootUrl);

            WebApp = Microsoft.Owin.Hosting.WebApp.Start(startOptions, b => Startup.Configuration(b));
        }

        public async Task Stop()
        {
            if (bus != null)
            {
                await bus.Stop().ConfigureAwait(false);
            }

            WebApp?.Dispose();
            container.Dispose();
        }

        public IDisposable WebApp;
        private Settings settings;
        private IContainer container;
        private IEndpointInstance bus;

        static Dictionary<string, string> legacyTransportTypeNames = new Dictionary<string, string>
        {
            {"NServiceBus.SqsTransport, NServiceBus.AmazonSQS", "ServiceControl.Transports.AmazonSQS.ServiceControlSqsTransport, ServiceControl.Transports.AmazonSQS"},
            {"NServiceBus.AzureServiceBusTransport, NServiceBus.Azure.Transports.WindowsAzureServiceBus", "ServiceControl.Transports.LegacyAzureServiceBus.ForwardingTopologyAzureServiceBusTransport, ServiceControl.Transports.LegacyAzureServiceBus"},
            {"NServiceBus.RabbitMQTransport, NServiceBus.Transports.RabbitMQ", "ServiceControl.Transports.RabbitMQ.ConventialRoutingTopologyRabbitMQTransport, ServiceControl.Transports.RabbitMQ"},
            {"NServiceBus.SqlServerTransport, NServiceBus.Transport.SQLServer", "ServiceControl.Transports.SQLServer.ServiceControlSQLServerTransport, ServiceControl.Transports.SQLServer"},
            {"NServiceBus.AzureStorageQueueTransport, NServiceBus.Azure.Transports.WindowsAzureStorageQueues", "ServiceControl.Transports.AzureStorageQueues.ServiceControlAzureStorageQueueTransport, ServiceControl.Transports.AzureStorageQueues"}
        };

        static ILog Logger = LogManager.GetLogger<Bootstrapper>();
    }

    class AllConstructorFinder : IConstructorFinder
    {
        public ConstructorInfo[] FindConstructors(Type targetType)
        {
            var result = Cache.GetOrAdd(targetType, t => t.GetTypeInfo().DeclaredConstructors.ToArray());

            return result.Length > 0 ? result : throw new Exception($"No constructor found for type {targetType.FullName}");
        }

        static readonly ConcurrentDictionary<Type, ConstructorInfo[]> Cache = new ConcurrentDictionary<Type, ConstructorInfo[]>();
    }

    class MessagePoolReleasingBehavior : Behavior<IIncomingLogicalMessageContext>
    {
        public override async Task Invoke(IIncomingLogicalMessageContext context, Func<Task> next)
        {
            try
            {
                await next().ConfigureAwait(false);
            }
            finally
            {
                var messageType = context.Message.MessageType;
                var instance = context.Message.Instance;

                if (messageType == typeof(TaggedLongValueOccurrence))
                {
                    ReleaseMessage<TaggedLongValueOccurrence>(instance);
                }
            }
        }

        static void ReleaseMessage<T>(object instance) where T : RawMessage, new()
        {
            RawMessage.Pool<T>.Default.Release((T)instance);
        }
    }

    class ApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(Include)
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();
        }

        static bool Include(Type type)
        {
            if (IsMessageType(type))
            {
                return false;
            }

            if (IsMessageHandler(type))
            {
                return false;
            }

            return true;
        }

        static bool IsMessageType(Type type)
        {
            return typeof(IMessage).IsAssignableFrom(type);
        }


        static bool IsMessageHandler(Type type)
        {
            return type.GetInterfaces()
                .Where(@interface => @interface.IsGenericType)
                .Select(@interface => @interface.GetGenericTypeDefinition())
                .Any(genericTypeDef => genericTypeDef == typeof(IHandleMessages<>));
        }
    }
}