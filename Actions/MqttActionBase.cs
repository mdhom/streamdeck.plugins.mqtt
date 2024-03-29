﻿using System.Threading.Tasks;
using MQTTnet.Extensions.ManagedClient;
using SharpDeck;
using SharpDeck.Events.Received;

namespace streamdeck.plugins.mqtt.Actions
{
    public abstract class MqttActionBase : StreamDeckAction<MqttSettings>
    {
        private readonly MqttClientService _mqttClient;

        protected MqttSettings Settings { get; private set; }

        public MqttActionBase(MqttClientService mqttClient)
        {
            _mqttClient = mqttClient;
        }

        protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args)
        {
            var settings = args.Payload.Settings["settingsModel"]?.ToObject<MqttSettings>();
            if (settings == null) return;

            var mqttTargetChanged = settings.Broker != Settings?.Broker || settings.Port != Settings?.Port;

            Settings = settings;

            if (mqttTargetChanged)
                await _mqttClient.Connect(Settings.Broker, Settings.Port);

            await OnSettingsUpdated(Settings);
        }

        protected virtual Task OnSettingsUpdated(MqttSettings settings)
        {
            return Task.CompletedTask;
        }

        protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
        {
            await base.OnWillAppear(args);

            if (Settings == null)
            {
                if (!TryExtractSettings(args.Payload, out var settings))
                    return;
                Settings = settings;
            }

            await _mqttClient.Connect(Settings.Broker, Settings.Port);

            await OnAppear(Settings);
        }

        protected virtual Task OnAppear(MqttSettings settings)
        {
            return Task.CompletedTask;
        }

        private bool TryExtractSettings(SettingsPayload payload, out MqttSettings settings)
        {
            settings = payload.Settings["settingsModel"]?.ToObject<MqttSettings>();
            return settings != null;
        }

        protected Task PublishAsync(string topic, string payload)
            => _mqttClient.MqttClient.PublishAsync(topic, payload);

        protected Task Subscribe(string topic)
            => _mqttClient.SubscribeAsync(topic);
    }
}
