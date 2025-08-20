using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// �������� ���� � ���������� ���������� � �������� ��������� ��������������
/// </summary>
public class CodeEditor : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField codeInputField;
    public ButtonHandler runButton;
    public ButtonHandler clearButton;
    public ButtonHandler saveButton;
    public ButtonHandler loadButton;

    [Header("Editor Settings")]
    public bool enableSyntaxHighlighting = true;
    public bool enableLineNumbers = true;
    public int fontSize = 14;
    public float lineSpacing = 1.2f;

    [Header("Code Templates")]
    public List<string> codeTemplates;

    private string currentCode = "";
    private string savedCode = "";
    private WebSocketClient webSocketClient;

    /// <summary>
    /// ������������� ��� ������
    /// </summary>
    private void Start()
    {
        webSocketClient = WebSocketClient.Instance;
        InitializeButtons();
        ConfigureInputField();
        LoadDefaultTemplate();
    }

    /// <summary>
    /// ������������� ������ ���������
    /// </summary>
    private void InitializeButtons()
    {
        if (runButton != null)
            runButton.OnClick.AddListener(RunCode);

        if (clearButton != null)
            clearButton.OnClick.AddListener(ClearCode);

        if (saveButton != null)
            saveButton.OnClick.AddListener(SaveCode);

        if (loadButton != null)
            loadButton.OnClick.AddListener(LoadCode);
    }

    /// <summary>
    /// ��������� ���� ����� ����
    /// </summary>
    private void ConfigureInputField()
    {
        if (codeInputField != null)
        {
            codeInputField.onValueChanged.AddListener(OnCodeChanged);
            codeInputField.onEndEdit.AddListener(OnCodeEndEdit);

            // ��������� �������� ���� - ���������� textComponent ��� ������� � ��������� TMP_Text
            codeInputField.pointSize = fontSize;

            // ������������� ����������� �������� ����� textComponent
            if (codeInputField.textComponent != null)
            {
                codeInputField.textComponent.lineSpacing = lineSpacing;
            }

            // ����� ������������� ��� placeholder, ���� �� ����
            if (codeInputField.placeholder is TextMeshProUGUI placeholderTMP)
            {
                placeholderTMP.lineSpacing = lineSpacing;
            }
        }
    }

    /// <summary>
    /// �������� ������� ���� �� ���������
    /// </summary>
    private void LoadDefaultTemplate()
    {
        if (codeTemplates != null && codeTemplates.Count > 0)
        {
            currentCode = codeTemplates[0];
            UpdateCodeDisplay();
        }
    }

    /// <summary>
    /// ���������� ��������� ����
    /// </summary>
    private void OnCodeChanged(string newCode)
    {
        currentCode = newCode;

        if (enableSyntaxHighlighting)
        {
            ApplySyntaxHighlighting();
        }
    }

    /// <summary>
    /// ���������� ���������� ��������������
    /// </summary>
    private void OnCodeEndEdit(string finalCode)
    {
        currentCode = finalCode;
    }

    /// <summary>
    /// ���������� ��������� ����������
    /// </summary>
    private void ApplySyntaxHighlighting()
    {
        if (codeInputField == null || string.IsNullOrEmpty(currentCode))
            return;

        // ������� ��������� ���������� Python
        string highlightedCode = currentCode;

        // ��������� �������� ����
        highlightedCode = Regex.Replace(highlightedCode,
            @"\b(import|from|as|def|class|return|if|elif|else|for|while|break|continue|try|except|finally|with|lambda)\b",
            "<color=#569CD6>$1</color>");

        // ��������� �������
        highlightedCode = Regex.Replace(highlightedCode,
            @"(\b[A-Za-z_][A-Za-z0-9_]*\b)(?=\()",
            "<color=#DCDCAA>$1</color>");

        // ��������� �����
        highlightedCode = Regex.Replace(highlightedCode,
            @"(\""""\"".*?\""""\"")|(\"".*?\"")|('.*?')",
            "<color=#CE9178>$0</color>");

        // ��������� ������������
        highlightedCode = Regex.Replace(highlightedCode,
            @"(#.*)$",
            "<color=#6A9955>$0</color>",
            RegexOptions.Multiline);

        // ��������� �����
        highlightedCode = Regex.Replace(highlightedCode,
            @"\b(\d+\.?\d*)\b",
            "<color=#B5CEA8>$1</color>");

        // ���������� ������ ��� ������ ������� onChange
        codeInputField.SetTextWithoutNotify(highlightedCode);
    }

    /// <summary>
    /// ������ ���������� ����
    /// </summary>
    public void RunCode()
    {
        if (string.IsNullOrEmpty(currentCode))
        {
            Debug.LogWarning("No code to execute");
            EventSystem.TriggerStatusMessage("No code to execute");
            return;
        }

        // �������� ���� �� ���������� ����� WebSocket
        EventSystem.TriggerStatusMessage("Sending code for execution...");

        if (webSocketClient != null && webSocketClient.IsConnected)
        {
            var message = new
            {
                type = "code_execution",
                code = currentCode
            };

            webSocketClient.SendMessage(JsonUtility.ToJson(message));
        }
        else
        {
            EventSystem.TriggerApplicationError("Not connected to server", ErrorSeverity.Medium);
        }
    }

    /// <summary>
    /// ������� ��������� ����
    /// </summary>
    public void ClearCode()
    {
        currentCode = "";
        UpdateCodeDisplay();
        EventSystem.TriggerStatusMessage("Code editor cleared");
    }

    /// <summary>
    /// ���������� ���� �� ��������� ���������
    /// </summary>
    public void SaveCode()
    {
        savedCode = currentCode;
        EventSystem.TriggerStatusMessage("Code saved temporarily");
    }

    /// <summary>
    /// �������� ������������ ����
    /// </summary>
    public void LoadCode()
    {
        if (!string.IsNullOrEmpty(savedCode))
        {
            currentCode = savedCode;
            UpdateCodeDisplay();
            EventSystem.TriggerStatusMessage("Code loaded");
        }
        else
        {
            EventSystem.TriggerStatusMessage("No saved code to load");
        }
    }

    /// <summary>
    /// �������� ������� ����
    /// </summary>
    public void LoadTemplate(int templateIndex)
    {
        if (codeTemplates != null && templateIndex >= 0 && templateIndex < codeTemplates.Count)
        {
            currentCode = codeTemplates[templateIndex];
            UpdateCodeDisplay();
            EventSystem.TriggerStatusMessage($"Loaded template #{templateIndex + 1}");
        }
    }

    /// <summary>
    /// ��������� interactable ���������
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (codeInputField != null)
            codeInputField.interactable = interactable;

        if (runButton != null)
            runButton.SetInteractable(interactable);

        if (clearButton != null)
            clearButton.SetInteractable(interactable);

        if (saveButton != null)
            saveButton.SetInteractable(interactable);

        if (loadButton != null)
            loadButton.SetInteractable(interactable);
    }

    /// <summary>
    /// ���������� ����������� ����
    /// </summary>
    private void UpdateCodeDisplay()
    {
        if (codeInputField != null)
        {
            codeInputField.text = currentCode;
            if (enableSyntaxHighlighting)
            {
                ApplySyntaxHighlighting();
            }
        }
    }

    /// <summary>
    /// ��������� �������� ����
    /// </summary>
    public string GetCurrentCode()
    {
        return currentCode;
    }

    /// <summary>
    /// ��������� ���� � ��������
    /// </summary>
    public void SetCode(string code)
    {
        currentCode = code;
        UpdateCodeDisplay();
    }

    /// <summary>
    /// ������� ��� �����������
    /// </summary>
    private void OnDestroy()
    {
        // ������������ �� ������� ������
        if (runButton != null)
            runButton.OnClick.RemoveListener(RunCode);

        if (clearButton != null)
            clearButton.OnClick.RemoveListener(ClearCode);

        if (saveButton != null)
            saveButton.OnClick.RemoveListener(SaveCode);

        if (loadButton != null)
            loadButton.OnClick.RemoveListener(LoadCode);

        // ������������ �� ������� InputField
        if (codeInputField != null)
        {
            codeInputField.onValueChanged.RemoveListener(OnCodeChanged);
            codeInputField.onEndEdit.RemoveListener(OnCodeEndEdit);
        }
    }
}