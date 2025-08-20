using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

/// <summary>
/// Компонент для отображения логов и сообщений в консоли
/// </summary>
public class ConsoleOutput : MonoBehaviour
{
    public enum LogType
    {
        Info,
        Warning,
        Error,
        Success,
        Status,
        GPIO
    }

    [Header("UI References")]
    public TMP_Text consoleText;
    public ScrollRect scrollRect;
    public ButtonHandler clearButton;
    public ButtonHandler copyButton;
    public TMP_Dropdown filterDropdown;

    [Header("Console Settings")]
    public int maxLines = 1000;
    public bool autoScroll = true;
    public bool showTimestamps = true;

    [Header("Colors")]
    public Color infoColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color errorColor = Color.red;
    public Color successColor = Color.green;
    public Color statusColor = Color.cyan;
    public Color gpioColor = Color.magenta;

    private StringBuilder content = new StringBuilder();
    private List<string> allMessages = new List<string>();
    private LogType currentFilter = LogType.Info;
    private bool needsRefresh = false;

    /// <summary>
    /// Инициализация при старте
    /// </summary>
    private void Start()
    {
        InitializeUI();
        Clear();
    }

    /// <summary>
    /// Инициализация UI элементов
    /// </summary>
    private void InitializeUI()
    {
        if (clearButton != null)
            clearButton.OnClick.AddListener(Clear);

        if (copyButton != null)
            copyButton.OnClick.AddListener(CopyToClipboard);

        if (filterDropdown != null)
        {
            filterDropdown.onValueChanged.AddListener(OnFilterChanged);

            // Настройка вариантов фильтрации
            filterDropdown.ClearOptions();
            filterDropdown.AddOptions(new List<string> {
                "All", "Info", "Warnings", "Errors", "Success", "Status", "GPIO"
            });
        }
    }

    /// <summary>
    /// Обновление каждый кадр для обработки обновлений отображения
    /// </summary>
    private void Update()
    {
        if (needsRefresh)
        {
            RefreshDisplay();
            needsRefresh = false;
        }
    }

    /// <summary>
    /// Добавление сообщения в консоль
    /// </summary>
    public void AddMessage(string message, LogType type = LogType.Info)
    {
        // Форматирование сообщения
        string formattedMessage = FormatMessage(message, type);

        // Добавление в общий список
        allMessages.Add(formattedMessage);

        // Добавление к отображаемому содержимому если соответствует фильтру
        if (ShouldDisplay(type))
        {
            content.AppendLine(formattedMessage);
            TrimExcessLines();
        }

        needsRefresh = true;
    }

    /// <summary>
    /// Добавление сообщения с пользовательским цветом
    /// </summary>
    public void AddMessage(string message, string hexColor)
    {
        string coloredMessage = $"<color={hexColor}>{message}</color>";
        AddMessage(coloredMessage, LogType.Info);
    }

    /// <summary>
    /// Форматирование сообщения с учетом типа
    /// </summary>
    private string FormatMessage(string message, LogType type)
    {
        StringBuilder formatted = new StringBuilder();

        // Добавление временной метки
        if (showTimestamps)
        {
            formatted.Append($"[{System.DateTime.Now:HH:mm:ss}] ");
        }

        // Добавление префикса типа сообщения
        switch (type)
        {
            case LogType.Warning:
                formatted.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(warningColor)).Append(">[WARN] </color>");
                break;
            case LogType.Error:
                formatted.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(errorColor)).Append(">[ERROR] </color>");
                break;
            case LogType.Success:
                formatted.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(successColor)).Append(">[SUCCESS] </color>");
                break;
            case LogType.Status:
                formatted.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(statusColor)).Append(">[STATUS] </color>");
                break;
            case LogType.GPIO:
                formatted.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(gpioColor)).Append(">[GPIO] </color>");
                break;
        }

        // Добавление самого сообщения с цветом
        string colorHex = GetColorHex(type);
        formatted.Append("<color=").Append(colorHex).Append(">")
                .Append(message)
                .Append("</color>");

        return formatted.ToString();
    }

    /// <summary>
    /// Получение HEX кода цвета для типа сообщения
    /// </summary>
    private string GetColorHex(LogType type)
    {
        switch (type)
        {
            case LogType.Info: return "#" + ColorUtility.ToHtmlStringRGB(infoColor);
            case LogType.Warning: return "#" + ColorUtility.ToHtmlStringRGB(warningColor);
            case LogType.Error: return "#" + ColorUtility.ToHtmlStringRGB(errorColor);
            case LogType.Success: return "#" + ColorUtility.ToHtmlStringRGB(successColor);
            case LogType.Status: return "#" + ColorUtility.ToHtmlStringRGB(statusColor);
            case LogType.GPIO: return "#" + ColorUtility.ToHtmlStringRGB(gpioColor);
            default: return "#" + ColorUtility.ToHtmlStringRGB(infoColor);
        }
    }

    /// <summary>
    /// Проверка, должно ли сообщение отображаться при текущем фильтре
    /// </summary>
    private bool ShouldDisplay(LogType type)
    {
        if (currentFilter == LogType.Info) // Info фильтрует все
            return true;

        return type == currentFilter;
    }

    /// <summary>
    /// Удаление лишних строк при превышении лимита
    /// </summary>
    private void TrimExcessLines()
    {
        if (content.Length == 0) return;

        // Подсчет строк и удаление старых при необходимости
        int lineCount = 0;
        int index = content.Length - 1;

        while (index > 0 && lineCount < maxLines)
        {
            if (content[index] == '\n')
                lineCount++;

            index--;
        }

        if (lineCount >= maxLines)
        {
            int firstNewLine = content.ToString().IndexOf('\n');
            if (firstNewLine >= 0)
            {
                content.Remove(0, firstNewLine + 1);
            }
        }
    }

    /// <summary>
    /// Обновление отображения консоли
    /// </summary>
    private void RefreshDisplay()
    {
        if (consoleText != null)
        {
            consoleText.text = content.ToString();

            // Автопрокрутка к нижней части
            if (autoScroll && scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }

    /// <summary>
    /// Очистка консоли
    /// </summary>
    public void Clear()
    {
        content.Clear();
        allMessages.Clear();
        RefreshDisplay();
    }

    /// <summary>
    /// Копирование содержимого консоли в буфер обмена
    /// </summary>
    public void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = content.ToString();
        AddMessage("Console content copied to clipboard", LogType.Success);
    }

    /// <summary>
    /// Обработчик изменения фильтра
    /// </summary>
    private void OnFilterChanged(int index)
    {
        // Обновление фильтра на основе выбранного варианта
        switch (index)
        {
            case 0: currentFilter = LogType.Info; break;      // All
            case 1: currentFilter = LogType.Info; break;      // Info
            case 2: currentFilter = LogType.Warning; break;   // Warnings
            case 3: currentFilter = LogType.Error; break;     // Errors
            case 4: currentFilter = LogType.Success; break;   // Success
            case 5: currentFilter = LogType.Status; break;    // Status
            case 6: currentFilter = LogType.GPIO; break;      // GPIO
        }

        // Перестроение отображаемого содержимого с учетом фильтра
        content.Clear();

        foreach (string message in allMessages)
        {
            // Определяем тип сообщения по его формату
            LogType messageType = DetectMessageType(message);

            if (ShouldDisplay(messageType))
            {
                content.AppendLine(message);
            }
        }

        TrimExcessLines();
        RefreshDisplay();

        AddMessage($"Filter changed to: {filterDropdown.options[index].text}", LogType.Status);
    }

    /// <summary>
    /// Определение типа сообщения по его содержимому
    /// </summary>
    private LogType DetectMessageType(string message)
    {
        if (message.Contains("[WARN]")) return LogType.Warning;
        if (message.Contains("[ERROR]")) return LogType.Error;
        if (message.Contains("[SUCCESS]")) return LogType.Success;
        if (message.Contains("[STATUS]")) return LogType.Status;
        if (message.Contains("[GPIO]")) return LogType.GPIO;
        return LogType.Info;
    }

    /// <summary>
    /// Установка автоскролла
    /// </summary>
    public void SetAutoScroll(bool enabled)
    {
        autoScroll = enabled;
    }

    /// <summary>
    /// Установка отображения временных меток
    /// </summary>
    public void SetShowTimestamps(bool enabled)
    {
        showTimestamps = enabled;
    }

    /// <summary>
    /// Очистка при уничтожении объекта
    /// </summary>
    private void OnDestroy()
    {
        // Отписываемся от событий кнопок
        if (clearButton != null)
            clearButton.OnClick.RemoveListener(Clear);

        if (copyButton != null)
            copyButton.OnClick.RemoveListener(CopyToClipboard);

        // Отписываемся от событий Dropdown
        if (filterDropdown != null)
        {
            filterDropdown.onValueChanged.RemoveListener(OnFilterChanged);
        }
    }
}