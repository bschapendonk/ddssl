#include <Arduino.h>

void setup()
{
    ledcSetup(1, 10000, 8);
    ledcSetup(2, 10000, 8);
    ledcSetup(3, 10000, 8);

    ledcAttachPin(25, 1);
    ledcAttachPin(26, 2);
    ledcAttachPin(27, 3);

    int ledArray[3] = {1, 2, 3};
    for (int i = 0; i < 3; i++)
    {
        ledcWrite(ledArray[i], 255);
        delay(1000);
        ledcWrite(ledArray[i], 0);
    }

    Serial.begin(115200);
}

void loop()
{
    byte buffer[3];
    if (Serial.available() > 0)
    {
        Serial.readBytes(buffer, 3);
        ledcWrite(1, buffer[0]);
        ledcWrite(2, buffer[1]);
        ledcWrite(3, buffer[2]);
    }
}
