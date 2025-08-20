using System;
using System.Collections;
using System.Text;
using UnityEngine;
using NativeWebSocket;
using System.Net.WebSockets;
using WebSocket = NativeWebSocket.WebSocket;
using WebSocketState = NativeWebSocket.WebSocketState;

/// <summary>
/// Клиент для WebSocket соединения с сервером
/// Обеспечивает подключение, отправку и получение сообщений
/// </summary>
public class WebSocketClient : MonoBehaviour
{
    [Header("Connection Settings")]
    public string serverUrl = "ws://localhost:8000/api/ws/";  // URL сервера WebSocket
    public float reconnectDelay = 3f;        // Задержка между попытками переподключения
    public int maxReconnectAttempts = 5;     // Максимальное количество попыток переподключения

    private WebSocket _webSocket;            // Экземпляр WebSocket
    private string _clientId;                // Уникальный идентификатор клиента
    private bool _isConnecting;              // Флаг процесса подключения
    private int _reconnectAttempts;          // Счетчик попыток переподключения

    // Свойства для доступа к состоянию соединения
    public bool IsConnected => _webSocket?.State == WebSocketState.Open;
    public string ClientId => _clientId;

    /// <summary>
    /// Инициализация при старте
    /// </summary>
    private void Start()
    {
        GenerateClientId();  // Генерация уникального ID клиента
        Connect();           // Подключение к серверу
    }

    /// <summary>
    /// Генерация уникального идентификатора клиента
    /// </summary>
    private void GenerateClientId()
    {
        _clientId = $"unity-client-{System.Guid.NewGuid().ToString().Substring(0, 8)}";
        Debug.Log($"Generated client ID: {_clientId}");
    }

    /// <summary>
    /// Подключение к WebSocket серверу
    /// </summary>
    public async void Connect()
    {
        if (_isConnecting) return;  // Если уже подключаемся, выходим

        _isConnecting = true;

        try
        {
            // Закрываем существующее соединение, если есть
            if (_webSocket != null)
            {
                await _webSocket.Close();
                _webSocket = null;
            }

            string connectionUrl = $"{serverUrl}{_clientId}";
            Debug.Log($"Connecting to WebSocket: {connectionUrl}");

            _webSocket = new WebSocket(connectionUrl);

            // Настройка обработчиков событий
            _webSocket.OnOpen += OnWebSocketOpen;
            _webSocket.OnError += OnWebSocketError;
            _webSocket.OnClose += OnWebSocketClose;
            _webSocket.OnMessage += OnWebSocketMessage;

            await _webSocket.Connect();  // Установка соединения
            _reconnectAttempts = 0;      // Сброс счетчика попыток переподключения
        }
        catch (Exception ex)
        {
            Debug.LogError($"WebSocket connection failed: {ex.Message}");
            HandleConnectionFailure(ex.Message);
        }
        finally
        {
            _isConnecting = false;
        }
    }

    /// <summary>
    /// Обработчик успешного подключения
    /// </summary>
    private void OnWebSocketOpen()
    {
        Debug.Log("WebSocket connected successfully");
        EventSystem.TriggerWebSocketConnected();
        EventSystem.TriggerStatusMessage("Connected to server");
    }

    /// <summary>
    /// Обработчик ошибок WebSocket
    /// </summary>
    private void OnWebSocketError(string errorMsg)
    {
        Debug.LogError($"WebSocket error: {errorMsg}");
        EventSystem.TriggerWebSocketError(errorMsg);
        EventSystem.TriggerStatusMessage($"Connection error: {errorMsg}");
    }

    /// <summary>
    /// Обработчик закрытия соединения
    /// </summary>
    private void OnWebSocketClose(WebSocketCloseCode closeCode)
    {
        string reason = $"WebSocket closed: {closeCode}";
        Debug.Log(reason);
        EventSystem.TriggerWebSocketDisconnected(reason);
        EventSystem.TriggerStatusMessage("Disconnected from server");

        // Попытка переподключения, если соединение закрыто не нормально
        if (closeCode != WebSocketCloseCode.Normal)
        {
            AttemptReconnect();
        }
    }

    /// <summary>
    /// Обработчик входящих сообщений
    /// </summary>
    private void OnWebSocketMessage(byte[] data)
    {
        string message = Encoding.UTF8.GetString(data);
        Debug.Log($"Received message: {message}");

        // TODO: Обработка сообщения с помощью MessageHandler
    }

    /// <summary>
    /// Отправка сообщения на сервер
    /// </summary>
    /// <param name="message">Сообщение для отправки</param>
    public async void SendMessage(string message)
    {
        if (_webSocket?.State == WebSocketState.Open)
        {
            try
            {
                await _webSocket.SendText(message);
                Debug.Log($"Sent message: {message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to send message: {ex.Message}");
                EventSystem.TriggerApplicationError("Failed to send message", ErrorSeverity.Medium);
            }
        }
        else
        {
            Debug.LogWarning("Cannot send message - WebSocket is not connected");
            EventSystem.TriggerApplicationError("Not connected to server", ErrorSeverity.Low);
        }
    }

    /// <summary>
    /// Попытка переподключения к серверу
    /// </summary>
    private void AttemptReconnect()
    {
        if (_reconnectAttempts < maxReconnectAttempts)
        {
            _reconnectAttempts++;
            Debug.Log($"Attempting to reconnect ({_reconnectAttempts}/{maxReconnectAttempts})...");
            EventSystem.TriggerStatusMessage($"Reconnecting... ({_reconnectAttempts}/{maxReconnectAttempts})");

            Invoke(nameof(Connect), reconnectDelay);  // Повторная попытка через задержку
        }
        else
        {
            Debug.LogError("Max reconnection attempts reached");
            EventSystem.TriggerApplicationError("Failed to connect to server after multiple attempts", ErrorSeverity.High);
        }
    }

    /// <summary>
    /// Обработчик неудачного подключения
    /// </summary>
    private void HandleConnectionFailure(string error)
    {
        EventSystem.TriggerApplicationError($"Connection failed: {error}", ErrorSeverity.Medium);
        AttemptReconnect();
    }

    /// <summary>
    /// Обновление состояния WebSocket (обработка очереди сообщений)
    /// </summary>
    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (_webSocket != null)
        {
            _webSocket.DispatchMessageQueue();
        }
#endif
    }

    /// <summary>
    /// Закрытие соединения при выходе из приложения
    /// </summary>
    private async void OnApplicationQuit()
    {
        if (_webSocket != null)
        {
            await _webSocket.Close();
        }
    }
}