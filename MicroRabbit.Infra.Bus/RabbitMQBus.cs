using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MicroRabbit.Infra.Bus
{
    public sealed class RabbitMQBus : IEventBus
    {
        private readonly IMediator _mediator;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public bool isRabbitMQUp { get; set; }

        public RabbitMQBus(IMediator mediator, IServiceScopeFactory serviceScopeFactory)
        {
            _mediator = mediator;
            _serviceScopeFactory = serviceScopeFactory;
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new List<Type>();
            if (!CheckRabbitMQStatus())
            {
                throw new Exception("Unable to access RabbitMQ Server.This may be down or unable to start.");
            }
        }


        public bool CheckRabbitMQStatus()
        {
            bool isOpen = false;

            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                DispatchConsumersAsync = true
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                isOpen = channel.IsOpen;
            }

            return isOpen;
        }

        public Task SendCommand<T>(T command) where T : Command
        {
            return _mediator.Send(command);
            //throw new NotImplementedException();
        }


        public void Publish<T>(T @event) where T : Event
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var eventName = @event.GetType().Name;
                channel.QueueDeclare(eventName, true, false, false, null);
                channel.ConfirmSelect();
                var outstandingConfirms = new ConcurrentDictionary<ulong, string>();

                void cleanOutstandingConfirms(ulong sequenceNumber, bool multiple)
                {
                    if (multiple)
                    {
                        var confirmed = outstandingConfirms.Where(k => k.Key <= sequenceNumber);
                        foreach (var entry in confirmed)
                            outstandingConfirms.TryRemove(entry.Key, out _);
                    }
                    else
                        outstandingConfirms.TryRemove(sequenceNumber, out _);
                }

                //Set the message to persist in the event of a broker shutdown
                var messageProperties = channel.CreateBasicProperties();
                messageProperties.Persistent = true;

                var message = JsonConvert.SerializeObject(@event);
                //Send an acknowledgement that the message was persisted to disk

                channel.BasicAcks += (sender, ea) => cleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
                channel.BasicNacks += (sender, ea) =>
                {
                    outstandingConfirms.TryGetValue(ea.DeliveryTag, out string body);
                    Console.WriteLine($"Message with body {body} has been nack-ed. Sequence number: {ea.DeliveryTag}, multiple: {ea.Multiple}");
                    cleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
                };

                var timer = new Stopwatch();
                timer.Start();
                //var body = i.ToString();
                outstandingConfirms.TryAdd(channel.NextPublishSeqNo, message);
                channel.BasicPublish(exchange: "", routingKey: eventName, basicProperties: messageProperties, body: Encoding.UTF8.GetBytes(message));

                if (!WaitUntil(60, () => outstandingConfirms.IsEmpty))
                    throw new Exception("All messages could not be confirmed in 60 seconds");

                timer.Stop();
                Console.WriteLine($"Published messages and handled confirm asynchronously {timer.ElapsedMilliseconds:N0} ms");


            }

        }

        private static bool WaitUntil(int numberOfSeconds, Func<bool> condition)
        {
            int waited = 0;
            while (!condition() && waited < numberOfSeconds * 1000)
            {
                Thread.Sleep(100);
                waited += 100;
            }

            return condition();
        }

        public void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            //event doesn't registered 
            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }

            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<Type>());
            }

            if (_handlers[eventName].Any(a => a.GetType() == handlerType))
            {
                throw new ArgumentException($"Handler Type {handlerType.Name} already is registered for '{eventName}'", nameof(handlerType));
            }

            _handlers[eventName].Add(handlerType);

            StartBasicConsume<T>();
        }

        private void StartBasicConsume<T>() where T : Event
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                DispatchConsumersAsync = true
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var eventName = typeof(T).Name;

            channel.QueueDeclare(eventName, true, false, false, null);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += Consumer_Received;
            channel.BasicConsume(eventName, true, consumer);
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var eventName = e.RoutingKey;
            var message = Encoding.UTF8.GetString(e.Body.ToArray());

            try
            {
                await ProcessEvent(eventName, message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {

            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (_handlers.ContainsKey(eventName))
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var subscriptions = _handlers[eventName];

                    foreach (var subscription in subscriptions)
                    {
                        //var handler = Activator.CreateInstance(subscription);

                        var handler = scope.ServiceProvider.GetService(subscription);
                        if (handler == null)
                            continue;

                        var eventType = _eventTypes.SingleOrDefault(s => s.Name == eventName);
                        var @event = JsonConvert.DeserializeObject(message, eventType);

                        var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);

                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { @event });
                    }
                }
            }
        }
    }
}
