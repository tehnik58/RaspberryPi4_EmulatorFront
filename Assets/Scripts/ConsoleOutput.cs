using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConsoleOutput : MonoBehaviour
{
    public enum MessageType
    {
        Info,
        Error,
        Success,
        Warning
    }

    [SerializeField] private TMP_Text consoleText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private int maxLines = 1000;
    [SerializeField] private bool autoScroll = true;

    [Header("Colors")]
    [SerializeField] private Color infoColor = Color.white;
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;

    private Queue<string> messageQueue = new Queue<string>();
    private bool isDirty = false;
    private float scrollTimer = 0f;
    private const float scrollDelay = 0.1f;

    void Start()
    {
        if (consoleText == null)
        {
            consoleText = GetComponentInChildren<TMP_Text>();
        }
        
        if (scrollRect == null)
        {
            scrollRect = GetComponent<ScrollRect>();
        }
        
        Clear();
        AddMessage("Консоль инициализирована", MessageType.Info);
    }

    void Update()
    {
        if (isDirty)
        {
            scrollTimer += Time.deltaTime;
            if (scrollTimer >= scrollDelay)
            {
                UpdateConsoleDisplay();
                if (autoScroll)
                {
                    ScrollToBottom();
                }
                isDirty = false;
                scrollTimer = 0f;
            }
        }
    }

    public void AddMessage(string message, MessageType type = MessageType.Info)
    {
        if (string.IsNullOrEmpty(message)) return;

        Color color = GetColorForType(type);
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        string coloredMessage = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>[{timestamp}] {message}</color>\n";

        messageQueue.Enqueue(coloredMessage);

        // Ограничиваем количество строк
        while (messageQueue.Count > maxLines)
        {
            messageQueue.Dequeue();
        }

        isDirty = true;
        scrollTimer = 0f;
    }

    public void Clear()
    {
        messageQueue.Clear();
        consoleText.text = "";
        AddMessage("Консоль очищена", MessageType.Info);
    }

    private void UpdateConsoleDisplay()
    {
        if (consoleText != null)
        {
            consoleText.text = string.Join("", messageQueue);
        }
    }

    public void ScrollToBottom()
    {
        if (scrollRect != null)
        {
            // Несколько способов прокрутки для надежности
            Canvas.ForceUpdateCanvases();
            
            // Способ 1: Прямая установка позиции
            scrollRect.verticalNormalizedPosition = 0f;
            
            // Способ 2: Через Content позицию
            if (scrollRect.content != null)
            {
                scrollRect.content.anchoredPosition = new Vector2(
                    scrollRect.content.anchoredPosition.x,
                    0f
                );
            }
            
            // Способ 3: Через Layout Group
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        }
    }

    public void ScrollToTop()
    {
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    public void ToggleAutoScroll(bool enable)
    {
        autoScroll = enable;
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

    // Методы для быстрого добавления сообщений
    public void AddInfo(string message) => AddMessage(message, MessageType.Info);
    public void AddError(string message) => AddMessage(message, MessageType.Error);
    public void AddSuccess(string message) => AddMessage(message, MessageType.Success);
    public void AddWarning(string message) => AddMessage(message, MessageType.Warning);

    // Свойство для доступа к количеству сообщений
    public int MessageCount => messageQueue.Count;
}