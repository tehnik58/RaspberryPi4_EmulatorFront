using System.Collections.Generic;
using UnityEngine;

public class DigitalTwinManager : MonoBehaviour
{
    // Словарь для хранения состояния всех GPIO пинов
    public Dictionary<int, bool> PinStates { get; private set; } = new Dictionary<int, bool>();

    public bool IsConnected { get; private set; } = false;

    private void OnEnable()
    {
        // Подписываемся на все события, которые нас интересуют
        EventManager.Instance.Subscribe<GPIOStateChangedEvent>(OnGPIOStateChanged);
        EventManager.Instance.Subscribe<ConnectionStatusEvent>(OnConnectionStatusChanged);
        EventManager.Instance.Subscribe<ExecutionStartedEvent>(OnExecutionStarted);
    }

    private void OnDisable()
    {
        // Отписываемся от всех событий
        EventManager.Instance.Unsubscribe<GPIOStateChangedEvent>(OnGPIOStateChanged);
        EventManager.Instance.Unsubscribe<ConnectionStatusEvent>(OnConnectionStatusChanged);
        EventManager.Instance.Unsubscribe<ExecutionStartedEvent>(OnExecutionStarted);
    }

    private void OnGPIOStateChanged(GPIOStateChangedEvent eventData)
    {
        PinStates[eventData.PinNumber] = eventData.IsActive;
        //Debug.Log($"Digital Twin: Pin {eventData.PinNumber} set to {eventData.IsActive}");
        // Здесь можно добавить любую другую логику, которая должна реагировать на изменение состояния пина
    }

    private void OnConnectionStatusChanged(ConnectionStatusEvent eventData)
    {
        IsConnected = eventData.IsConnected;
        //Debug.Log($"Digital Twin: Connection status changed to {IsConnected}");
    }

    private void OnExecutionStarted(ExecutionStartedEvent eventData)
    {
        Debug.Log("Digital Twin: Execution started");
        // сбросить состояние двойника перед началом нового выполнения
        PinStates.Clear();
    }

    // Метод для запроса текущего состояния пина у двойника
    public bool GetPinState(int pinNumber)
    {
        if (PinStates.TryGetValue(pinNumber, out bool state))
        {
            return state;
        }
        return false; // false по умолчанию, если состояние пина ещё не известно
    }
}