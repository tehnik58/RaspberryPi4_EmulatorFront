using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public class RaspberryPiEmulator : MonoBehaviour
{
    private WebSocketManager webSocketManager;
    private ConsoleOutput consoleOutput;

    [SerializeField] private string initialCode = @"import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BCM)
GPIO.setup(18, GPIO.OUT)

try:
    for i in range(10):
        GPIO.output(18, GPIO.HIGH)
        time.sleep(0.5)
        GPIO.output(18, GPIO.LOW)
        time.sleep(0.5)
finally:
    GPIO.cleanup()";

    private void Start()
    {
        InitializeComponents();
        _ = ConnectToServerAsync(); // discard для async void
    }

    private void InitializeComponents()
    {
        // Сначала находим или создаем WebSocketManager
        webSocketManager = GetComponent<WebSocketManager>();
        if (webSocketManager == null)
        {
            webSocketManager = gameObject.AddComponent<WebSocketManager>();
            Debug.Log("WebSocketManager component added");
        }

        // Затем находим остальные компоненты
        consoleOutput = FindObjectOfType<ConsoleOutput>();

        // Проверяем что все компоненты найдены
        if (consoleOutput == null)
            Debug.LogError("ConsoleOutput not found!");
        else
            Debug.Log("ConsoleOutput found");

        // Подписываемся на события
        webSocketManager.OnMessageReceived += HandleWebSocketMessage;
        webSocketManager.OnConnected += OnConnected;
        webSocketManager.OnDisconnected += OnDisconnected;
        webSocketManager.OnError += OnError;

        Debug.Log("All components initialized successfully");
    }

    public async Task ConnectToServerAsync()
    {
        Debug.Log("Connecting to server...");
        
        // Проверяем доступность сервера перед подключением
        bool serverAvailable = await webSocketManager.CheckServerAvailability();
        
        if (!serverAvailable)
        {
            Debug.LogError("Server is not available");
            OnError("Сервер не доступен. Запустите серверную часть на порту 8000");
            return;
        }

        Debug.Log("Server is available, attempting connection...");
        await webSocketManager.ConnectAsync();
    }

    public void ExecuteCode(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            Debug.Log("Empty code");
            if (consoleOutput != null)
            {
                consoleOutput.AddMessage("Ошибка: Пустой код", ConsoleOutput.MessageType.Error);
            }
            return;
        }

        if (webSocketManager == null || !webSocketManager.IsConnected)
        {
            Debug.Log("WebSocket not connected");
            if (consoleOutput != null)
            {
                consoleOutput.AddMessage("Ошибка: Нет соединения с сервером", ConsoleOutput.MessageType.Error);
            }
            return;
        }

        Debug.Log($"Executing code length: {code.Length} characters");
    
        try
        {
            // формирование JSON
            string jsonMessage = $"{{\"type\":\"execute\",\"code\":{EscapeJsonString(code)}}}";
        
            Debug.Log($"Sending JSON: {jsonMessage}");
            webSocketManager.SendMessage(jsonMessage);
        
            if (consoleOutput != null)
            {
                consoleOutput.AddMessage("Код отправлен на выполнение", ConsoleOutput.MessageType.Success);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Send error: {ex.Message}");
            if (consoleOutput != null)
            {
                consoleOutput.AddMessage($"Ошибка отправки: {ex.Message}", ConsoleOutput.MessageType.Error);
            }
        }
    }

// Метод для экранирования строки JSON
    private string EscapeJsonString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "\"\"";

        //специальные символы JSON
        string escaped = input
            .Replace("\\", "\\\\")  // обратный слеш
            .Replace("\"", "\\\"")  // двойные кавычки
            .Replace("\n", "\\n")   // новая строка
            .Replace("\r", "\\r")   // возврат каретки
            .Replace("\t", "\\t");  // табуляция

        return $"\"{escaped}\"";
    }
    private void DetectGPIOEvents(string output)
    {
        Match outputMatch = Regex.Match(output, @"GPIO\s+(\d+)\s+output:\s+(True|False)", RegexOptions.IgnoreCase);
        if (outputMatch.Success)
        {
            int pin = int.Parse(outputMatch.Groups[1].Value);
            bool state = outputMatch.Groups[2].Value.ToLower() == "true";
            EventManager.Instance.Publish(new GPIOStateChangedEvent(pin, state));
        }
    }
    
    private void HandleWebSocketMessage(string message)
    {
        Debug.Log($"📨 Received: {message}");
        if (consoleOutput != null)
        {
            consoleOutput.AddMessage(message, ConsoleOutput.MessageType.Info);
        }
        DetectGPIOEvents(message);
        EventManager.Instance.Publish(new ConsoleOutput.RawMessageEvent(message));
    }

    private void OnConnected()
    {
        consoleOutput?.AddMessage("Подключено к серверу", ConsoleOutput.MessageType.Success);
        EventManager.Instance.Publish(new ConnectionStatusEvent(true));
    }

    private void OnDisconnected()
    {
        consoleOutput?.AddMessage("Соединение потеряно", ConsoleOutput.MessageType.Error);
        EventManager.Instance.Publish(new ConnectionStatusEvent(false));
    }

    private void OnError(string error)
    {
        Debug.LogError($"Error: {error}");
        if (consoleOutput != null)
        {
            consoleOutput.AddMessage($"⚠ Ошибка: {error}", ConsoleOutput.MessageType.Error);
        }
    }

    public string GetInitialCode()
    {
        return initialCode;
    }

    void OnDestroy()
    {
        // Отписываемся от событий
        if (webSocketManager != null)
        {
            webSocketManager.OnMessageReceived -= HandleWebSocketMessage;
            webSocketManager.OnConnected -= OnConnected;
            webSocketManager.OnDisconnected -= OnDisconnected;
            webSocketManager.OnError -= OnError;
        }
    }
}