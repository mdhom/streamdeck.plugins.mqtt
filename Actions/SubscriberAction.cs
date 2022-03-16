using System.Text;
using System.Threading.Tasks;
using SharpDeck;

namespace streamdeck.plugins.mqtt.Actions
{
    [StreamDeckAction("org.mdwd.streamdeck.plugins.mqtt.subscriber")]
    public class SubscriberAction : MqttActionBase
    {
        private string _title;

        public SubscriberAction(MqttClientService mqttClient)
            : base(mqttClient)
        {
            mqttClient.MessageReceived += MqttClient_MessageReceived;
            mqttClient.Disconnected += MqttClient_Disconnected;
        }

        private void MqttClient_Disconnected(object sender, System.EventArgs e)
        {
            Task.Run(async () =>
            {
                await SetTitleAsync("Disconnected");
            });
        }

        protected override Task OnAppear(MqttSettings settings) 
            => Subscribe(settings.Topic);

        protected override Task OnSettingsUpdated(MqttSettings settings) 
            => Subscribe(settings.Topic);

        private void MqttClient_MessageReceived(object sender, MQTTnet.MqttApplicationMessageReceivedEventArgs e)
        {
            if (e.ApplicationMessage.Topic != Settings.Topic) return; // only handle my own topic
            var title = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            if (title == _title) return; // title has not changed
            _title = title;
            Task.Run(async () =>
            {
                await SetTitleAsync(title);
            });
        }
    }
}
