// global websocket, used to communicate from/to Stream Deck software
// as well as some info about our plugin, as sent by Stream Deck software 
var websocket = null,
    uuid = null,
    inInfo = null,
    actionInfo = {},
    settingsModel = {
        Port: 1883,
        Broker: "",
        Topic: "streamdeck/button"
    };

function connectElgatoStreamDeckSocket(inPort, inUUID, inRegisterEvent, inInfo, inActionInfo) {
    uuid = inUUID;
    actionInfo = JSON.parse(inActionInfo);
    inInfo = JSON.parse(inInfo);
    websocket = new WebSocket('ws://localhost:' + inPort);

    //initialize values
    if (actionInfo.payload.settings.settingsModel) {
        settingsModel.Port = actionInfo.payload.settings.settingsModel.Port;
        settingsModel.Broker = actionInfo.payload.settings.settingsModel.Broker;
        settingsModel.Topic = actionInfo.payload.settings.settingsModel.Topic;
    }

    document.getElementById('txtPortValue').value = settingsModel.Port;
    document.getElementById('txtBrokerValue').value = settingsModel.Broker;
    document.getElementById('txtTopicValue').value = settingsModel.Topic;

    websocket.onopen = function () {
        var json = { event: inRegisterEvent, uuid: inUUID };
        // register property inspector to Stream Deck
        websocket.send(JSON.stringify(json));

    };

    websocket.onmessage = function (evt) {
        // Received message from Stream Deck
        var jsonObj = JSON.parse(evt.data);
        var sdEvent = jsonObj['event'];
        switch (sdEvent) {
            case "didReceiveSettings":
                if (jsonObj.payload.settings.settingsModel.Port) {
                    settingsModel.Port = jsonObj.payload.settings.settingsModel.Port;
                    document.getElementById('txtPortValue').value = settingsModel.Port;
                }
                if (jsonObj.payload.settings.settingsModel.Broker) {
                    settingsModel.Broker = jsonObj.payload.settings.settingsModel.Broker;
                    document.getElementById('txtBrokerValue').value = settingsModel.Broker;
                }
                if (jsonObj.payload.settings.settingsModel.Topic) {
                    settingsModel.Topic = jsonObj.payload.settings.settingsModel.Topic;
                    document.getElementById('txtTopicValue').value = settingsModel.Topic;
                }
                break;
            default:
                break;
        }
    };
}

const setSettings = (value, param) => {
    if (websocket) {
        settingsModel[param] = value;
        var json = {
            "event": "setSettings",
            "context": uuid,
            "payload": {
                "settingsModel": settingsModel
            }
        };
        websocket.send(JSON.stringify(json));
    }
};

