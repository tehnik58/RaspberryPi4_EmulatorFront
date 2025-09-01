using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicCarControllerTest : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float accelerationRate = 0.15f;
    [SerializeField] private float decelerationRate = 0.4f;

    [Header("Текущее состояние")]
    [SerializeField] private float currentSpeed;
    [SerializeField] private int forwardPin = -1;
    [SerializeField] private int backwardPin = -1;
    [SerializeField] private int pwmPin = -1;

    [SerializeField] private bool isMovingForward;
    [SerializeField] private bool isEmergencyStop;

    private WebSocketClient webSocketClient;
    private readonly Dictionary<int, bool> gpioStates = new Dictionary<int, bool>();
    private float targetDutyCycle;

    private void Start()
    {
        webSocketClient = FindObjectOfType<WebSocketClient>();

        if (webSocketClient == null)
        {
            Debug.LogError("WebSocketClient не найден на сцене!");
            enabled = false;
            return;
        }

        // Подписываемся на события от WebSocketClient
        webSocketClient.OnGpioStateUpdated += HandleGpioUpdate;
        webSocketClient.OnPwmStateUpdated += HandlePwmUpdate;
    }

    private void HandleGpioUpdate(GpioStateUpdate update)
    {
        // Сохраняем состояние всех GPIO пинов
        gpioStates[update.pin] = update.state;

        // Автоопределение пинов при первом использовании
        if (forwardPin == -1 && update.mode == "output" && update.state)
        {
            // Предполагаем, что первый активный пин - это направление вперёд
            forwardPin = update.pin;
            Debug.Log($"Автоопределение: пин {forwardPin} = ДВИЖЕНИЕ ВПЕРЁД");
        }
        else if (backwardPin == -1 && update.mode == "output" && update.state && update.pin != forwardPin)
        {
            // Второй активный пин - направление назад
            backwardPin = update.pin;
            Debug.Log($"Автоопределение: пин {backwardPin} = ДВИЖЕНИЕ НАЗАД");
        }
    }

    private void HandlePwmUpdate(PwmStateUpdate update)
    {
        // Автоопределение PWM пина
        if (pwmPin == -1)
        {
            pwmPin = update.pin;
            Debug.Log($"Автоопределение: пин {pwmPin} = PWM (скорость)");
        }

        // Сохраняем целевую скважность
        if (update.pin == pwmPin)
        {
            targetDutyCycle = update.duty_cycle;
        }
    }

    private void Update()
    {
        if (isEmergencyStop) return;

        // Автоопределение направления движения
        UpdateDirection();

        // Плавное изменение скорости
        float targetSpeed = (targetDutyCycle / 100f) * maxSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, accelerationRate);

        // Плавное торможение
        if (Mathf.Abs(targetSpeed) < 0.1f && currentSpeed > 0.1f)
        {
            currentSpeed = Mathf.Max(0, currentSpeed - decelerationRate * Time.deltaTime);
        }

        // Применение движения
        if (isMovingForward && currentSpeed > 0.1f)
        {
            transform.Translate(0, 0, currentSpeed * Time.deltaTime);
        }
        // Движение назад можно добавить аналогично
    }

    private void UpdateDirection()
    {
        // Проверяем, определены ли пины
        if (forwardPin == -1 || backwardPin == -1) return;

        // Получаем текущие состояния
        bool forwardState = gpioStates.GetValueOrDefault(forwardPin, false);
        bool backwardState = gpioStates.GetValueOrDefault(backwardPin, false);

        // Определяем направление
        isMovingForward = forwardState && !backwardState;
        bool isMovingBackward = !forwardState && backwardState;

        // Если оба пина активны или оба неактивны - остановка
        if ((forwardState && backwardState) || (!forwardState && !backwardState))
        {
            isMovingForward = false;
        }
    }

    private void OnDestroy()
    {
        if (webSocketClient != null)
        {
            webSocketClient.OnGpioStateUpdated -= HandleGpioUpdate;
            webSocketClient.OnPwmStateUpdated -= HandlePwmUpdate;
        }
    }

    // Отладочный интерфейс
    private void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label($"Автоопределение пинов:");
        GUILayout.Label($"  Вперёд: {(forwardPin >= 0 ? forwardPin.ToString() : "не определён")}");
        GUILayout.Label($"  Назад: {(backwardPin >= 0 ? backwardPin.ToString() : "не определён")}");
        GUILayout.Label($"  PWM: {(pwmPin >= 0 ? pwmPin.ToString() : "не определён")}");
        GUILayout.Label($"Текущая скорость: {currentSpeed * 10:F1} ед/с");
        GUILayout.Label($"Состояние: {(isMovingForward ? "ВПЕРЁД" : "СТОП")}");
        GUILayout.EndArea();
    }
}
