#include <Arduino.h>
#include <WiFiClientSecure.h>
#include <WiFiUdp.h>

#include <secrets.h>
#include <iothub.h>

void initLights()
{
    ledcSetup(1, 200, 8);
    ledcSetup(2, 200, 8);
    ledcSetup(3, 200, 8);

    ledcAttachPin(25, 1);
    ledcAttachPin(26, 2);
    ledcAttachPin(27, 3);

    int ledArray[3] = {1, 2, 3};
    for (int i = 0; i < 3; i++)
    {
        ledcWrite(ledArray[i], 255); // test high output of all leds in sequence
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
    /*
    WiFi.mode(WIFI_AP_STA);
    WiFi.beginSmartConfig();

    Serial.println("Waiting for SmartConfig.");
    while (!WiFi.smartConfigDone())
    {
        Serial.print(".");
        ledcWrite(2, 255);
        delay(250);
        ledcWrite(2, 0);
        delay(250);
    }
    Serial.println("SmartConfig done.");
    */

    WiFi.begin(SECRET_WIFI_SSID, SECRET_WIFI_PASSWORD);

    Serial.println("Waiting for WiFi");
    while (WiFi.status() != WL_CONNECTED)
    {
        Serial.print(".");
        ledcWrite(1, 255);
        delay(250);
        ledcWrite(1, 0);
        delay(250);
    }

    Serial.println("WiFi Connected.");
    Serial.print("IP Address: ");
    Serial.println(WiFi.localIP());
    Serial.print("IPv6 address: ");
    Serial.println(WiFi.localIPv6());
}

void initTime()
{
    time_t epochTime;

    configTime(0, 0, "time.google.com");

    while (true)
    {
        epochTime = time(NULL);

        if (epochTime == 0)
        {
            Serial.println("Fetching NTP epoch time failed! Waiting 2 seconds to retry.");
            ledcWrite(2, 255);
            delay(1000);
            ledcWrite(2, 0);
            delay(1000);
        }
        else
        {
            Serial.print("Fetched NTP epoch time is: ");
            Serial.println(epochTime);
            break;
        }
    }
}

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
