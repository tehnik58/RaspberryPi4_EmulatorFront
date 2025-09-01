using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicCarControllerTest : MonoBehaviour
{
    [Header("��������� ��������")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float accelerationRate = 0.15f;
    [SerializeField] private float decelerationRate = 0.4f;

    [Header("������� ���������")]
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
            Debug.LogError("WebSocketClient �� ������ �� �����!");
            enabled = false;
            return;
        }

        // ������������� �� ������� �� WebSocketClient
        webSocketClient.OnGpioStateUpdated += HandleGpioUpdate;
        webSocketClient.OnPwmStateUpdated += HandlePwmUpdate;
    }

    private void HandleGpioUpdate(GpioStateUpdate update)
    {
        // ��������� ��������� ���� GPIO �����
        gpioStates[update.pin] = update.state;

        // ��������������� ����� ��� ������ �������������
        if (forwardPin == -1 && update.mode == "output" && update.state)
        {
            // ������������, ��� ������ �������� ��� - ��� ����������� �����
            forwardPin = update.pin;
            Debug.Log($"���������������: ��� {forwardPin} = �������� ���Ш�");
        }
        else if (backwardPin == -1 && update.mode == "output" && update.state && update.pin != forwardPin)
        {
            // ������ �������� ��� - ����������� �����
            backwardPin = update.pin;
            Debug.Log($"���������������: ��� {backwardPin} = �������� �����");
        }
    }

    private void HandlePwmUpdate(PwmStateUpdate update)
    {
        // ��������������� PWM ����
        if (pwmPin == -1)
        {
            pwmPin = update.pin;
            Debug.Log($"���������������: ��� {pwmPin} = PWM (��������)");
        }

        // ��������� ������� ����������
        if (update.pin == pwmPin)
        {
            targetDutyCycle = update.duty_cycle;
        }
    }

    private void Update()
    {
        if (isEmergencyStop) return;

        // ��������������� ����������� ��������
        UpdateDirection();

        // ������� ��������� ��������
        float targetSpeed = (targetDutyCycle / 100f) * maxSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, accelerationRate);

        // ������� ����������
        if (Mathf.Abs(targetSpeed) < 0.1f && currentSpeed > 0.1f)
        {
            currentSpeed = Mathf.Max(0, currentSpeed - decelerationRate * Time.deltaTime);
        }

        // ���������� ��������
        if (isMovingForward && currentSpeed > 0.1f)
        {
            transform.Translate(0, 0, currentSpeed * Time.deltaTime);
        }
        // �������� ����� ����� �������� ����������
    }

    private void UpdateDirection()
    {
        // ���������, ���������� �� ����
        if (forwardPin == -1 || backwardPin == -1) return;

        // �������� ������� ���������
        bool forwardState = gpioStates.GetValueOrDefault(forwardPin, false);
        bool backwardState = gpioStates.GetValueOrDefault(backwardPin, false);

        // ���������� �����������
        isMovingForward = forwardState && !backwardState;
        bool isMovingBackward = !forwardState && backwardState;

        // ���� ��� ���� ������� ��� ��� ��������� - ���������
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

    // ���������� ���������
    private void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label($"��������������� �����:");
        GUILayout.Label($"  �����: {(forwardPin >= 0 ? forwardPin.ToString() : "�� ��������")}");
        GUILayout.Label($"  �����: {(backwardPin >= 0 ? backwardPin.ToString() : "�� ��������")}");
        GUILayout.Label($"  PWM: {(pwmPin >= 0 ? pwmPin.ToString() : "�� ��������")}");
        GUILayout.Label($"������� ��������: {currentSpeed * 10:F1} ��/�");
        GUILayout.Label($"���������: {(isMovingForward ? "���Ш�" : "����")}");
        GUILayout.EndArea();
    }
}
