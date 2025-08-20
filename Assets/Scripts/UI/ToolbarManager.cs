using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ���������� ������� ������������ � �������� ��������
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
    /// ������������� ��� ������
    /// </summary>
    private void Start()
    {
        InitializeReferences();
        InitializeButtons();
        UpdateButtonStates();
        UpdateConnectionStatus(false);
    }

    /// <summary>
    /// ������������� ������ �� ����������
    /// </summary>
    private void InitializeReferences()
    {
        webSocketClient = WebSocketClient.Instance;
        gameStateManager = GameStateManager.Instance;
        codeEditor = FindObjectOfType<CodeEditor>();
        consoleOutput = FindObjectOfType<ConsoleOutput>();
    }

    /// <summary>
    /// ������������� ������������ ������
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
    /// ���������� ������ �������
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
    /// ���������� ������ ���������
    /// </summary>
    private void OnStopButtonClick()
    {
        EventSystem.TriggerStatusMessage("Stopping execution...");
        // TODO: ����������� ��������� ����������
    }

    /// <summary>
    /// ���������� ������ �������
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
    /// ���������� ������ ����������
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
    /// ���������� ������ ��������
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
    /// ���������� ������ ��������
    /// </summary>
    private void OnSettingsButtonClick()
    {
        EventSystem.TriggerStatusMessage("Opening settings...");
        // TODO: ����������� �������� ��������
    }

    /// <summary>
    /// ���������� ��������� ������
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
    /// ���������� ������� ����������
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
    /// ��������� interactable ���������
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
    /// �������� �� ������� ��� ���������
    /// </summary>
    private void OnEnable()
    {
        EventSystem.OnWebSocketConnected += HandleWebSocketConnected;
        EventSystem.OnWebSocketDisconnected += HandleWebSocketDisconnected;
        EventSystem.OnGameStateChanged += HandleGameStateChanged;
    }

    /// <summary>
    /// ������� �� ������� ��� ����������
    /// </summary>
    private void OnDisable()
    {
        EventSystem.OnWebSocketConnected -= HandleWebSocketConnected;
        EventSystem.OnWebSocketDisconnected -= HandleWebSocketDisconnected;
        EventSystem.OnGameStateChanged -= HandleGameStateChanged;
    }

    /// <summary>
    /// ���������� ����������� WebSocket
    /// </summary>
    private void HandleWebSocketConnected()
    {
        UpdateConnectionStatus(true);
    }

    /// <summary>
    /// ���������� ���������� WebSocket
    /// </summary>
    private void HandleWebSocketDisconnected(string reason)
    {
        UpdateConnectionStatus(false);
    }

    /// <summary>
    /// ���������� ��������� ��������� ����
    /// </summary>
    private void HandleGameStateChanged(GameState oldState, GameState newState)
    {
        UpdateButtonStates();
    }

    /// <summary>
    /// ������� ��� �����������
    /// </summary>
    private void OnDestroy()
    {
        // ������������ �� ���� ������
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