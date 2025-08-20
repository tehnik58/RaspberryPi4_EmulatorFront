using System;
using System.Collections;
using System.Text;
using UnityEngine;
using NativeWebSocket;

/// <summary>
/// ������ WebSocket ���������� � ��������
/// ��������� ������������, ��������� � ���������� ���������
/// </summary>
public class WebSocketClient : MonoBehaviour
{
    public static WebSocketClient Instance { get; private set; }

    [Header("Connection Settings")]
    public string serverUrl = "ws://localhost:8000/api/ws/";
    public float reconnectDelay = 3f;
    public int maxReconnectAttempts = 5;

    private WebSocket _webSocket;
    private string _clientId;
    private bool _isConnecting;
    private int _reconnectAttempts;

    /// <summary>
    /// ������ ����������� (������ ��� ������)
    /// </summary>
    public bool IsConnected => _webSocket?.State == WebSocketState.Open;

    /// <summary>
    /// ������������� ������� (������ ��� ������)
    /// </summary>
    public string ClientId => _clientId;

    /// <summary>
    /// ������������� Singleton ��� ��������
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// ��������� ��������� � ����������� ��� ������
    /// </summary>
    private void Start()
    {
        GenerateClientId();
        Connect();
    }

    /// <summary>
    /// ��������� ����������� �������������� �������
    /// </summary>
    private void GenerateClientId()
    {
        _clientId = $"unity-client-{System.Guid.NewGuid().ToString().Substring(0, 8)}";
        Debug.Log($"Generated client ID: {_clientId}");
    }

    /// <summary>
    /// ����������� � WebSocket �������
    /// </summary>
    public async void Connect()
    {
        if (_isConnecting) return;

        _isConnecting = true;
        EventSystem.TriggerStatusMessage("Connecting to server...");

        try
        {
            // ��������� ������������ ���������� ���� ����
            if (_webSocket != null)
            {
                await _webSocket.Close();
                _webSocket = null;
            }

            // ��������� URL �����������
            string connectionUrl = $"{serverUrl}{_clientId}";
            Debug.Log($"Connecting to WebSocket: {connectionUrl}");

            // ������� ����� WebSocket ����������
            _webSocket = new WebSocket(connectionUrl);

            // ����������� ����������� �������
            SetupEventHandlers();

            // ������������� ����������
            await _webSocket.Connect();
            _reconnectAttempts = 0;
        }
        catch (Exception ex)
        {
            Debug.LogError($"WebSocket connection failed: {ex.Message}");
            HandleConnectionFailure(ex.Message);
        }
        finally
        {
            _isConnecting = false;
        }
    }

    /// <summary>
    /// ��������� ������������ ������� WebSocket
    /// </summary>
    private void SetupEventHandlers()
    {
        _webSocket.OnOpen += OnWebSocketOpen;
        _webSocket.OnError += OnWebSocketError;
        _webSocket.OnClose += OnWebSocketClose;
        _webSocket.OnMessage += OnWebSocketMessage;
    }

    /// <summary>
    /// ���������� ��������� �����������
    /// </summary>
    private void OnWebSocketOpen()
    {
        Debug.Log("WebSocket connected successfully");
        EventSystem.TriggerWebSocketConnected();
        EventSystem.TriggerStatusMessage("Connected to server");
    }

    /// <summary>
    /// ���������� ������ WebSocket
    /// </summary>
    private void OnWebSocketError(string errorMsg)
    {
        Debug.LogError($"WebSocket error: {errorMsg}");
        EventSystem.TriggerWebSocketError(errorMsg);
        EventSystem.TriggerStatusMessage($"Connection error: {errorMsg}");
    }

    /// <summary>
    /// ���������� �������� ����������
    /// </summary>
    private void OnWebSocketClose(WebSocketCloseCode closeCode)
    {
        string reason = $"WebSocket closed: {closeCode}";
        Debug.Log(reason);
        EventSystem.TriggerWebSocketDisconnected(reason);
        EventSystem.TriggerStatusMessage("Disconnected from server");

        // ������� ��������������� ���� ���������� ������� �� ���������
        if (closeCode != WebSocketCloseCode.Normal)
        {
            AttemptReconnect();
        }
    }

    /// <summary>
    /// ���������� �������� ���������
    /// </summary>
    private void OnWebSocketMessage(byte[] data)
    {
        string message = Encoding.UTF8.GetString(data);
        Debug.Log($"Received message: {message}");

        // �������� ��������� ��� ���������
        MessageHandler messageHandler = FindObjectOfType<MessageHandler>();
        if (messageHandler != null)
        {
            messageHandler.ProcessMessage(message);
        }
    }

    /// <summary>
    /// �������� ��������� �� ������
    /// </summary>
    /// <param name="message">��������� ��� ��������</param>
    public async void SendMessage(string message)
    {
        if (_webSocket?.State == WebSocketState.Open)
        {
            try
            {
                await _webSocket.SendText(message);
                Debug.Log($"Sent message: {message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to send message: {ex.Message}");
                EventSystem.TriggerApplicationError("Failed to send message", ErrorSeverity.Medium);
            }
        }
        else
        {
            Debug.LogWarning("Cannot send message - WebSocket is not connected");
            EventSystem.TriggerApplicationError("Not connected to server", ErrorSeverity.Low);
        }
    }

    /// <summary>
    /// ������� ��������������� � �������
    /// </summary>
    private void AttemptReconnect()
    {
        if (_reconnectAttempts < maxReconnectAttempts)
        {
            _reconnectAttempts++;
            Debug.Log($"Attempting to reconnect ({_reconnectAttempts}/{maxReconnectAttempts})...");
            EventSystem.TriggerStatusMessage($"Reconnecting... ({_reconnectAttempts}/{maxReconnectAttempts})");

            Invoke(nameof(Connect), reconnectDelay);
        }
        else
        {
            Debug.LogError("Max reconnection attempts reached");
            EventSystem.TriggerApplicationError("Failed to connect to server after multiple attempts", ErrorSeverity.High);
        }
    }

    /// <summary>
    /// ���������� ���������� �����������
    /// </summary>
    private void HandleConnectionFailure(string error)
    {
        EventSystem.TriggerApplicationError($"Connection failed: {error}", ErrorSeverity.Medium);
        AttemptReconnect();
    }

    /// <summary>
    /// ��������� ������� ��������� ������ ����
    /// </summary>
    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (_webSocket != null)
        {
            _webSocket.DispatchMessageQueue();
        }
#endif
    }

    /// <summary>
    /// �������� ���������� ��� ������ �� ����������
    /// </summary>
    private async void OnApplicationQuit()
    {
        if (_webSocket != null)
        {
            await _webSocket.Close();
        }
    }
}