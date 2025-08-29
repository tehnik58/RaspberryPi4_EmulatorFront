using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class CarController : MonoBehaviour
{
    [Header("��������� ��������")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float accelerationRate = 0.15f;
    [SerializeField] private float decelerationRate = 0.4f;

    [Header("���������� ����������")]
    [SerializeField] private float currentSpeed;
    [SerializeField] private bool isMovingForward;
    [SerializeField] private bool isEmergencyStop;

    private WebSocketClient webSocketClient;
    private readonly Regex speedRegex = new Regex(@"MOTOR_(?:SPEED|PULSE|STEP):\s*(\d+)%");
    private readonly Regex stateRegex = new Regex(@"MOTOR_STATE:\s*(\w+)\s*at\s*(\d+)%");
    private readonly Regex emergencyRegex = new Regex(@"MOTOR_EMERGENCY:\s*(\w+)");

    private void Start()
    {
        webSocketClient = FindObjectOfType<WebSocketClient>();

        if (webSocketClient == null)
        {
            Debug.LogError("WebSocketClient �� ������ �� �����!");
            enabled = false;
            return;
        }

        // ������������� ������ �� ��������� �����
        webSocketClient.OnOutputReceived += ProcessMotorCommands;
    }

    private void ProcessMotorCommands(string line)
    {
        // 1. ��������� ������ ��������
        Match speedMatch = speedRegex.Match(line);
        if (speedMatch.Success)
        {
            float targetSpeed = float.Parse(speedMatch.Groups[1].Value) / 100f * maxSpeed;
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, accelerationRate);
            isEmergencyStop = false;
            return;
        }

        // 2. ��������� ��������� ��������
        Match stateMatch = stateRegex.Match(line);
        if (stateMatch.Success)
        {
            string direction = stateMatch.Groups[1].Value.ToUpper();
            float speedValue = float.Parse(stateMatch.Groups[2].Value) / 100f * maxSpeed;

            isMovingForward = direction == "FORWARD";
            currentSpeed = speedValue;
            isEmergencyStop = false;
            return;
        }

        // 3. ��������� ��������� ���������
        Match emergencyMatch = emergencyRegex.Match(line);
        if (emergencyMatch.Success)
        {
            string status = emergencyMatch.Groups[1].Value.ToUpper();
            if (status == "STOPPED")
            {
                isEmergencyStop = true;
                currentSpeed = 0;
            }
            return;
        }
    }

    private void Update()
    {
        if (isEmergencyStop) return;

        // ������� ����������
        if (currentSpeed > 0.1f)
        {
            currentSpeed = Mathf.Max(0, currentSpeed - decelerationRate * Time.deltaTime);
        }

        // �������� �����
        if (isMovingForward)
        {
            transform.Translate(0, 0, currentSpeed * Time.deltaTime);
        }
    }

    private void OnDestroy()
    {
        if (webSocketClient != null)
        {
            webSocketClient.OnOutputReceived -= ProcessMotorCommands;
        }
    }

    // ���������� ���������
    private void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        GUILayout.Label($"������� ��������: {currentSpeed * 10:F1} ��/�");
        GUILayout.Label($"���������: {(isEmergencyStop ? "������" : (isMovingForward ? "���Ш�" : "����"))}");
        GUILayout.EndArea();
    }
}
