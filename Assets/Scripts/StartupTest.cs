using UnityEngine;

/// <summary>
/// �������� ������ ��� �������� ������ ������� ����������������
/// </summary>
public class StartupTest : MonoBehaviour
{
    /// <summary>
    /// ������ ����� ��� ������
    /// </summary>
    private void Start()
    {
        Debug.Log("Startup test running...");

        // ������������ ������� �������
        EventSystem.OnStatusMessage += TestStatusMessage;
        EventSystem.TriggerStatusMessage("Startup test completed");

        // ������������ WebSocket ����������
        WebSocketClient wsClient = FindObjectOfType<WebSocketClient>();
        if (wsClient != null)
        {
            Debug.Log($"WebSocket client found: {wsClient.ClientId}");
        }
    }

    /// <summary>
    /// ���������� ��������� ���������� ���������
    /// </summary>
    private void TestStatusMessage(string message)
    {
        Debug.Log($"Status message received: {message}");
        EventSystem.OnStatusMessage -= TestStatusMessage;  // ������������ ����� ���������
    }
}