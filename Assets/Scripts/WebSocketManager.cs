using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;

public class WebSocketManager : MonoBehaviour
{
    private ClientWebSocket webSocket;
    private Uri serverUri = new Uri("ws://localhost:8000/ws/execute");
    
    private readonly Queue<string> messageQueue = new Queue<string>();
    private readonly object queueLock = new object();
    
    // Инициализируем события пустыми делегатами
    public event Action<string> OnMessageReceived = delegate { };
    public event Action OnConnected = delegate { };
    public event Action OnDisconnected = delegate { };
    public event Action<string> OnError = delegate { };

    public bool IsConnected { get; private set; }
    public WebSocketState ConnectionState => webSocket?.State ?? WebSocketState.Closed;

    private CancellationTokenSource cancellationTokenSource;
    private bool isConnecting = false;

    private void Start()
    {
        cancellationTokenSource = new CancellationTokenSource();
        _ = ConnectAsync(); // Используем discard для async void
    }

    public async Task ConnectAsync()
    {
        if (isConnecting || webSocket?.State == WebSocketState.Open)
        {
            Debug.LogWarning("⚠️ Already connecting or connected");
            return;
        }

        isConnecting = true;
        Debug.Log("🔄 Starting WebSocket connection...");

        try
        {
            webSocket = new ClientWebSocket();
            Debug.Log("✅ WebSocket instance created");

            // Таймаут подключения - 10 секунд
            var timeoutTokenSource = new CancellationTokenSource(10000);
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationTokenSource.Token, timeoutTokenSource.Token);

            Debug.Log($"🌐 Connecting to: {serverUri}");
            await webSocket.ConnectAsync(serverUri, linkedTokenSource.Token);
            
            IsConnected = true;
            Debug.Log("✅ WebSocket connected successfully");
            OnConnected(); // Без ?.Invoke - уже инициализировано

            // Запускаем прием сообщений
            _ = ReceiveMessagesAsync();

        }
        catch (OperationCanceledException)
        {
            string errorMsg = "⏰ Connection timeout";
            Debug.LogError(errorMsg);
            OnError(errorMsg);
        }
        catch (System.Net.Sockets.SocketException sockEx)
        {
            string errorMsg = $"🌐 Socket error: {sockEx.SocketErrorCode}";
            Debug.LogError(errorMsg);
            OnError(errorMsg);
        }
        catch (WebSocketException wsEx)
        {
            string errorMsg = $"🔌 WebSocket error: {wsEx.WebSocketErrorCode}";
            Debug.LogError(errorMsg);
            OnError(errorMsg);
        }
        catch (Exception ex)
        {
            string errorMsg = $"❌ Connection error: {ex.GetType().Name}: {ex.Message}";
            Debug.LogError(errorMsg);
            OnError(errorMsg);
        }
        finally
        {
            isConnecting = false;
        }
    }

    private async Task ReceiveMessagesAsync()
    {
        Debug.Log("👂 Starting to listen for messages...");
        var buffer = new byte[4096];
        
        try
        {
            while (webSocket != null && webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), 
                    cancellationTokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Debug.Log($"📨 Received message: {message}");
                    
                    lock (queueLock)
                    {
                        messageQueue.Enqueue(message);
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Debug.Log("🔌 Connection closed by server");
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("👂 Listening cancelled");
        }
        catch (Exception ex)
        {
            string errorMsg = $"❌ Receive error: {ex.Message}";
            Debug.LogError(errorMsg);
            OnError(errorMsg);
        }
        finally
        {
            Debug.Log("👂 Stopped listening for messages");
            IsConnected = false;
            OnDisconnected();
        }
    }

    void Update()
    {
        ProcessQueuedMessages();
    }

    private void ProcessQueuedMessages()
    {
        lock (queueLock)
        {
            while (messageQueue.Count > 0)
            {
                string message = messageQueue.Dequeue();
                Debug.Log($"Processing message: {message}");
                OnMessageReceived(message);
            }
        }
    }

    public void SendMessage(string message)
    {
        print(message);
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            try
            {
                Debug.Log($"📤 Sending message: {message}");
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                _ = webSocket.SendAsync(
                    new ArraySegment<byte>(bytes), 
                    WebSocketMessageType.Text, 
                    true, 
                    cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                string errorMsg = $"❌ Send error: {ex.Message}";
                Debug.LogError(errorMsg);
                OnError(errorMsg);
            }
        }
        else
        {
            string warningMsg = "⚠️ Cannot send - WebSocket not connected";
            Debug.LogWarning(warningMsg);
            OnError(warningMsg);
        }
    }

    public void SendJson(object data)
    {
        string json = JsonUtility.ToJson(data);
        SendMessage(json);
    }

    public async void Disconnect()
    {
        if (webSocket != null)
        {
            try
            {
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure, 
                    "Client disconnect", 
                    cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Close error: {ex.Message}");
            }
            finally
            {
                webSocket.Dispose();
                webSocket = null;
            }
        }
        IsConnected = false;
        cancellationTokenSource?.Cancel();
    }

    void OnDestroy()
    {
        Disconnect();
        cancellationTokenSource?.Dispose();
    }

    // Метод для проверки доступности сервера
    public async Task<bool> CheckServerAvailability()
    {
        try
        {
            using (var testSocket = new ClientWebSocket())
            {
                var timeoutToken = new CancellationTokenSource(3000).Token;
                await testSocket.ConnectAsync(serverUri, timeoutToken);
                await testSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test", timeoutToken);
                return true;
            }
        }
        catch
        {
            return false;
        }
    }
}