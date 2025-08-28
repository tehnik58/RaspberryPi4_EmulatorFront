using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public class WebSocketClient : MonoBehaviour
{
    public string serverUrl = "ws://localhost:8000/ws/execute";
    
    private WebSocket websocket;
    private bool isConnected = false;
    
    // События для обработки сообщений от сервера
    public event Action<string> OnOutputReceived;
    public event Action<string> OnErrorReceived;
    public event Action<string> OnExecutionStarted;
    public event Action<string> OnExecutionCompleted;
    public event Action<GpioStateUpdate> OnGpioStateUpdated;
    public event Action<PwmStateUpdate> OnPwmStateUpdated;
    public event Action<SensorDataUpdate> OnSensorDataUpdated;
    
    // Регулярное выражение для парсинга GPIO output
    private Regex gpioOutputRegex = new Regex(@"GPIO\s+(\d+)\s+output:\s*(True|False)", RegexOptions.IgnoreCase);
    
    async void Start()
    {
        await ConnectWebSocket();
    }
    
    async Task ConnectWebSocket()
    {
        websocket = new WebSocket(serverUrl);
        
        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket connected!");
            isConnected = true;
        };
        
        websocket.OnError += (e) =>
        {
            Debug.LogError("WebSocket error: " + e);
            isConnected = false;
        };
        
        websocket.OnClose += (e) =>
        {
            Debug.Log("WebSocket closed: " + e);
            isConnected = false;
        };
        
        websocket.OnMessage += (bytes) =>
        {
            var message = Encoding.UTF8.GetString(bytes);
            HandleServerMessage(message);
        };
        
        await websocket.Connect();
    }
    
    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
            if (websocket != null)
                websocket.DispatchMessageQueue();
        #endif
    }
    
    async void OnDestroy()
    {
        if (websocket != null)
            await websocket.Close();
    }
    
    // Отправка сообщений на сервер
    public async void SendExecuteCode(string code)
    {
        if (!isConnected) return;
        
        string escapedCode = EscapeJsonString(code);
        string message = $"{{\"type\": \"execute\", \"code\": \"{escapedCode}\"}}";
        
        await websocket.SendText(message);
    }
    
    public async void SendStopExecution()
    {
        if (!isConnected) return;
        
        var message = "{\"type\": \"stop\"}";
        await websocket.SendText(message);
    }
    
    public async void SendGpioInput(int pin, bool state)
    {
        if (!isConnected) return;
        
        var message = $"{{\"type\": \"gpio_input\", \"pin\": {pin}, \"state\": {state.ToString().ToLower()}}}";
        await websocket.SendText(message);
    }
    
    public async void SendPwmControl(int pin, float dutyCycle, float frequency, string action)
    {
        if (!isConnected) return;
        
        var message = $"{{\"type\": \"pwm_control\", \"pin\": {pin}, \"duty_cycle\": {dutyCycle}, \"frequency\": {frequency}, \"action\": \"{action}\"}}";
        await websocket.SendText(message);
    }
    
    // Обработка входящих сообщений
    private void HandleServerMessage(string jsonMessage)
    {
        try
        {
            jsonMessage = RemoveControlCharacters(jsonMessage);
            
            var message = JsonUtility.FromJson<ServerMessageBase>(jsonMessage);
            
            switch (message.type)
            {
                case "output":
                    var outputMsg = JsonUtility.FromJson<OutputMessage>(jsonMessage);
                    HandleOutputMessage(outputMsg.content);
                    break;
                    
                case "error":
                    var errorMsg = JsonUtility.FromJson<ErrorMessage>(jsonMessage);
                    OnErrorReceived?.Invoke(errorMsg.content);
                    break;
                    
                case "execution_started":
                    var startedMsg = JsonUtility.FromJson<ExecutionStartedMessage>(jsonMessage);
                    OnExecutionStarted?.Invoke(startedMsg.message);
                    break;
                    
                case "execution_completed":
                    var completedMsg = JsonUtility.FromJson<ExecutionCompletedMessage>(jsonMessage);
                    OnExecutionCompleted?.Invoke(completedMsg.message);
                    break;
                    
                case "gpio_state_update":
                    var gpioMsg = JsonUtility.FromJson<GpioStateUpdate>(jsonMessage);
                    OnGpioStateUpdated?.Invoke(gpioMsg);
                    break;
                    
                case "pwm_state_update":
                    var pwmMsg = JsonUtility.FromJson<PwmStateUpdate>(jsonMessage);
                    print(pwmMsg);
                    OnPwmStateUpdated?.Invoke(pwmMsg);
                    break;
                    
                case "sensor_data_update":
                    var sensorMsg = JsonUtility.FromJson<SensorDataUpdate>(jsonMessage);
                    OnSensorDataUpdated?.Invoke(sensorMsg);
                    break;
                    
                default:
                    Debug.Log("Unknown message type: " + message.type);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing server message: " + e.Message + "\nJSON: " + jsonMessage);
        }
    }
    
    // Обработка output сообщений с парсингом GPIO output
    private void HandleOutputMessage(string output)
    {
        // Пытаемся распарсить GPIO output
        Match match = gpioOutputRegex.Match(output);
        if (match.Success)
        {
            int pin = int.Parse(match.Groups[1].Value);
            bool state = match.Groups[2].Value.ToLower() == "true";
            
            // Создаем событие обновления GPIO
            var gpioUpdate = new GpioStateUpdate
            {
                pin = pin,
                state = state,
                mode = "output"
            };
            
            OnGpioStateUpdated?.Invoke(gpioUpdate);
        }
        else
        {
            // Если не GPIO output, отправляем как обычный вывод
            OnOutputReceived?.Invoke(output);
        }
    }
    
    // Правильное экранирование строк для JSON
    private string EscapeJsonString(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        
        StringBuilder sb = new StringBuilder();
        foreach (char c in input)
        {
            switch (c)
            {
                case '\\': sb.Append("\\\\"); break;
                case '\"': sb.Append("\\\""); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < ' ')
                    {
                        sb.AppendFormat("\\u{0:X4}", (int)c);
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }
        return sb.ToString();
    }
    
    // Удаление управляющих символов из JSON
    private string RemoveControlCharacters(string input)
    {
        return Regex.Replace(input, @"[\x00-\x1F\x7F]", string.Empty);
    }
}

// Классы для десериализации сообщений
[System.Serializable]
public class ServerMessageBase
{
    public string type;
}

[System.Serializable]
public class OutputMessage : ServerMessageBase
{
    public string content;
}

[System.Serializable]
public class ErrorMessage : ServerMessageBase
{
    public string content;
}

[System.Serializable]
public class ExecutionStartedMessage : ServerMessageBase
{
    public string message;
}

[System.Serializable]
public class ExecutionCompletedMessage : ServerMessageBase
{
    public string message;
}

[System.Serializable]
public class GpioStateUpdate : ServerMessageBase
{
    public int pin;
    public bool state;
    public string mode;
}

[System.Serializable]
public class PwmStateUpdate : ServerMessageBase
{
    public int pin;
    public float duty_cycle;
    public float frequency;
}

[System.Serializable]
public class SensorDataUpdate : ServerMessageBase
{
    public string sensor;
    public float value;
    public string unit;
}