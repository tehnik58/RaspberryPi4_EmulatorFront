using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// ������� �������� ����������������� ����������
/// ������������ ������ ���� UI �����������
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI Components")]
    public CodeEditor codeEditor;
    public ConsoleOutput consoleOutput;
    public ToolbarManager toolbarManager;
    public MainMenuUI mainMenuUI;

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject constructorPanel;
    public GameObject loadingPanel;

    [Header("Dependencies")]
    public WebSocketClient webSocketClient;
    public GameStateManager gameStateManager;
    public SceneLoader sceneLoader;

    private Dictionary<GameState, GameObject> statePanels;

    private void Awake()
    {
        // �������� ������������
        if (webSocketClient == null)
            webSocketClient = FindObjectOfType<WebSocketClient>();
        if (gameStateManager == null)
            gameStateManager = FindObjectOfType<GameStateManager>();
        if (sceneLoader == null)
            sceneLoader = FindObjectOfType<SceneLoader>();
    }

    private void Start()
    {
        InitializeStatePanels();
        SubscribeToEvents();
        ShowPanelForCurrentState();
    }

    private void InitializeStatePanels()
    {
        statePanels = new Dictionary<GameState, GameObject>
        {
            { GameState.MainMenu, mainMenuPanel },
            { GameState.Constructor, constructorPanel }
        };
    }

    private void SubscribeToEvents()
    {
        // �������� �� ������� ��������� ��������� ����
        EventSystem.OnGameStateChanged += HandleGameStateChanged;

        // �������� �� ������� WebSocket
        EventSystem.OnWebSocketConnected += HandleWebSocketConnected;
        EventSystem.OnWebSocketDisconnected += HandleWebSocketDisconnected;

        // �������� �� ������� ���������� ����
        EventSystem.OnStatusMessage += HandleStatusMessage;
    }

    private void UnsubscribeFromEvents()
    {
        EventSystem.OnGameStateChanged -= HandleGameStateChanged;
        EventSystem.OnWebSocketConnected -= HandleWebSocketConnected;
        EventSystem.OnWebSocketDisconnected -= HandleWebSocketDisconnected;
        EventSystem.OnStatusMessage -= HandleStatusMessage;
    }

    private void HandleGameStateChanged(GameState oldState, GameState newState)
    {
        ShowPanelForCurrentState();

        // ����������� �������� ��� ��������� ���������
        switch (newState)
        {
            case GameState.Constructor:
                InitializeConstructorUI();
                break;
            case GameState.ExecutingCode:
                SetUIInteractable(false);
                break;
            default:
                SetUIInteractable(true);
                break;
        }
    }

    private void HandleWebSocketConnected()
    {
        UpdateConnectionStatus(true);
    }

    private void HandleWebSocketDisconnected(string reason)
    {
        UpdateConnectionStatus(false);
    }

    private void HandleStatusMessage(string message)
    {
        // ���������� ��������� ��������� � �������
        consoleOutput.AddMessage(message, ConsoleOutput.LogType.Status);
    }

    private void ShowPanelForCurrentState()
    {
        // �������� ��� ������
        foreach (var _panel in statePanels.Values)
        {
            if (_panel != null) _panel.SetActive(false);
        }

        // ���������� ������ ��� �������� ���������
        if (statePanels.TryGetValue(gameStateManager.CurrentState, out GameObject panel))
        {
            if (panel != null) panel.SetActive(true);
        }
    }

    private void InitializeConstructorUI()
    {
        // ������������� UI ������������
        consoleOutput.Clear();
        toolbarManager.UpdateButtonStates();
    }

    private void SetUIInteractable(bool interactable)
    {
        // ���������� interactable ���������� UI ���������
        if (toolbarManager != null)
            toolbarManager.SetInteractable(interactable);

        if (codeEditor != null)
            codeEditor.SetInteractable(interactable);
    }

    private void UpdateConnectionStatus(bool connected)
    {
        // ���������� ���������� ������� ����������
        if (toolbarManager != null)
            toolbarManager.UpdateConnectionStatus(connected);

        string statusMessage = connected ? "Connected to server" : "Disconnected from server";
        consoleOutput.AddMessage(statusMessage, connected ? ConsoleOutput.LogType.Success : ConsoleOutput.LogType.Warning);
    }

    public void ShowLoadingScreen(bool show)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(show);
    }

    public void ShowError(string message, ErrorSeverity severity)
    {
        // ����� ������ � UI
        consoleOutput.AddMessage($"ERROR: {message}", ConsoleOutput.LogType.Error);

        // �������������� �������� � ����������� �� ����������� ������
        if (severity >= ErrorSeverity.High)
        {
            // ��� ����������� ������ ����� �������� ��������� ����
            Debug.LogError($"Critical error: {message}");
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
}