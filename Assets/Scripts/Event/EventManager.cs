using System;
using System.Collections.Generic;
using UnityEngine;

// Базовый класс для всех типов событий
public abstract class GameEvent { }

// Конкретные классы событий
public class GPIOStateChangedEvent : GameEvent
{
    public int PinNumber { get; }
    public bool IsActive { get; }

    public GPIOStateChangedEvent(int pinNumber, bool isActive)
    {
        PinNumber = pinNumber;
        IsActive = isActive;
    }
}

public class ExecutionStartedEvent : GameEvent { }
public class ExecutionStoppedEvent : GameEvent { }

public class RawMessageEvent : GameEvent
{
    public string Message { get; }
    public RawMessageEvent(string message)
    {
        Message = message;
    }
}
public class ConnectionStatusEvent : GameEvent
{
    public bool IsConnected { get; }

    public ConnectionStatusEvent(bool isConnected)
    {
        IsConnected = isConnected;
    }
}

public class EventManager : MonoBehaviour
{
    private static EventManager _instance;
    public static EventManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<EventManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("EventManager");
                    _instance = go.AddComponent<EventManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    // Словарь для хранения подписок: тип события -> список обработчиков
    private Dictionary<Type, List<Delegate>> _eventSubscriptions = new Dictionary<Type, List<Delegate>>();

    // Метод для подписки на событие
    public void Subscribe<T>(Action<T> handler) where T : GameEvent
    {
        Type eventType = typeof(T);
        if (!_eventSubscriptions.ContainsKey(eventType))
        {
            _eventSubscriptions[eventType] = new List<Delegate>();
        }
        _eventSubscriptions[eventType].Add(handler);
    }

    // Метод для отписки от события
    public void Unsubscribe<T>(Action<T> handler) where T : GameEvent
    {
        Type eventType = typeof(T);
        if (_eventSubscriptions.ContainsKey(eventType))
        {
            _eventSubscriptions[eventType].Remove(handler);
            if (_eventSubscriptions[eventType].Count == 0)
            {
                _eventSubscriptions.Remove(eventType);
            }
        }
    }

    // Метод для публикации события
    public void Publish<T>(T gameEvent) where T : GameEvent
    {
        Type eventType = typeof(T);
        if (_eventSubscriptions.ContainsKey(eventType))
        {
            // Проходим по копии списка, чтобы избежать проблем, если обработчики отписываются во время итерации
            var handlers = _eventSubscriptions[eventType].ToArray();
            foreach (Delegate delegateHandler in handlers)
            {
                // Проверяем, не был ли обработчик удален из оригинального списка после создания копии
                if (_eventSubscriptions.ContainsKey(eventType) && _eventSubscriptions[eventType].Contains(delegateHandler))
                {
                    ((Action<T>)delegateHandler)?.Invoke(gameEvent);
                }
            }
        }
    }

    void OnDestroy()
    {
        _eventSubscriptions.Clear();
    }
}