using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaspberryEmulatorUI : MonoBehaviour
{
    public WebSocketClient webSocketClient;
    public TMP_InputField codeInputField;
    public Button executeButton;
    public Button stopButton;
    public TMP_Text outputText;
    public ScrollRect outputScrollRect;
    
    // Визуализация GPIO пинов
    public GpioPinVisualization[] gpioPins;
    
    void Start()
    {
        executeButton.onClick.AddListener(ExecuteCode);
        stopButton.onClick.AddListener(StopExecution);
        
        // Подписываемся на события WebSocket клиента
        webSocketClient.OnOutputReceived += HandleOutput;
        webSocketClient.OnErrorReceived += HandleError;
        webSocketClient.OnExecutionStarted += HandleExecutionStarted;
        webSocketClient.OnExecutionCompleted += HandleExecutionCompleted;
        webSocketClient.OnGpioStateUpdated += HandleGpioStateUpdate;
        webSocketClient.OnPwmStateUpdated += HandlePwmStateUpdate;
        webSocketClient.OnSensorDataUpdated += HandleSensorDataUpdate;
    }
    
    void ExecuteCode()
    {
        if (string.IsNullOrEmpty(codeInputField.text)) return;
        
        webSocketClient.SendExecuteCode(codeInputField.text);
    }
    
    void StopExecution()
    {
        webSocketClient.SendStopExecution();
    }
    
    void HandleOutput(string output)
    {
        // Разделяем сообщение на строки
        string[] lines = output.Split('\n');
    
        foreach (string line in lines)
        {
            if (string.IsNullOrEmpty(line.Trim())) continue;
        
            // Проверяем, не является ли строка EMU_EVENT (чтобы не дублировать вывод)
            if (line.Contains("@@EMU_EVENT:"))
            {
                // EMU_EVENT уже обработан отдельно, пропускаем дублирование
                continue;
            }
        
            // Проверяем, не является ли строка автоматическим PWM выводом
            if (line.Contains("PWM duty cycle changed to") && line.Contains("on pin"))
            {
                // PWM изменения уже обработаны отдельно, пропускаем дублирование
                continue;
            }
        
            outputText.text += $"\n{line}";
        }
    
        Canvas.ForceUpdateCanvases();
        outputScrollRect.verticalNormalizedPosition = 0f;
    }
    
    void HandleError(string error)
    {
        outputText.text += $"\n<color=red>ERROR: {error}</color>";
        Canvas.ForceUpdateCanvases();
        outputScrollRect.verticalNormalizedPosition = 0f;
    }
    
    void HandleExecutionStarted(string message)
    {
        outputText.text += $"\n<color=green>{message}</color>";
    }
    
    void HandleExecutionCompleted(string message)
    {
        outputText.text += $"\n<color=green>{message}</color>";
    }
    
    void HandleGpioStateUpdate(GpioStateUpdate update)
    {
        // Обновление UI состояния GPIO пинов
        Debug.Log($"GPIO {update.pin} ({update.mode}): {update.state}");
        
        // Визуальное обновление состояния пина
        foreach (var pinVisual in gpioPins)
        {
            if (pinVisual.pinNumber == update.pin)
            {
                pinVisual.SetState(update.state);
                break;
            }
        }
        
        // Также выводим в лог
        outputText.text += $"\n<color=blue>GPIO {update.pin} {update.mode}: {update.state}</color>";
        Canvas.ForceUpdateCanvases();
        outputScrollRect.verticalNormalizedPosition = 0f;
    }
    
    void HandlePwmStateUpdate(PwmStateUpdate update)
    {
        // Обновление UI состояния PWM
        //Debug.Log($"PWM Pin {update.pin}: Duty Cycle={update.duty_cycle}%, Frequency={update.frequency}Hz");
        
        outputText.text += $"\n<color=purple>PWM {update.pin}: Duty Cycle={update.duty_cycle}%, Frequency={update.frequency}Hz</color>";
        Canvas.ForceUpdateCanvases();
        outputScrollRect.verticalNormalizedPosition = 0f;
    }
    
    void HandleSensorDataUpdate(SensorDataUpdate update)
    {
        // Обновление UI данных с датчиков
        Debug.Log($"Sensor {update.sensor}: {update.value} {update.unit}");
        
        outputText.text += $"\n<color=orange>{update.sensor}: {update.value} {update.unit}</color>";
        Canvas.ForceUpdateCanvases();
        outputScrollRect.verticalNormalizedPosition = 0f;
    }
    
    void OnDestroy()
    {
        // Отписываемся от событий
        webSocketClient.OnOutputReceived -= HandleOutput;
        webSocketClient.OnErrorReceived -= HandleError;
        webSocketClient.OnExecutionStarted -= HandleExecutionStarted;
        webSocketClient.OnExecutionCompleted -= HandleExecutionCompleted;
        webSocketClient.OnGpioStateUpdated -= HandleGpioStateUpdate;
        webSocketClient.OnPwmStateUpdated -= HandlePwmStateUpdate;
        webSocketClient.OnSensorDataUpdated -= HandleSensorDataUpdate;
    }
}

// Класс для визуализации GPIO пинов
[System.Serializable]
public class GpioPinVisualization
{
    public int pinNumber;
    public Image pinImage;
    public Color activeColor = Color.green;
    public Color inactiveColor = Color.red;
    
    public void SetState(bool isActive)
    {
        if (pinImage != null)
        {
            pinImage.color = isActive ? activeColor : inactiveColor;
        }
    }
}