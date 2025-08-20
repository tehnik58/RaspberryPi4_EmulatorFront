using UnityEngine;

/// <summary>
/// Главный менеджер приложения, координирует все системы
/// Центральный узел управления всем приложением
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
    /// Инициализация Singleton при создании
    /// </summary>
    private void Awake()
    {
        // Реализация паттерна Singleton
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
    /// Инициализация всех систем приложения
    /// </summary>
    private void Initialize()
    {
        Debug.Log("Initializing AppManager...");

        // Поиск и инициализация всех зависимостей
        InitializeDependencies();

        // Подписка на глобальные события
        SubscribeToEvents();

        // Установка начального состояния
        gameStateManager.SetState(GameState.MainMenu);

        Debug.Log("AppManager initialized successfully");
    }

    /// <summary>
    /// Инициализация всех зависимых компонентов
    /// </summary>
    private void InitializeDependencies()
    {
        // Инициализация менеджера состояния
        gameStateManager = GameStateManager.Instance;
        if (gameStateManager == null)
        {
            Debug.LogError("GameStateManager not found!");
        }

        // Инициализация WebSocket клиента
        webSocketClient = WebSocketClient.Instance;
        if (webSocketClient == null)
        {
            Debug.LogError("WebSocketClient not found!");
        }

        // Инициализация загрузчика сцен
        sceneLoader = SceneLoader.Instance;
        if (sceneLoader == null)
        {
            Debug.LogError("SceneLoader not found!");
        }

        // Инициализация UI менеджера
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogWarning("UIManager not found, UI functionality will be limited");
        }
    }

    /// <summary>
    /// Подписка на глобальные события системы
    /// </summary>
    private void SubscribeToEvents()
    {
        EventSystem.OnApplicationError += HandleApplicationError;
        EventSystem.OnStatusMessage += HandleStatusMessage;
    }

    /// <summary>
    /// Отписка от событий при уничтожении
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        EventSystem.OnApplicationError -= HandleApplicationError;
        EventSystem.OnStatusMessage -= HandleStatusMessage;
    }

    /// <summary>
    /// Обработчик ошибок приложения
    /// </summary>
    private void HandleApplicationError(string errorMessage, ErrorSeverity severity)
    {
        Debug.LogError($"Application error: {errorMessage} (Severity: {severity})");

        // Передача ошибки в UI систему если она доступна
        if (uiManager != null)
        {
            uiManager.ShowError(errorMessage, severity);
        }

        // Для критических ошибок переходим в состояние ошибки
        if (severity >= ErrorSeverity.High)
        {
            gameStateManager.SetState(GameState.Error);
        }
    }

    /// <summary>
    /// Обработчик статусных сообщений
    /// </summary>
    private void HandleStatusMessage(string message)
    {
        Debug.Log($"Status: {message}");
    }

    /// <summary>
    /// Вызывается при уничтожении объекта, выполняет очистку
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this)
        {
            UnsubscribeFromEvents();
        }
    }
}