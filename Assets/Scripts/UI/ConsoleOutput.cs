using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

/// <summary>
/// ��������� ��� ����������� ����� � ��������� � �������
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
    /// ������������� ��� ������
    /// </summary>
    private void Start()
    {
        InitializeUI();
        Clear();
    }

    /// <summary>
    /// ������������� UI ���������
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

            // ��������� ��������� ����������
            filterDropdown.ClearOptions();
            filterDropdown.AddOptions(new List<string> {
                "All", "Info", "Warnings", "Errors", "Success", "Status", "GPIO"
            });
        }
    }

    /// <summary>
    /// ���������� ������ ���� ��� ��������� ���������� �����������
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
    /// ���������� ��������� � �������
    /// </summary>
    public void AddMessage(string message, LogType type = LogType.Info)
    {
        // �������������� ���������
        string formattedMessage = FormatMessage(message, type);

        // ���������� � ����� ������
        allMessages.Add(formattedMessage);

        // ���������� � ������������� ����������� ���� ������������� �������
        if (ShouldDisplay(type))
        {
            content.AppendLine(formattedMessage);
            TrimExcessLines();
        }

        needsRefresh = true;
    }

    /// <summary>
    /// ���������� ��������� � ���������������� ������
    /// </summary>
    public void AddMessage(string message, string hexColor)
    {
        string coloredMessage = $"<color={hexColor}>{message}</color>";
        AddMessage(coloredMessage, LogType.Info);
    }

    /// <summary>
    /// �������������� ��������� � ������ ����
    /// </summary>
    private string FormatMessage(string message, LogType type)
    {
        StringBuilder formatted = new StringBuilder();

        // ���������� ��������� �����
        if (showTimestamps)
        {
            formatted.Append($"[{System.DateTime.Now:HH:mm:ss}] ");
        }

        // ���������� �������� ���� ���������
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

        // ���������� ������ ��������� � ������
        string colorHex = GetColorHex(type);
        formatted.Append("<color=").Append(colorHex).Append(">")
                .Append(message)
                .Append("</color>");

        return formatted.ToString();
    }

    /// <summary>
    /// ��������� HEX ���� ����� ��� ���� ���������
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
    /// ��������, ������ �� ��������� ������������ ��� ������� �������
    /// </summary>
    private bool ShouldDisplay(LogType type)
    {
        if (currentFilter == LogType.Info) // Info ��������� ���
            return true;

        return type == currentFilter;
    }

    /// <summary>
    /// �������� ������ ����� ��� ���������� ������
    /// </summary>
    private void TrimExcessLines()
    {
        if (content.Length == 0) return;

        // ������� ����� � �������� ������ ��� �������������
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
    /// ���������� ����������� �������
    /// </summary>
    private void RefreshDisplay()
    {
        if (consoleText != null)
        {
            consoleText.text = content.ToString();

            // ������������� � ������ �����
            if (autoScroll && scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }

    /// <summary>
    /// ������� �������
    /// </summary>
    public void Clear()
    {
        content.Clear();
        allMessages.Clear();
        RefreshDisplay();
    }

    /// <summary>
    /// ����������� ����������� ������� � ����� ������
    /// </summary>
    public void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = content.ToString();
        AddMessage("Console content copied to clipboard", LogType.Success);
    }

    /// <summary>
    /// ���������� ��������� �������
    /// </summary>
    private void OnFilterChanged(int index)
    {
        // ���������� ������� �� ������ ���������� ��������
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

        // ������������ ������������� ����������� � ������ �������
        content.Clear();

        foreach (string message in allMessages)
        {
            // ���������� ��� ��������� �� ��� �������
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
    /// ����������� ���� ��������� �� ��� �����������
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
    /// ��������� �����������
    /// </summary>
    public void SetAutoScroll(bool enabled)
    {
        autoScroll = enabled;
    }

    /// <summary>
    /// ��������� ����������� ��������� �����
    /// </summary>
    public void SetShowTimestamps(bool enabled)
    {
        showTimestamps = enabled;
    }

    /// <summary>
    /// ������� ��� ����������� �������
    /// </summary>
    private void OnDestroy()
    {
        // ������������ �� ������� ������
        if (clearButton != null)
            clearButton.OnClick.RemoveListener(Clear);

        if (copyButton != null)
            copyButton.OnClick.RemoveListener(CopyToClipboard);

        // ������������ �� ������� Dropdown
        if (filterDropdown != null)
        {
            filterDropdown.onValueChanged.RemoveListener(OnFilterChanged);
        }
    }
}