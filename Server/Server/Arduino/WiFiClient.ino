#include <ESP8266WiFi.h>
#include <WebSocketsClient.h>
#include <ArduinoJson.h>


#ifndef STASSID
#define STASSID "Synology"
#define STAPSK "wpa2.psk.86021813458"
#endif

const char* ssid = STASSID;
const char* password = STAPSK;
const int allowRemoteGpio[1] = { 5 };
const String deviceName = "DEV_1";
WebSocketsClient webSocket;

void webSocketEvent(WStype_t type, uint8_t* payload, size_t length) {

  switch (type) {
    case WStype_DISCONNECTED:
      Serial.printf("[Socket] Disconnected!\n");
      break;
    case WStype_CONNECTED:
      Serial.printf("[Socket] Connected to url: %s\n", payload);
      initialize();
      break;
    case WStype_TEXT:
      Serial.printf("[Socket] Receive: %s\n", payload);
      execute(payload);
      break;
  }
}

void initialize() {
  DynamicJsonDocument connectionInfo(1024);
  String response;

  connectionInfo["Command"] = "REGISTER_NODE_MCU";
  connectionInfo["Args"] = deviceName;
  serializeJson(connectionInfo, response);
  webSocket.sendTXT(response);
}

void execute(uint8_t* payload) {

  DynamicJsonDocument json(1024);

  deserializeJson(json, payload);
  String command = json["Command"];
  String args = json["Args"];
  int gpio = args.toInt();
  String response;

  if (command == "SWITCH_GPIO") {
    if (digitalRead(gpio) == 1) {
      digitalWrite(gpio, LOW);
      json["Args"] = String(gpio) + ":" + digitalRead(gpio);
    } else {
      digitalWrite(gpio, HIGH);
      json["Args"] = String(gpio) + ":" + digitalRead(gpio);
    }
    json["Command"] = "SWITCH_GPIO_DONE";
  } else if (command == "STATE_GPIO") {
    json["Args"] = digitalRead(gpio);
    json["Command"] = "STATE_GPIO_DONE";
  } else if (command == "SWITCH_LED") {
    if (digitalRead(LED_BUILTIN) == 1) {
      digitalWrite(LED_BUILTIN, LOW);
      json["Args"] = String(gpio) + ":1";
    } else {
      digitalWrite(LED_BUILTIN, HIGH);
      json["Args"] = String(gpio) + ":0";
    }
    json["Command"] = "SWITCH_LED_DONE";
  } else if (command == "GET_GPIO_LIST") {
    String args;
    for (int currentGpio = 0; currentGpio < sizeof(allowRemoteGpio) / sizeof(int); currentGpio++) {
      args = args + allowRemoteGpio[currentGpio] + ";";
    }
    json["Command"] = "GET_GPIO_LIST_DONE";
    json["Args"] = args;
  } else if (command == "OFF_GPIO") {
    digitalWrite(gpio, LOW);
    json["Command"] = "OFF_GPIO_DONE";
    json["Args"] = String(gpio) + ":0";
    ;
  } else if (command == "ON_GPIO") {
    digitalWrite(gpio, HIGH);
    json["Command"] = "ON_GPIO_DONE";
    json["Args"] = String(gpio) + ":1";
  }

  serializeJson(json, response);
  Serial.print("[Socket] Send: ");
  Serial.println(response);
  webSocket.sendTXT(response);
}

// void onSocketConnected() {
//   while (!manualLedMode) {
//     digitalWrite(LED_BUILTIN, LOW);
//     delay(100);
//     digitalWrite(LED_BUILTIN, HIGH);
//     delay(50);
//     digitalWrite(LED_BUILTIN, LOW);
//     delay(100);
//     digitalWrite(LED_BUILTIN, HIGH);
//     delay(1000);
//   }
// }

void setup() {
  Serial.begin(115200);

  //pinMode(LED_BUILTIN, OUTPUT);
  //digitalWrite(LED_BUILTIN, HIGH);

  //pinMode(1, OUTPUT);
  //pinMode(2, OUTPUT);
  // pinMode(3, OUTPUT);
  //pinMode(4, OUTPUT);
  pinMode(5, OUTPUT);
  //pinMode(6, OUTPUT);
  //pinMode(7, OUTPUT);
  //pinMode(8, OUTPUT);

  Serial.println();
  Serial.print("Connecting to ");
  Serial.println(ssid);

  WiFi.mode(WIFI_STA);
  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());

  webSocket.beginSSL("hub-iot.azurewebsites.net", 443, "/ws");
  webSocket.onEvent(webSocketEvent);
}

void loop() {
  webSocket.loop();
}