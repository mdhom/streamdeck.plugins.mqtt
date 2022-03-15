using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using SharpDeck.Connectivity;

namespace SharedCounter
{
    public class MqttClientService : IHostedService, IMqttApplicationMessageReceivedHandler, IMqttClientDisconnectedHandler
    {
        public IManagedMqttClient MqttClient { get; private set; }
        public HashSet<string> SubscriptionTopics { get; } = new HashSet<string>();

        public event EventHandler<MqttApplicationMessageReceivedEventArgs> MessageReceived;
        public event EventHandler Disconnected;

        public MqttClientService(IStreamDeckConnection connection)
        {

        }

        public async Task Connect(string broker, int port)
        {
            if (MqttClient != null && MqttClient.IsConnected)
            {
                Disconnect();
            }

            Debug.WriteLine($"Connecting to {broker}:{port}");
            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("StreamDeck" + Guid.NewGuid().ToString())
                    .WithTcpServer(broker, port)
                    .Build())
                .Build();

            MqttClient = new MqttFactory().CreateManagedMqttClient();
            MqttClient.ApplicationMessageReceivedHandler = this;
            MqttClient.DisconnectedHandler = this;

            foreach (var topic in SubscriptionTopics)
                await MqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());

            await MqttClient.StartAsync(options);
        }

        public void Disconnect()
        {
            Debug.WriteLine($"Disconnect");
            MqttClient.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task SubscribeAsync(string topic)
        {
            if (string.IsNullOrEmpty(topic)) return Task.CompletedTask;

            if (SubscriptionTopics.Add(topic))
                return MqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
            return Task.CompletedTask;
        }

        public Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            MessageReceived?.Invoke(this, eventArgs);
            return Task.CompletedTask;
        }

        public Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
        {
            Debug.WriteLine(nameof(HandleDisconnectedAsync));
            Disconnected?.Invoke(this, eventArgs);
            return Task.CompletedTask;
        }
    }
}
