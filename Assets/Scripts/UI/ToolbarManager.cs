using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Управление панелью инструментов и кнопками действий
/// </summary>
public class ToolbarManager : MonoBehaviour
{
    [Header("Toolbar Buttons")]
    public ButtonHandler runButton;
    public ButtonHandler stopButton;
    public ButtonHandler clearButton;
    public ButtonHandler saveButton;
    public ButtonHandler loadButton;
    public ButtonHandler settingsButton;

    [Header("Status Indicators")]
    public Image connectionStatus;
    public TMP_Text statusText;
    public GameObject executingIndicator;

    [Header("Colors")]
    public Color connectedColor = Color.green;
    public Color disconnectedColor = Color.red;
    public Color connectingColor = Color.yellow;

    private WebSocketClient webSocketClient;
    private GameStateManager gameStateManager;
    private CodeEditor codeEditor;
    private ConsoleOutput consoleOutput;

    /// <summary>
    /// Инициализация при старте
    /// </summary>
    private void Start()
    {
        InitializeReferences();
        InitializeButtons();
        UpdateButtonStates();
        UpdateConnectionStatus(false);
    }

    /// <summary>
    /// Инициализация ссылок на компоненты
    /// </summary>
    private void InitializeReferences()
    {
        webSocketClient = WebSocketClient.Instance;
        gameStateManager = GameStateManager.Instance;
        codeEditor = FindObjectOfType<CodeEditor>();
        consoleOutput = FindObjectOfType<ConsoleOutput>();
    }

    /// <summary>
    /// Инициализация обработчиков кнопок
    /// </summary>
    private void InitializeButtons()
    {
        if (runButton != null)
            runButton.OnClick.AddListener(OnRunButtonClick);

        if (stopButton != null)
            stopButton.OnClick.AddListener(OnStopButtonClick);

        if (clearButton != null)
            clearButton.OnClick.AddListener(OnClearButtonClick);

        if (saveButton != null)
            saveButton.OnClick.AddListener(OnSaveButtonClick);

        if (loadButton != null)
            loadButton.OnClick.AddListener(OnLoadButtonClick);

        if (settingsButton != null)
            settingsButton.OnClick.AddListener(OnSettingsButtonClick);
    }

    /// <summary>
    /// Обработчик кнопки запуска
    /// </summary>
    private void OnRunButtonClick()
    {
        EventSystem.TriggerStatusMessage("Executing code...");
        if (codeEditor != null)
        {
            codeEditor.RunCode();
        }
    }

    /// <summary>
    /// Обработчик кнопки остановки
    /// </summary>
    private void OnStopButtonClick()
    {
        EventSystem.TriggerStatusMessage("Stopping execution...");
        // TODO: Реализовать остановку выполнения
    }

    /// <summary>
    /// Обработчик кнопки очистки
    /// </summary>
    private void OnClearButtonClick()
    {
        EventSystem.TriggerStatusMessage("Clearing console...");
        if (consoleOutput != null)
        {
            consoleOutput.Clear();
        }
    }

    /// <summary>
    /// Обработчик кнопки сохранения
    /// </summary>
    private void OnSaveButtonClick()
    {
        EventSystem.TriggerStatusMessage("Saving project...");
        if (codeEditor != null)
        {
            codeEditor.SaveCode();
        }
    }

    /// <summary>
    /// Обработчик кнопки загрузки
    /// </summary>
    private void OnLoadButtonClick()
    {
        EventSystem.TriggerStatusMessage("Loading project...");
        if (codeEditor != null)
        {
            codeEditor.LoadCode();
        }
    }

    /// <summary>
    /// Обработчик кнопки настроек
    /// </summary>
    private void OnSettingsButtonClick()
    {
        EventSystem.TriggerStatusMessage("Opening settings...");
        // TODO: Реализовать открытие настроек
    }

    /// <summary>
    /// Обновление состояний кнопок
    /// </summary>
    public void UpdateButtonStates()
    {
        bool isConnected = webSocketClient != null && webSocketClient.IsConnected;
        bool isExecuting = gameStateManager != null && gameStateManager.IsState(GameState.ExecutingCode);

        if (runButton != null)
            runButton.SetInteractable(isConnected && !isExecuting);

        if (stopButton != null)
            stopButton.SetInteractable(isConnected && isExecuting);

        if (clearButton != null)
            clearButton.SetInteractable(true);

        if (saveButton != null)
            saveButton.SetInteractable(true);

        if (loadButton != null)
            loadButton.SetInteractable(true);

        if (settingsButton != null)
            settingsButton.SetInteractable(true);

        if (executingIndicator != null)
            executingIndicator.SetActive(isExecuting);
    }

    /// <summary>
    /// Обновление статуса соединения
    /// </summary>
    public void UpdateConnectionStatus(bool connected)
    {
        if (connectionStatus != null)
        {
            connectionStatus.color = connected ? connectedColor : disconnectedColor;
        }

        if (statusText != null)
        {
            statusText.text = connected ? "Connected" : "Disconnected";
            statusText.color = connected ? connectedColor : disconnectedColor;
        }

        UpdateButtonStates();
    }

    /// <summary>
    /// Установка interactable состояния
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (runButton != null)
            runButton.SetInteractable(interactable);

        if (stopButton != null)
            stopButton.SetInteractable(interactable);

        if (clearButton != null)
            clearButton.SetInteractable(interactable);

        if (saveButton != null)
            saveButton.SetInteractable(interactable);

        if (loadButton != null)
            loadButton.SetInteractable(interactable);

        if (settingsButton != null)
            settingsButton.SetInteractable(interactable);
    }

    /// <summary>
    /// Подписка на события при включении
    /// </summary>
    private void OnEnable()
    {
        EventSystem.OnWebSocketConnected += HandleWebSocketConnected;
        EventSystem.OnWebSocketDisconnected += HandleWebSocketDisconnected;
        EventSystem.OnGameStateChanged += HandleGameStateChanged;
    }

    /// <summary>
    /// Отписка от событий при выключении
    /// </summary>
    private void OnDisable()
    {
        EventSystem.OnWebSocketConnected -= HandleWebSocketConnected;
        EventSystem.OnWebSocketDisconnected -= HandleWebSocketDisconnected;
        EventSystem.OnGameStateChanged -= HandleGameStateChanged;
    }

    /// <summary>
    /// Обработчик подключения WebSocket
    /// </summary>
    private void HandleWebSocketConnected()
    {
        UpdateConnectionStatus(true);
    }

    /// <summary>
    /// Обработчик отключения WebSocket
    /// </summary>
    private void HandleWebSocketDisconnected(string reason)
    {
        UpdateConnectionStatus(false);
    }

    /// <summary>
    /// Обработчик изменения состояния игры
    /// </summary>
    private void HandleGameStateChanged(GameState oldState, GameState newState)
    {
        UpdateButtonStates();
    }

    /// <summary>
    /// Очистка при уничтожении
    /// </summary>
    private void OnDestroy()
    {
        // Отписываемся от всех кнопок
        if (runButton != null)
            runButton.OnClick.RemoveListener(OnRunButtonClick);

        if (stopButton != null)
            stopButton.OnClick.RemoveListener(OnStopButtonClick);

        if (clearButton != null)
            clearButton.OnClick.RemoveListener(OnClearButtonClick);

        if (saveButton != null)
            saveButton.OnClick.RemoveListener(OnSaveButtonClick);

        if (loadButton != null)
            loadButton.OnClick.RemoveListener(OnLoadButtonClick);

        if (settingsButton != null)
            settingsButton.OnClick.RemoveListener(OnSettingsButtonClick);
    }
}