using System.Threading.Tasks;
using SharpDeck;
using SharpDeck.Events.Received;

namespace streamdeck.plugins.mqtt.Actions
{
    [StreamDeckAction("org.mdwd.streamdeck.plugins.mqtt.publisher")]
    public class PublisherAction : MqttActionBase
    {
        public PublisherAction(MqttClientService mqttClient) 
            : base(mqttClient)
        {
        }

        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args) 
            => await PublishAsync(Settings.Topic, "{}");
    }
}
