#include <Arduino.h>

#include <WiFiClientSecure.h>
#include <WiFiUdp.h>

#include <iothub.h>
#include <secrets.h>

void setup()
{
    initLights();
    initSerial();
    initWifi();
    initTime();
}

void loop()
{
    iothub_run();
}

void initLights()
{
    ledcSetup(1, 12000, 12);
    ledcSetup(2, 12000, 12);
    ledcSetup(3, 12000, 12);

    ledcAttachPin(25, 1);
    ledcAttachPin(26, 2);
    ledcAttachPin(27, 3);

    uint8_t ledArray[3] = {1, 2, 3};
    for (uint8_t i = 0; i < 3; i++)
    {
        ledcWrite(ledArray[i], 4095); // test high output of all leds in sequence
        delay(1000);
        ledcWrite(ledArray[i], 0);
    }
}

void initSerial()
{
    Serial.begin(115200);
    delay(10);
}

void initWifi()
{
    Serial.print("Attempting to connect to SSID: ");
    Serial.println(SECRET_WIFI_SSID);

    WiFi.begin(SECRET_WIFI_SSID, SECRET_WIFI_PASSWORD);

    Serial.print("Waiting for Wifi connection.");
    while (WiFi.status() != WL_CONNECTED)
    {
        Serial.print(".");
        delay(500);
    }

    Serial.println("Connected to wifi");
    Serial.println("IP address: ");
    Serial.println(WiFi.localIP());
}

void initTime()
{
    time_t epochTime;

    configTime(0, 0, "time1.google.com", "time1.google.com");

    while (true)
    {
        epochTime = time(NULL);

        if (epochTime == 0)
        {
            Serial.println("Fetching NTP epoch time failed! Waiting 2 seconds to retry.");
            delay(2000);
        }
        else
        {
            Serial.print("Fetched NTP epoch time is: ");
            Serial.println(epochTime);
            break;
        }
    }
}