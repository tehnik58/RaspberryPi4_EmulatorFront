using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static MessageClassifier;

public class ConsoleOutput : MonoBehaviour
{
    [Header("Required References")]
    [SerializeField] private TMP_Text consoleText;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Settings")]
    [SerializeField] private int maxLines = 1000;
    [SerializeField] private bool autoScroll = true;
    [SerializeField] private bool enableRichText = true;
    [SerializeField] private bool extractFromJson = true; // Новая опция для извлечения из JSON

    [Header("Message Colors")]
    [SerializeField] private Color infoColor = Color.white;
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;

    private Queue<string> messageQueue = new Queue<string>();

    private void Awake()
    {
        ValidateComponents();
        SetupTextComponent();
    }

    private void ValidateComponents()
    {
        if (consoleText == null)
            consoleText = GetComponentInChildren<TMP_Text>();

        if (scrollRect == null)
            scrollRect = GetComponentInChildren<ScrollRect>();
    }

    private void SetupTextComponent()
    {
        if (consoleText != null)
        {
            consoleText.richText = enableRichText;
        }
    }

    public void AddMessage(string message, MessageType type = MessageType.Info)
    {
        if (string.IsNullOrEmpty(message)) return;

        // Извлекаем чистое сообщение из JSON если нужно
        string cleanMessage = extractFromJson ? ExtractContentFromJson(message) : message;

        string formattedMessage = enableRichText ?
            FormatMessageWithColor(cleanMessage, GetColorForType(type)) :
            FormatMessagePlain(cleanMessage);

        messageQueue.Enqueue(formattedMessage);

        while (messageQueue.Count > maxLines)
            messageQueue.Dequeue();

        UpdateConsoleDisplay();

        if (autoScroll)
            ScrollToBottom();
    }

    public void AddMessage(string message, Color customColor)
    {
        if (string.IsNullOrEmpty(message)) return;

        // Извлекаем чистое сообщение из JSON если нужно
        string cleanMessage = extractFromJson ? ExtractContentFromJson(message) : message;

        string formattedMessage = enableRichText ?
            FormatMessageWithColor(cleanMessage, customColor) :
            FormatMessagePlain(cleanMessage);

        messageQueue.Enqueue(formattedMessage);

        while (messageQueue.Count > maxLines)
            messageQueue.Dequeue();

        UpdateConsoleDisplay();

        if (autoScroll)
            ScrollToBottom();
    }

    // Метод для извлечения content из JSON сообщения
    private string ExtractContentFromJson(string jsonMessage)
    {
        try
        {
            // Простая проверка, является ли строка JSON
            if (jsonMessage.Trim().StartsWith("{") && jsonMessage.Trim().EndsWith("}"))
            {
                // Ищем поле "content" в JSON
                int contentStart = jsonMessage.IndexOf("\"content\"") + "\"content\"".Length;
                if (contentStart > 0)
                {
                    // Пропускаем двоеточие и пробелы
                    contentStart = jsonMessage.IndexOf(':', contentStart) + 1;
                    while (contentStart < jsonMessage.Length && char.IsWhiteSpace(jsonMessage[contentStart]))
                        contentStart++;

                    // Определяем начало и конец значения
                    int valueStart = contentStart;
                    int valueEnd = jsonMessage.Length - 1;

                    // Если значение в кавычках
                    if (valueStart < jsonMessage.Length && jsonMessage[valueStart] == '"')
                    {
                        valueStart++; // Пропускаем открывающую кавычку
                        valueEnd = jsonMessage.IndexOf('"', valueStart);
                        if (valueEnd < 0) valueEnd = jsonMessage.Length - 1;
                    }
                    else
                    {
                        // Ищем конец значения (запятая или закрывающая скобка)
                        valueEnd = valueStart;
                        while (valueEnd < jsonMessage.Length &&
                               jsonMessage[valueEnd] != ',' &&
                               jsonMessage[valueEnd] != '}')
                        {
                            valueEnd++;
                        }
                    }

                    if (valueStart < valueEnd)
                    {
                        return jsonMessage.Substring(valueStart, valueEnd - valueStart).Trim();
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to extract content from JSON: {e.Message}");
        }

        // Если не удалось извлечь из JSON, возвращаем оригинальное сообщение
        return jsonMessage;
    }

    private string FormatMessageWithColor(string message, Color color)
    {
        string timestamp = $"[{System.DateTime.Now:HH:mm:ss}] ";
        string hexColor = ColorUtility.ToHtmlStringRGB(color);
        return $"<color=#{hexColor}>{timestamp}{message}</color>\n";
    }

    private string FormatMessagePlain(string message)
    {
        string timestamp = $"[{System.DateTime.Now:HH:mm:ss}] ";
        return $"{timestamp}{message}\n";
    }

    private void UpdateConsoleDisplay()
    {
        if (consoleText != null)
            consoleText.text = string.Join("", messageQueue);
    }

    private void ScrollToBottom()
    {
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    public void Clear()
    {
        messageQueue.Clear();
        UpdateConsoleDisplay();
    }

    public void SetMaxLines(int newMaxLines)
    {
        maxLines = Mathf.Max(1, newMaxLines);
        while (messageQueue.Count > maxLines)
            messageQueue.Dequeue();

        UpdateConsoleDisplay();
    }

    public void SetAutoScroll(bool enable)
    {
        autoScroll = enable;
        if (autoScroll)
            ScrollToBottom();
    }

    public void SetExtractFromJson(bool enable)
    {
        extractFromJson = enable;
    }

    private Color GetColorForType(MessageType type)
    {
        return type switch
        {
            MessageType.Info => infoColor,
            MessageType.Error => errorColor,
            MessageType.Success => successColor,
            MessageType.Warning => warningColor,
            _ => infoColor
        };
    }

    // Public shortcuts for message types
    public void AddInfo(string message) => AddMessage(message, MessageType.Info);
    public void AddError(string message) => AddMessage(message, MessageType.Error);
    public void AddSuccess(string message) => AddMessage(message, MessageType.Success);
    public void AddWarning(string message) => AddMessage(message, MessageType.Warning);
}