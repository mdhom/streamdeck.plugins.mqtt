using System;
using System.Collections.Generic;
using System.Text;

namespace streamdeck.plugins.mqtt
{
    public class MqttSettings
    {
        public string Title { get; set; }
        public string Broker { get; set; }
        public int Port { get; set; }
        public string Topic { get; set; }
    }
}
