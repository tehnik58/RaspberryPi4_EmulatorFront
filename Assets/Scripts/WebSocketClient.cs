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
    public event Action<EmuEvent> OnEmuEventReceived;
    
    // Регулярные выражения для парсинга
    private Regex gpioOutputRegex = new Regex(@"GPIO\s+(\d+)\s+output:\s*(True|False)", RegexOptions.IgnoreCase);
    private Regex emuEventRegex = new Regex(@"@@EMU_EVENT:(\{.*?\})");
    private Regex pwmDutyRegex = new Regex(@"PWM duty cycle changed to (\d+)% on pin (\d+)", RegexOptions.IgnoreCase);
    
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
            print(message.type);
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
                    OnPwmStateUpdated?.Invoke(pwmMsg);
                    break;
                    
                case "sensor_data_update":
                    var sensorMsg = JsonUtility.FromJson<SensorDataUpdate>(jsonMessage);
                    OnSensorDataUpdated?.Invoke(sensorMsg);
                    break;
                    
                case "emu_event":
                    var emuEventMsg = JsonUtility.FromJson<EmuEventMessage>(jsonMessage);
                    print(emuEventMsg.@event);
                    OnEmuEventReceived?.Invoke(emuEventMsg.@event);
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
    
    // Обработка output сообщений с парсингом различных форматов
    private void HandleOutputMessage(string output)
    {
        // Разделяем сообщение на строки для обработки
        string[] lines = output.Split('\n');
        
        foreach (string line in lines)
        {
            if (string.IsNullOrEmpty(line.Trim())) continue;
            
            // Пытаемся найти EMU_EVENT
            Match emuMatch = emuEventRegex.Match(line);
            if (emuMatch.Success)
            {
                HandleEmuEvent(emuMatch.Groups[1].Value);
                continue;
            }
            
            // Пытаемся распарсить GPIO output
            Match gpioMatch = gpioOutputRegex.Match(line);
            if (gpioMatch.Success)
            {
                int pin = int.Parse(gpioMatch.Groups[1].Value);
                bool state = gpioMatch.Groups[2].Value.ToLower() == "true";
                
                var gpioUpdate = new GpioStateUpdate
                {
                    pin = pin,
                    state = state,
                    mode = "output"
                };
                
                OnGpioStateUpdated?.Invoke(gpioUpdate);
                continue;
            }
            
            // Пытаемся распарсить PWM duty cycle изменения
            Match pwmMatch = pwmDutyRegex.Match(line);
            if (pwmMatch.Success)
            {
                float dutyCycle = float.Parse(pwmMatch.Groups[1].Value);
                int pin = int.Parse(pwmMatch.Groups[2].Value);
                
                // Создаем PWM событие на основе текстового вывода
                var pwmUpdate = new PwmStateUpdate
                {
                    pin = pin,
                    duty_cycle = dutyCycle,
                    frequency = 100 // Предполагаемая частота по умолчанию
                };
                
                OnPwmStateUpdated?.Invoke(pwmUpdate);
                continue;
            }
            
            // Если не нашли специальных форматов, отправляем как обычный вывод
            OnOutputReceived?.Invoke(line);
        }
    }
    
    // Обработка EMU_EVENT событий
    private void HandleEmuEvent(string jsonEvent)
    {
        try
        {
            var emuEvent = JsonUtility.FromJson<EmuEvent>(jsonEvent);
            OnEmuEventReceived?.Invoke(emuEvent);
            
            // Обработка конкретных типов событий
            switch (emuEvent.type)
            {
                case "pwm_event":
                    HandlePwmEvent(emuEvent);
                    break;
                    
                case "gpio_event":
                    HandleGpioEvent(emuEvent);
                    break;
                    
                case "sensor_event":
                    HandleSensorEvent(emuEvent);
                    break;
                    
                default:
                    Debug.Log("Unknown EMU event type: " + emuEvent.type);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing EMU_EVENT: " + e.Message + "\nJSON: " + jsonEvent);
        }
    }
    
    // Обработка PWM событий
    private void HandlePwmEvent(EmuEvent emuEvent)
    {
        var pwmUpdate = new PwmStateUpdate
        {
            pin = emuEvent.pin,
            frequency = emuEvent.frequency,
            duty_cycle = emuEvent.duty_cycle
        };
        
        OnPwmStateUpdated?.Invoke(pwmUpdate);
        
        Debug.Log($"PWM Event: {emuEvent.@event} - Pin {emuEvent.pin}, " +
                 $"Duty Cycle: {emuEvent.duty_cycle}%, Frequency: {emuEvent.frequency}Hz");
    }
    
    // Обработка GPIO событий
    private void HandleGpioEvent(EmuEvent emuEvent)
    {
        var gpioUpdate = new GpioStateUpdate
        {
            pin = emuEvent.pin,
            state = emuEvent.state,
            mode = emuEvent.mode
        };
        OnGpioStateUpdated?.Invoke(gpioUpdate);
    }
    
    // Обработка сенсорных событий
    private void HandleSensorEvent(EmuEvent emuEvent)
    {
        var sensorUpdate = new SensorDataUpdate
        {
            sensor = emuEvent.sensor_type,
            value = emuEvent.value,
            unit = emuEvent.unit
        };
        OnSensorDataUpdated?.Invoke(sensorUpdate);
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

// Классы для десериализации сообщений (остаются без изменений)
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

[System.Serializable]
public class EmuEventMessage : ServerMessageBase
{
    public EmuEvent @event;
}

[System.Serializable]
public class EmuEvent
{
    public string type;
    public string @event;
    public int pin;
    public float frequency;
    public float duty_cycle;
    public bool state;
    public string mode;
    public string sensor_type;
    public float value;
    public string unit;
}