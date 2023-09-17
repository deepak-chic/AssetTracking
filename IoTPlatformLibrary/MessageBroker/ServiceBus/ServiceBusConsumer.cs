using System.ComponentModel.Design;
using System.Reflection;
using System.Text;
using IoTPlatformLibrary.EventHandler;
using IoTPlatformLibrary.Events;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace IoTPlatformLibrary.ServiceBus
{
    public class ServiceBusConsumer : IServiceBusConsumer
    {
        private readonly IEnumerable<IIntegrationBaseHandler> _eventHandlers;
        private readonly IOptions<ServiceBusConfigOption> _serviceBusConfig;
        private readonly IServiceProvider _serviceProvider;

        public ServiceBusConsumer(IEnumerable<IIntegrationBaseHandler> eventHandlers,
            IOptions<ServiceBusConfigOption> serviceBusConfig,
            IServiceProvider serviceProvider)
        {
            _eventHandlers = eventHandlers;
            _serviceBusConfig = serviceBusConfig;
            _serviceProvider = serviceProvider;
            this.MakeConnection();
        }

        private void MakeConnection()
        {
            var subscriptionClient = new SubscriptionClient(_serviceBusConfig.Value.ConnectionString,
                                                                _serviceBusConfig.Value.TopicName,
                                                                _serviceBusConfig.Value.SubscriptionName)
            {
                PrefetchCount = _serviceBusConfig.Value.PrefetchCount
            };

            if (_serviceBusConfig.Value.IsSessionEnabled)
            {
                var sessionHandlerOptions = new SessionHandlerOptions(ExceptionHandler)
                {
                    MaxConcurrentSessions = _serviceBusConfig.Value.MaxConcurrentSessions,
                    AutoComplete = _serviceBusConfig.Value.AutoComplete
                };

                subscriptionClient.RegisterSessionHandler(ProcessMessagesAsync, sessionHandlerOptions);
            }
            else
            {

                var messageHandlerOptions = new MessageHandlerOptions(ExceptionHandler)
                {
                    MaxConcurrentCalls = _serviceBusConfig.Value.MaxConcurrentCalls,
                    AutoComplete = _serviceBusConfig.Value.AutoComplete
                };

                subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
            }
        }

        private async Task ExceptionHandler(ExceptionReceivedEventArgs exceptionReceived)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Message handler encountered an exception {exceptionReceived.Exception}.");
            var context = exceptionReceived.ExceptionReceivedContext;
            stringBuilder.AppendLine("Exception context for troubleshooting:");
            stringBuilder.AppendLine($"- Endpoint: {context.Endpoint}");
            stringBuilder.AppendLine($"- Entity Path: {context.EntityPath}");
            stringBuilder.AppendLine($"- Executing Action: {context.Action}");

            Console.WriteLine(stringBuilder.ToString());
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken cancellationToken)
        {
            await ProcessIfLockedAsync(message);
        }

        private async Task ProcessMessagesAsync(IMessageSession messageSession, Message message, CancellationToken cancellationToken)
        {
            await ProcessIfLockedAsync(message);
        }

        private async Task ProcessIfLockedAsync(Message messageBody)
        {
            //this check is required when prefetching is on
            if (messageBody.SystemProperties.LockedUntilUtc <= DateTime.UtcNow)
            {
                throw new Exception("Exception while fetching from service bus.");
            }

            var message = Encoding.UTF8.GetString(messageBody.Body);
            dynamic dynamicEvent = JsonConvert.DeserializeObject(message);

            var baseEventHandler = _eventHandlers.FirstOrDefault(e => e.MessageType.Name.ToString() == ((Type)dynamicEvent.MessageType).Name);
            if (baseEventHandler != null)
            {
                dynamic eventHandler = GetHandler(baseEventHandler);
                if (eventHandler != null)
                {
                    try
                    {
                        var telemetry = JsonConvert.DeserializeObject(message, eventHandler.MessageType);
                        eventHandler.HandleEvent(telemetry);
                    }
                    catch(Exception ex)
                    {

                    }
                }
                else
                {
                    throw new Exception($"Event handler not found for message type {dynamicEvent.MessageType.ToString()}");
                }
            }
        }

        private dynamic GetHandler(IIntegrationBaseHandler integratioMessage)
        {
            var scope = _serviceProvider.CreateScope();
            return ActivatorUtilities.CreateInstance(scope.ServiceProvider, integratioMessage.GetType());
        }
    }
}