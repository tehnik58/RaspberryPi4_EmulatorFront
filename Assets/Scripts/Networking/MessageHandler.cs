using System;
using UnityEngine;

/// <summary>
/// ������� ����� ��� ��������� WebSocket
/// </summary>
[System.Serializable]
public class WebSocketMessage
{
    public string type;      // ��� ���������
    public string message;   // ����� ���������
}

/// <summary>
/// ���������� �������� ��������� �� �������
/// ������ JSON � ������������ ��������� �� ��������������� ������������
/// </summary>
public class MessageHandler : MonoBehaviour
{
    /// <summary>
    /// �������� �� ������� ��� ��������� ����������
    /// </summary>
    private void OnEnable()
    {
        // ����������� ������������ ������� WebSocket
        EventSystem.OnWebSocketConnected += HandleWebSocketConnected;
        EventSystem.OnWebSocketDisconnected += HandleWebSocketDisconnected;
        EventSystem.OnWebSocketError += HandleWebSocketError;
    }

    /// <summary>
    /// ������� �� ������� ��� ���������� ����������
    /// </summary>
    private void OnDisable()
    {
        // ������ ����������� ������������ �������
        EventSystem.OnWebSocketConnected -= HandleWebSocketConnected;
        EventSystem.OnWebSocketDisconnected -= HandleWebSocketDisconnected;
        EventSystem.OnWebSocketError -= HandleWebSocketError;
    }

    /// <summary>
    /// �������� ����� ��������� �������� ���������
    /// </summary>
    /// <param name="jsonMessage">��������� � ������� JSON</param>
    public void ProcessMessage(string jsonMessage)
    {
        try
        {
            // ������ ������� ��������� ��������� ��� ����������� ����
            WebSocketMessage baseMessage = JsonUtility.FromJson<WebSocketMessage>(jsonMessage);

            // ������������ ��������� �� ����� ���������
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
    /// ���������� ������� ����������� WebSocket
    /// </summary>
    private void HandleWebSocketConnected()
    {
        Debug.Log("WebSocket connected event received");
        EventSystem.TriggerStatusMessage("Connected to server");
    }

    /// <summary>
    /// ���������� ������� ���������� WebSocket
    /// </summary>
    private void HandleWebSocketDisconnected(string reason)
    {
        Debug.Log($"WebSocket disconnected: {reason}");
        EventSystem.TriggerStatusMessage($"Disconnected: {reason}");
    }

    /// <summary>
    /// ���������� ������� ������ WebSocket
    /// </summary>
    private void HandleWebSocketError(string error)
    {
        Debug.LogError($"WebSocket error: {error}");
        EventSystem.TriggerApplicationError($"WebSocket error: {error}", ErrorSeverity.Medium);
    }

    /// <summary>
    /// ���������� ��������� �� ������������ ����������
    /// </summary>
    private void HandleConnectionEstablished(string jsonMessage)
    {
        // ������ ����������� ��������� ���������
        ConnectionEstablishedMessage message = JsonUtility.FromJson<ConnectionEstablishedMessage>(jsonMessage);
        Debug.Log($"Connection established: {message.message}");
        EventSystem.TriggerStatusMessage(message.message);
    }

    /// <summary>
    /// ���������� ���-��������� �� �������
    /// </summary>
    private void HandleLogMessage(string jsonMessage)
    {
        LogMessage message = JsonUtility.FromJson<LogMessage>(jsonMessage);
        Debug.Log($"Server log: {message.message}");
        // TODO: ��������� � UI �������
    }

    /// <summary>
    /// ���������� ������-��������� �� �������
    /// </summary>
    private void HandleStatusMessage(string jsonMessage)
    {
        StatusMessage message = JsonUtility.FromJson<StatusMessage>(jsonMessage);
        Debug.Log($"Server status: {message.message}");
        EventSystem.TriggerStatusMessage(message.message);
    }

    /// <summary>
    /// ���������� ��������� �� ������� �� �������
    /// </summary>
    private void HandleErrorMessage(string jsonMessage)
    {
        ErrorMessage message = JsonUtility.FromJson<ErrorMessage>(jsonMessage);
        Debug.LogError($"Server error: {message.message}");
        EventSystem.TriggerApplicationError($"Server error: {message.message}", ErrorSeverity.Medium);
    }
}

// ������ ��� �������� ����������� ����� ���������

/// <summary>
/// ��������� �� ������������ ����������
/// </summary>
[System.Serializable]
public class ConnectionEstablishedMessage : WebSocketMessage
{
    public string client_id;  // ������������� �������
}

/// <summary>
/// ���-��������� �� �������
/// </summary>
[System.Serializable]
public class LogMessage : WebSocketMessage
{
    public string timestamp;  // ��������� ����� ���������
}

/// <summary>
/// ������-��������� �� �������
/// </summary>
[System.Serializable]
public class StatusMessage : WebSocketMessage
{
    public string timestamp;  // ��������� ����� ���������
}

/// <summary>
/// ��������� �� ������ �� �������
/// </summary>
[System.Serializable]
public class ErrorMessage : WebSocketMessage
{
    public string code;       // ��� ������
    public string details;    // ������ ������
    public string timestamp;  // ��������� ����� ���������
}