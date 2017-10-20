#include <Arduino.h>
#include <AzureIoTHub.h>

#include <secrets.h>

BEGIN_NAMESPACE(DeveloperDeskStatusStackLight);

DECLARE_MODEL(StackLight,
              WITH_ACTION(SetRed, uint8_t, Brightness),
              WITH_ACTION(SetOrange, uint8_t, Brightness),
              WITH_ACTION(SetGreen, uint8_t, Brightness));

END_NAMESPACE(DeveloperDeskStatusStackLight);

EXECUTE_COMMAND_RESULT SetGreen(StackLight *device, uint8_t Brightness)
{
    device;
    ledcWrite(1, Brightness);
    printf("Setting Green Brightness to %d.\r\n", Brightness);
    return EXECUTE_COMMAND_SUCCESS;
}

EXECUTE_COMMAND_RESULT SetOrange(StackLight *device, uint8_t Brightness)
{
    device;
    ledcWrite(2, Brightness);
    printf("Setting Orange Brightness to %d.\r\n", Brightness);
    return EXECUTE_COMMAND_SUCCESS;
}

EXECUTE_COMMAND_RESULT SetRed(StackLight *device, uint8_t Brightness)
{
    device;
    ledcWrite(3, Brightness);
    printf("Setting Red Brightness to %d.\r\n", Brightness);
    return EXECUTE_COMMAND_SUCCESS;
}

static IOTHUBMESSAGE_DISPOSITION_RESULT IoTHubMessage(IOTHUB_MESSAGE_HANDLE message, void *userContextCallback)
{
    IOTHUBMESSAGE_DISPOSITION_RESULT result;
    const unsigned char *buffer;
    size_t size;
    if (IoTHubMessage_GetByteArray(message, &buffer, &size) != IOTHUB_MESSAGE_OK)
    {
        printf("unable to IoTHubMessage_GetByteArray\r\n");
        result = IOTHUBMESSAGE_ABANDONED;
    }
    else
    {
        /*buffer is not zero terminated*/
        char *temp = malloc(size + 1);
        if (temp == NULL)
        {
            printf("failed to malloc\r\n");
            result = IOTHUBMESSAGE_ABANDONED;
        }
        else
        {
            memcpy(temp, buffer, size);
            temp[size] = '\0';
            EXECUTE_COMMAND_RESULT executeCommandResult = EXECUTE_COMMAND(userContextCallback, temp);
            result =
                (executeCommandResult == EXECUTE_COMMAND_ERROR) ? IOTHUBMESSAGE_ABANDONED : (executeCommandResult == EXECUTE_COMMAND_SUCCESS) ? IOTHUBMESSAGE_ACCEPTED : IOTHUBMESSAGE_REJECTED;
            free(temp);
        }
    }
    return result;
}

void iothub_run()
{
    if (platform_init() != 0)
    {
        printf("Failed to initialize platform.\r\n");
    }
    else
    {
        if (serializer_init(NULL) != SERIALIZER_OK)
        {
            printf("Failed on serializer_init\r\n");
        }
        else
        {
            IOTHUB_CLIENT_LL_HANDLE iotHubClientHandle = IoTHubClient_LL_CreateFromConnectionString(SECRET_DEVICE_CONNECTIONSTRING, MQTT_Protocol);

            if (iotHubClientHandle == NULL)
            {
                printf("Failed on IoTHubClient_LL_Create\r\n");
            }
            else
            {
                StackLight *stackLight = CREATE_MODEL_INSTANCE(DeveloperDeskStatusStackLight, StackLight);
                if (stackLight == NULL)
                {
                    printf("Failed on CREATE_MODEL_INSTANCE\r\n");
                }
                else
                {
                    if (IoTHubClient_LL_SetMessageCallback(iotHubClientHandle, IoTHubMessage, stackLight) != IOTHUB_CLIENT_OK)
                    {
                        printf("unable to IoTHubClient_SetMessageCallback\r\n");
                    }
                    else
                    {
                        while (1)
                        {
                            IoTHubClient_LL_DoWork(iotHubClientHandle);
                            ThreadAPI_Sleep(100);
                        }
                    }

                    DESTROY_MODEL_INSTANCE(stackLight);
                }
                IoTHubClient_LL_Destroy(iotHubClientHandle);
            }
            serializer_deinit();
        }
        platform_deinit();
    }
}