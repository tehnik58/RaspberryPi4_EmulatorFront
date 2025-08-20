using System;
using UnityEngine;

/// <summary>
/// Базовый класс для сообщений WebSocket
/// </summary>
[System.Serializable]
public class WebSocketMessage
{
    public string type;      // Тип сообщения
    public string message;   // Текст сообщения
}

/// <summary>
/// Обработчик входящих сообщений от сервера
/// Парсит JSON и распределяет сообщения по соответствующим обработчикам
/// </summary>
public class MessageHandler : MonoBehaviour
{
    /// <summary>
    /// Подписка на события при включении компонента
    /// </summary>
    private void OnEnable()
    {
        // Регистрация обработчиков событий WebSocket
        EventSystem.OnWebSocketConnected += HandleWebSocketConnected;
        EventSystem.OnWebSocketDisconnected += HandleWebSocketDisconnected;
        EventSystem.OnWebSocketError += HandleWebSocketError;
    }

    /// <summary>
    /// Отписка от событий при выключении компонента
    /// </summary>
    private void OnDisable()
    {
        // Отмена регистрации обработчиков событий
        EventSystem.OnWebSocketConnected -= HandleWebSocketConnected;
        EventSystem.OnWebSocketDisconnected -= HandleWebSocketDisconnected;
        EventSystem.OnWebSocketError -= HandleWebSocketError;
    }

    /// <summary>
    /// Основной метод обработки входящих сообщений
    /// </summary>
    /// <param name="jsonMessage">Сообщение в формате JSON</param>
    public void ProcessMessage(string jsonMessage)
    {
        try
        {
            // Парсим базовую структуру сообщения для определения типа
            WebSocketMessage baseMessage = JsonUtility.FromJson<WebSocketMessage>(jsonMessage);

            // Распределяем обработку по типам сообщений
            switch (baseMessage.type)
            {
                case "connection_established":
                    HandleConnectionEstablished(jsonMessage);
                    break;
                case "log":
                    HandleLogMessage(jsonMessage);
                    break;
                case "status":
                    HandleStatusMessage(jsonMessage);
                    break;
                case "error":
                    HandleErrorMessage(jsonMessage);
                    break;
                default:
                    Debug.LogWarning($"Unknown message type: {baseMessage.type}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to process message: {ex.Message}");
            EventSystem.TriggerApplicationError("Failed to process server message", ErrorSeverity.Medium);
        }
    }

    /// <summary>
    /// Обработчик события подключения WebSocket
    /// </summary>
    private void HandleWebSocketConnected()
    {
        Debug.Log("WebSocket connected event received");
        EventSystem.TriggerStatusMessage("Connected to server");
    }

    /// <summary>
    /// Обработчик события отключения WebSocket
    /// </summary>
    private void HandleWebSocketDisconnected(string reason)
    {
        Debug.Log($"WebSocket disconnected: {reason}");
        EventSystem.TriggerStatusMessage($"Disconnected: {reason}");
    }

    /// <summary>
    /// Обработчик события ошибки WebSocket
    /// </summary>
    private void HandleWebSocketError(string error)
    {
        Debug.LogError($"WebSocket error: {error}");
        EventSystem.TriggerApplicationError($"WebSocket error: {error}", ErrorSeverity.Medium);
    }

    /// <summary>
    /// Обработчик сообщения об установлении соединения
    /// </summary>
    private void HandleConnectionEstablished(string jsonMessage)
    {
        // Парсим специфичную структуру сообщения
        ConnectionEstablishedMessage message = JsonUtility.FromJson<ConnectionEstablishedMessage>(jsonMessage);
        Debug.Log($"Connection established: {message.message}");
        EventSystem.TriggerStatusMessage(message.message);
    }

    /// <summary>
    /// Обработчик лог-сообщений от сервера
    /// </summary>
    private void HandleLogMessage(string jsonMessage)
    {
        LogMessage message = JsonUtility.FromJson<LogMessage>(jsonMessage);
        Debug.Log($"Server log: {message.message}");
        // TODO: Пересылка в UI консоль
    }

    /// <summary>
    /// Обработчик статус-сообщений от сервера
    /// </summary>
    private void HandleStatusMessage(string jsonMessage)
    {
        StatusMessage message = JsonUtility.FromJson<StatusMessage>(jsonMessage);
        Debug.Log($"Server status: {message.message}");
        EventSystem.TriggerStatusMessage(message.message);
    }

    /// <summary>
    /// Обработчик сообщений об ошибках от сервера
    /// </summary>
    private void HandleErrorMessage(string jsonMessage)
    {
        ErrorMessage message = JsonUtility.FromJson<ErrorMessage>(jsonMessage);
        Debug.LogError($"Server error: {message.message}");
        EventSystem.TriggerApplicationError($"Server error: {message.message}", ErrorSeverity.Medium);
    }
}

// Классы для парсинга специфичных типов сообщений

/// <summary>
/// Сообщение об установлении соединения
/// </summary>
[System.Serializable]
public class ConnectionEstablishedMessage : WebSocketMessage
{
    public string client_id;  // Идентификатор клиента
}

/// <summary>
/// Лог-сообщение от сервера
/// </summary>
[System.Serializable]
public class LogMessage : WebSocketMessage
{
    public string timestamp;  // Временная метка сообщения
}

/// <summary>
/// Статус-сообщение от сервера
/// </summary>
[System.Serializable]
public class StatusMessage : WebSocketMessage
{
    public string timestamp;  // Временная метка сообщения
}

/// <summary>
/// Сообщение об ошибке от сервера
/// </summary>
[System.Serializable]
public class ErrorMessage : WebSocketMessage
{
    public string code;       // Код ошибки
    public string details;    // Детали ошибки
    public string timestamp;  // Временная метка сообщения
}