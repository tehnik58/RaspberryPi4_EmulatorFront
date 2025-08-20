using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Главный менеджер приложения, отвечающий за инициализацию и координацию всех систем
/// Реализует шаблон Singleton для глобального доступа
/// </summary>
public class AppManager : MonoBehaviour
{
    // Статическая ссылка на экземпляр для реализации шаблона Singleton
    public static AppManager Instance { get; private set; }

    [Header("Dependencies")]
    public WebSocketClient webSocketClient;      // Клиент для WebSocket соединения
    public SceneLoader sceneLoader;              // Загрузчик сцен
    public GameStateManager gameStateManager;    // Менеджер состояния приложения

    /// <summary>
    /// Вызывается при создании объекта, обеспечивает единственность экземпляра
    /// </summary>
    private void Awake()
    {
        // Реализация шаблона Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Сохраняем объект между сценами
        }
        else
        {
            Destroy(gameObject);  // Уничтожаем дубликаты
            return;
        }

        Initialize();  // Инициализация приложения
    }

    /// <summary>
    /// Инициализация всех систем приложения
    /// </summary>
    private void Initialize()
    {
        Debug.Log("Initializing AppManager...");

        // Инициализация подсистем
        gameStateManager = new GameStateManager();
        sceneLoader = GetComponent<SceneLoader>();
        webSocketClient = GetComponent<WebSocketClient>();

        // Установка начального состояния приложения
        gameStateManager.SetState(GameState.Initializing);

        // Подписка на события системы событий
        EventSystem.OnApplicationError += HandleApplicationError;

        Debug.Log("AppManager initialized successfully");
        gameStateManager.SetState(GameState.MainMenu);  // Переход в состояние главного меню

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
    /// Обработчик ошибок приложения
    /// </summary>
    /// <param name="errorMessage">Сообщение об ошибке</param>
    /// <param name="severity">Уровень серьезности ошибки</param>
    private void HandleApplicationError(string errorMessage, ErrorSeverity severity)
    {
        Debug.LogError($"Application error: {errorMessage} (Severity: {severity})");

        // TODO: Показать ошибку пользователю в зависимости от уровня серьезности
    }

    /// <summary>
    /// Вызывается при уничтожении объекта, отписываемся от событий
    /// </summary>
    private void OnDestroy()
    {
        // Отписываемся от событий, если это основной экземпляр
        if (Instance == this)
        {
            EventSystem.OnApplicationError -= HandleApplicationError;
        }
    }
}