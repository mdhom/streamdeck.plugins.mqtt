namespace SharedCounter.Actions
{
    using System;
    using System.Threading.Tasks;
    using MQTTnet;
    using MQTTnet.Client.Options;
    using MQTTnet.Extensions.ManagedClient;
    using Newtonsoft.Json;
    using SharpDeck;
    using SharpDeck.Events.Received;

    /// <summary>
    /// The shared counter action; displays the count, and increments the count each press.
    /// </summary>
    [StreamDeckAction("org.mdwd.sharedcounter.counter")]
    public class PublisherAction : MqttActionBase
    {
        public PublisherAction(MqttClientService mqttClient) 
            : base(mqttClient)
        {
        }

        /// <inheritdoc/>
        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            await PublishAsync(Settings.Topic, "{}");

            await base.OnKeyDown(args);
        }
    }
}
