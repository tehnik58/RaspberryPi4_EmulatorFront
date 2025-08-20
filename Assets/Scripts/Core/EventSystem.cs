using System;
using UnityEngine;

/// <summary>
/// Уровни серьезности ошибок приложения
/// </summary>
public enum ErrorSeverity
{
    Low,        // Незначительная ошибка, не влияющая на работу
    Medium,     // Средняя ошибка, частично влияющая на функциональность
    High,       // Серьезная ошибка, ограничивающая функциональность
    Critical    // Критическая ошибка, приложение не может работать
}

/// <summary>
/// Централизованная система событий для коммуникации между компонентами
/// Реализует шаблон Observer для松散 связанных компонентов
/// </summary>
public static class EventSystem
{
    // События приложения
    public static event Action<string, ErrorSeverity> OnApplicationError;
    public static event Action<string> OnStatusMessage;

    // События WebSocket
    public static event Action OnWebSocketConnected;
    public static event Action<string> OnWebSocketDisconnected;
    public static event Action<string> OnWebSocketError;

    // События состояния игры
    public static event Action<GameState, GameState> OnGameStateChanged;

    // Методы для вызова событий

    /// <summary>
    /// Вызов события ошибки приложения
    /// </summary>
    public static void TriggerApplicationError(string message, ErrorSeverity severity)
    {
        OnApplicationError?.Invoke(message, severity);
    }

    /// <summary>
    /// Вызов события статусного сообщения
    /// </summary>
    public static void TriggerStatusMessage(string message)
    {
        OnStatusMessage?.Invoke(message);
    }

    /// <summary>
    /// Вызов события подключения WebSocket
    /// </summary>
    public static void TriggerWebSocketConnected()
    {
        OnWebSocketConnected?.Invoke();
    }

    /// <summary>
    /// Вызов события отключения WebSocket
    /// </summary>
    public static void TriggerWebSocketDisconnected(string reason)
    {
        OnWebSocketDisconnected?.Invoke(reason);
    }

    /// <summary>
    /// Вызов события ошибки WebSocket
    /// </summary>
    public static void TriggerWebSocketError(string error)
    {
        OnWebSocketError?.Invoke(error);
    }

    /// <summary>
    /// Вызов события изменения состояния игры
    /// </summary>
    public static void TriggerGameStateChanged(GameState oldState, GameState newState)
    {
        OnGameStateChanged?.Invoke(oldState, newState);
    }
}