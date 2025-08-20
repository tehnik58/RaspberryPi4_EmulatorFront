using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ������� �������� ����������, ���������� �� ������������� � ����������� ���� ������
/// ��������� ������ Singleton ��� ����������� �������
/// </summary>
public class AppManager : MonoBehaviour
{
    // ����������� ������ �� ��������� ��� ���������� ������� Singleton
    public static AppManager Instance { get; private set; }

    [Header("Dependencies")]
    public WebSocketClient webSocketClient;      // ������ ��� WebSocket ����������
    public SceneLoader sceneLoader;              // ��������� ����
    public GameStateManager gameStateManager;    // �������� ��������� ����������

    /// <summary>
    /// ���������� ��� �������� �������, ������������ �������������� ����������
    /// </summary>
    private void Awake()
    {
        // ���������� ������� Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // ��������� ������ ����� �������
        }
        else
        {
            Destroy(gameObject);  // ���������� ���������
            return;
        }

        Initialize();  // ������������� ����������
    }

    /// <summary>
    /// ������������� ���� ������ ����������
    /// </summary>
    private void Initialize()
    {
        Debug.Log("Initializing AppManager...");

        // ������������� ���������
        gameStateManager = new GameStateManager();
        sceneLoader = GetComponent<SceneLoader>();
        webSocketClient = GetComponent<WebSocketClient>();

        // ��������� ���������� ��������� ����������
        gameStateManager.SetState(GameState.Initializing);

        // �������� �� ������� ������� �������
        EventSystem.OnApplicationError += HandleApplicationError;

        Debug.Log("AppManager initialized successfully");
        gameStateManager.SetState(GameState.MainMenu);  // ������� � ��������� �������� ����

        webSocketClient = GetComponent<WebSocketClient>();
        if (webSocketClient == null)
        {
            webSocketClient = gameObject.AddComponent<WebSocketClient>();
        }

        MessageHandler messageHandler = GetComponent<MessageHandler>();
        if (messageHandler == null)
        {
            messageHandler = gameObject.AddComponent<MessageHandler>();
        }

        sceneLoader = GetComponent<SceneLoader>();
        if (sceneLoader == null)
        {
            sceneLoader = gameObject.AddComponent<SceneLoader>();
        }
    }

    /// <summary>
    /// ���������� ������ ����������
    /// </summary>
    /// <param name="errorMessage">��������� �� ������</param>
    /// <param name="severity">������� ����������� ������</param>
    private void HandleApplicationError(string errorMessage, ErrorSeverity severity)
    {
        Debug.LogError($"Application error: {errorMessage} (Severity: {severity})");

        // TODO: �������� ������ ������������ � ����������� �� ������ �����������
    }

    /// <summary>
    /// ���������� ��� ����������� �������, ������������ �� �������
    /// </summary>
    private void OnDestroy()
    {
        // ������������ �� �������, ���� ��� �������� ���������
        if (Instance == this)
        {
            EventSystem.OnApplicationError -= HandleApplicationError;
        }
    }
}