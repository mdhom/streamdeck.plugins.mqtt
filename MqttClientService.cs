using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;

namespace streamdeck.plugins.mqtt
{
    public class MqttClientService : IHostedService, IMqttApplicationMessageReceivedHandler, IMqttClientDisconnectedHandler
    {
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1, 1);
        private string _broker;
        private int _port;

        public IManagedMqttClient MqttClient { get; private set; }
        public HashSet<string> SubscriptionTopics { get; } = new HashSet<string>();

        public event EventHandler<MqttApplicationMessageReceivedEventArgs> MessageReceived;
        public event EventHandler Disconnected;

        public async Task Connect(string broker, int port)
        {
            if (broker == _broker && port == _port)
            {
                // already connected / connecting to that target
                return;
            }
            _broker = broker;
            _port = port;

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

        private void Disconnect()
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

        public async Task SubscribeAsync(string topic)
        {
            try
            {
                // locking subscription handling because of multithreading concurrency
                await _syncRoot.WaitAsync();

                if (string.IsNullOrEmpty(topic)) return;

                if (SubscriptionTopics.Add(topic))
                    await MqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
            }
            finally
            {
                _syncRoot.Release();
            }
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
