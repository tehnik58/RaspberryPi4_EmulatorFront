using UnityEngine;

/// <summary>
/// Тестовый скрипт для проверки работы базовой функциональности
/// </summary>
public class StartupTest : MonoBehaviour
{
    /// <summary>
    /// Запуск теста при старте
    /// </summary>
    private void Start()
    {
        Debug.Log("Startup test running...");

        // Тестирование системы событий
        EventSystem.OnStatusMessage += TestStatusMessage;
        EventSystem.TriggerStatusMessage("Startup test completed");

        // Тестирование WebSocket соединения
        WebSocketClient wsClient = FindObjectOfType<WebSocketClient>();
        if (wsClient != null)
        {
            Debug.Log($"WebSocket client found: {wsClient.ClientId}");
        }
    }

    /// <summary>
    /// Обработчик тестового статусного сообщения
    /// </summary>
    private void TestStatusMessage(string message)
    {
        Debug.Log($"Status message received: {message}");
        EventSystem.OnStatusMessage -= TestStatusMessage;  // Отписываемся после получения
    }
}