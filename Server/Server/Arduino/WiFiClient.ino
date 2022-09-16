#include <ESP8266WiFi.h>
#include <WebSocketsClient.h>
#include <ArduinoJson.h>


#ifndef STASSID
#define STASSID ""
#define STAPSK ""
#endif

const char* ssid = STASSID;
const char* password = STAPSK;
const int allowRemoteGpio[3] = { 12, 13, 15 };
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
      json["Args"] = String(gpio) + ":" + digitalRead(gpio);
    } else {
      digitalWrite(LED_BUILTIN, HIGH);
      json["Args"] = String(gpio) + ":" + digitalRead(gpio);
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
    digitalWrite(gpio, HIGH);
    json["Command"] = "OFF_GPIO_DONE";
    json["Args"] = String(gpio) + ":" + digitalRead(gpio);
    ;
  } else if (command == "ON_GPIO") {
    digitalWrite(gpio, LOW);
    json["Command"] = "ON_GPIO_DONE";
    json["Args"] = String(gpio) + ":" + digitalRead(gpio);
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

  //pinMode(16, OUTPUT);  //D0
  //pinMode(5, OUTPUT);   //D1
  //pinMode(4, OUTPUT);   //D2
  //pinMode(0, OUTPUT);   //D3
  //pinMode(2, OUTPUT);   //D4
  //pinMode(14, OUTPUT);  //D5
  pinMode(12, OUTPUT);  //D6
  pinMode(13, OUTPUT);  //D7
  pinMode(15, OUTPUT);  //D8

  digitalWrite(12, HIGH);  //D6
  digitalWrite(13, HIGH);  //D7
  digitalWrite(15, HIGH);  //D8

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