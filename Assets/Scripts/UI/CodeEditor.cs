using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Редактор кода с подсветкой синтаксиса и базовыми функциями редактирования
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
    /// Инициализация при старте
    /// </summary>
    private void Start()
    {
        webSocketClient = WebSocketClient.Instance;
        InitializeButtons();
        ConfigureInputField();
        LoadDefaultTemplate();
    }

    /// <summary>
    /// Инициализация кнопок редактора
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
    /// Настройка поля ввода кода
    /// </summary>
    private void ConfigureInputField()
    {
        if (codeInputField != null)
        {
            codeInputField.onValueChanged.AddListener(OnCodeChanged);
            codeInputField.onEndEdit.AddListener(OnCodeEndEdit);

            // Настройка внешнего вида - используем textComponent для доступа к свойствам TMP_Text
            codeInputField.pointSize = fontSize;

            // Устанавливаем межстрочный интервал через textComponent
            if (codeInputField.textComponent != null)
            {
                codeInputField.textComponent.lineSpacing = lineSpacing;
            }

            // Также устанавливаем для placeholder, если он есть
            if (codeInputField.placeholder is TextMeshProUGUI placeholderTMP)
            {
                placeholderTMP.lineSpacing = lineSpacing;
            }
        }
    }

    /// <summary>
    /// Загрузка шаблона кода по умолчанию
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
    /// Обработчик изменения кода
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
    /// Обработчик завершения редактирования
    /// </summary>
    private void OnCodeEndEdit(string finalCode)
    {
        currentCode = finalCode;
    }

    /// <summary>
    /// Применение подсветки синтаксиса
    /// </summary>
    private void ApplySyntaxHighlighting()
    {
        if (codeInputField == null || string.IsNullOrEmpty(currentCode))
            return;

        // Базовая подсветка синтаксиса Python
        string highlightedCode = currentCode;

        // Подсветка ключевых слов
        highlightedCode = Regex.Replace(highlightedCode,
            @"\b(import|from|as|def|class|return|if|elif|else|for|while|break|continue|try|except|finally|with|lambda)\b",
            "<color=#569CD6>$1</color>");

        // Подсветка функций
        highlightedCode = Regex.Replace(highlightedCode,
            @"(\b[A-Za-z_][A-Za-z0-9_]*\b)(?=\()",
            "<color=#DCDCAA>$1</color>");

        // Подсветка строк
        highlightedCode = Regex.Replace(highlightedCode,
            @"(\""""\"".*?\""""\"")|(\"".*?\"")|('.*?')",
            "<color=#CE9178>$0</color>");

        // Подсветка комментариев
        highlightedCode = Regex.Replace(highlightedCode,
            @"(#.*)$",
            "<color=#6A9955>$0</color>",
            RegexOptions.Multiline);

        // Подсветка чисел
        highlightedCode = Regex.Replace(highlightedCode,
            @"\b(\d+\.?\d*)\b",
            "<color=#B5CEA8>$1</color>");

        // Обновление текста без вызова события onChange
        codeInputField.SetTextWithoutNotify(highlightedCode);
    }

    /// <summary>
    /// Запуск выполнения кода
    /// </summary>
    public void RunCode()
    {
        if (string.IsNullOrEmpty(currentCode))
        {
            Debug.LogWarning("No code to execute");
            EventSystem.TriggerStatusMessage("No code to execute");
            return;
        }

        // Отправка кода на выполнение через WebSocket
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
    /// Очистка редактора кода
    /// </summary>
    public void ClearCode()
    {
        currentCode = "";
        UpdateCodeDisplay();
        EventSystem.TriggerStatusMessage("Code editor cleared");
    }

    /// <summary>
    /// Сохранение кода во временное хранилище
    /// </summary>
    public void SaveCode()
    {
        savedCode = currentCode;
        EventSystem.TriggerStatusMessage("Code saved temporarily");
    }

    /// <summary>
    /// Загрузка сохраненного кода
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
    /// Загрузка шаблона кода
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
    /// Установка interactable состояния
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
    /// Обновление отображения кода
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
    /// Получение текущего кода
    /// </summary>
    public string GetCurrentCode()
    {
        return currentCode;
    }

    /// <summary>
    /// Установка кода в редактор
    /// </summary>
    public void SetCode(string code)
    {
        currentCode = code;
        UpdateCodeDisplay();
    }

    /// <summary>
    /// Очистка при уничтожении
    /// </summary>
    private void OnDestroy()
    {
        // Отписываемся от событий кнопок
        if (runButton != null)
            runButton.OnClick.RemoveListener(RunCode);

        if (clearButton != null)
            clearButton.OnClick.RemoveListener(ClearCode);

        if (saveButton != null)
            saveButton.OnClick.RemoveListener(SaveCode);

        if (loadButton != null)
            loadButton.OnClick.RemoveListener(LoadCode);

        // Отписываемся от событий InputField
        if (codeInputField != null)
        {
            codeInputField.onValueChanged.RemoveListener(OnCodeChanged);
            codeInputField.onEndEdit.RemoveListener(OnCodeEndEdit);
        }
    }
}