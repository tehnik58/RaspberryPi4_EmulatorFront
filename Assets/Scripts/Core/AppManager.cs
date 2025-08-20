using UnityEngine;

/// <summary>
/// ������� �������� ����������, ������������ ��� �������
/// ����������� ���� ���������� ���� �����������
/// </summary>
public class AppManager : MonoBehaviour
{
    public static AppManager Instance { get; private set; }

    [Header("Dependencies")]
    public WebSocketClient webSocketClient;
    public SceneLoader sceneLoader;
    public GameStateManager gameStateManager;
    public UIManager uiManager;

    /// <summary>
    /// ������������� Singleton ��� ��������
    /// </summary>
    private void Awake()
    {
        // ���������� �������� Singleton
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

        Initialize();
    }

    /// <summary>
    /// ������������� ���� ������ ����������
    /// </summary>
    private void Initialize()
    {
        Debug.Log("Initializing AppManager...");

        // ����� � ������������� ���� ������������
        InitializeDependencies();

        // �������� �� ���������� �������
        SubscribeToEvents();

        // ��������� ���������� ���������
        gameStateManager.SetState(GameState.MainMenu);

        Debug.Log("AppManager initialized successfully");
    }

    /// <summary>
    /// ������������� ���� ��������� �����������
    /// </summary>
    private void InitializeDependencies()
    {
        // ������������� ��������� ���������
        gameStateManager = GameStateManager.Instance;
        if (gameStateManager == null)
        {
            Debug.LogError("GameStateManager not found!");
        }

        // ������������� WebSocket �������
        webSocketClient = WebSocketClient.Instance;
        if (webSocketClient == null)
        {
            Debug.LogError("WebSocketClient not found!");
        }

        // ������������� ���������� ����
        sceneLoader = SceneLoader.Instance;
        if (sceneLoader == null)
        {
            Debug.LogError("SceneLoader not found!");
        }

        // ������������� UI ���������
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogWarning("UIManager not found, UI functionality will be limited");
        }
    }

    /// <summary>
    /// �������� �� ���������� ������� �������
    /// </summary>
    private void SubscribeToEvents()
    {
        EventSystem.OnApplicationError += HandleApplicationError;
        EventSystem.OnStatusMessage += HandleStatusMessage;
    }

    /// <summary>
    /// ������� �� ������� ��� �����������
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        EventSystem.OnApplicationError -= HandleApplicationError;
        EventSystem.OnStatusMessage -= HandleStatusMessage;
    }

    /// <summary>
    /// ���������� ������ ����������
    /// </summary>
    private void HandleApplicationError(string errorMessage, ErrorSeverity severity)
    {
        Debug.LogError($"Application error: {errorMessage} (Severity: {severity})");

        // �������� ������ � UI ������� ���� ��� ��������
        if (uiManager != null)
        {
            uiManager.ShowError(errorMessage, severity);
        }

        // ��� ����������� ������ ��������� � ��������� ������
        if (severity >= ErrorSeverity.High)
        {
            gameStateManager.SetState(GameState.Error);
        }
    }

    /// <summary>
    /// ���������� ��������� ���������
    /// </summary>
    private void HandleStatusMessage(string message)
    {
        Debug.Log($"Status: {message}");
    }

    /// <summary>
    /// ���������� ��� ����������� �������, ��������� �������
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this)
        {
            UnsubscribeFromEvents();
        }
    }
}