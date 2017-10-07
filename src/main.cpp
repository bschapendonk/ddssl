#include <Arduino.h>

#include <WiFiClientSecure.h>
#include <WiFiUdp.h>

#include <AzureIoTHub.h>
#include <AzureIoTUtility.h>
#include <AzureIoTProtocol_MQTT.h>

#include "Secrets.h"

uint8_t green = 25;
uint8_t orange = 26;
uint8_t red = 27;
uint8_t ledArray[3] = {1, 2, 3};

int brightness = 0;
int fadeAmount = 5;

static IOTHUB_CLIENT_LL_HANDLE iotHubClientHandle;

void setup()
{
    ledcSetup(1, 12000, 12);
    ledcSetup(2, 12000, 12);
    ledcSetup(3, 12000, 12);

    ledcAttachPin(green, 1);
    ledcAttachPin(orange, 2);
    ledcAttachPin(red, 3);

    for (uint8_t i = 0; i < 3; i++)
    {
        ledcWrite(ledArray[i], 4095); // test high output of all leds in sequence
        delay(1000);
        ledcWrite(ledArray[i], 0);
    }

    Serial.begin(115200);
    delay(10);

    Serial.println();
    Serial.println();
    Serial.print("Connecting to ");
    Serial.println(SECRET_WIFI_SSID);

    WiFi.begin(SECRET_WIFI_SSID, SECRET_WIFI_PASSWORD);

    while (WiFi.status() != WL_CONNECTED)
    {
        delay(500);
        Serial.print(".");
    }

    Serial.println("");
    Serial.println("WiFi connected");
    Serial.println("IP address: ");
    Serial.println(WiFi.localIP());

    iotHubClientHandle = IoTHubClient_LL_CreateFromConnectionString(SECRET_DEVICE_CONNECTIONSTRING, MQTT_Protocol);
    if (iotHubClientHandle == NULL)
    {
        Serial.println("Failed on IoTHubClient_CreateFromConnectionString.");
        while (1);
    }
}

void loop()
{
    ledcWrite(1, brightness);
    ledcWrite(2, brightness);
    ledcWrite(3, brightness);

    // change the brightness for next time through the loop:
    brightness = brightness + fadeAmount;

    // reverse the direction of the fading at the ends of the fade:
    if (brightness <= 0 || brightness >= 4095)
    {
        fadeAmount = -fadeAmount;
    }
    // wait for 30 milliseconds to see the dimming effect
    delay(2);
}