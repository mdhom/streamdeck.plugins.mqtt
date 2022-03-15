namespace SharedCounter.Actions
{
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;
    using SharpDeck;
    using SharpDeck.Events.Received;

    /// <summary>
    /// The reset count action.
    /// </summary>
    [StreamDeckAction("org.mdwd.sharedcounter.reset")]
    public class SubscriberAction : MqttActionBase<MqttSettings>
    {
        public SubscriberAction(MqttClientService mqttClient)
            : base(mqttClient)
        {
            mqttClient.MessageReceived += MqttClient_MessageReceived;
        }

        protected override Task OnAppear(MqttSettings settings)
        {
            Debug.WriteLine($"{nameof(SubscriberAction)} appear {settings.Topic}");
            return Subscribe(settings.Topic);
        }

        protected override Task OnSettingsUpdated(MqttSettings settings)
        {
            Debug.WriteLine($"{nameof(SubscriberAction)} settings updated {settings.Topic}");
            return Subscribe(settings.Topic);
        }

        private void MqttClient_MessageReceived(object sender, MQTTnet.MqttApplicationMessageReceivedEventArgs e)
        {
            if (e.ApplicationMessage.Topic != Settings.Topic)
                return;
            Debug.WriteLine($"Received {e.ApplicationMessage.Topic}");
            var title = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            Task.Run(async () =>
            {
                Debug.WriteLine($"title={title}");
                await SetTitleAsync(title);
            });
        }
    }
}
