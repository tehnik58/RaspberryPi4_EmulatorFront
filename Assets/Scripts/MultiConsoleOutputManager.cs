using System.Collections.Generic;
using UnityEngine;

public class MultiConsoleOutputManager : MonoBehaviour
{
    [System.Serializable]
    public class ConsoleInstance
    {
        public ConsoleOutput consoleOutput;
        public bool isActive = true;
    }

    [SerializeField] private List<ConsoleInstance> consoles = new List<ConsoleInstance>();
    [SerializeField] private int activeConsoleIndex = 0;

    private WebSocketManager webSocketManager;
    private MessageClassifier messageClassifier;
    private bool isInitialized = false;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (isInitialized) return;

        // Получаем зависимости
        webSocketManager = GetComponent<WebSocketManager>();
        messageClassifier = GetComponent<MessageClassifier>();

        // Подписываемся на события
        if (webSocketManager != null)
        {
            webSocketManager.OnMessageReceived += ProcessMessage;
        }

        // Инициализируем консоли
        foreach (var consoleInstance in consoles)
        {
            if (consoleInstance.consoleOutput != null)
            {
                consoleInstance.consoleOutput.Clear();
            }
        }

        UpdateConsoleVisibility();
        isInitialized = true;
    }

    private void ProcessMessage(string message)
    {
        // Если есть классификатор, используем его
        if (messageClassifier != null)
        {
            var classifiedMessage = messageClassifier.ClassifyAndProcessMessage(message);
            DistributeToConsoles(classifiedMessage);
        }
        else
        {
            // Иначе отправляем напрямую
            DistributeToConsoles(message, MessageClassifier.MessageType.Info);
        }
    }

    private void DistributeToConsoles(MessageClassifier.ClassifiedMessage message)
    {
        foreach (var consoleInstance in consoles)
        {
            if (consoleInstance.isActive && consoleInstance.consoleOutput != null)
            {
                consoleInstance.consoleOutput.AddMessage(
                    message.rawMessage,
                    message.type
                );
            }
        }
    }

    private void DistributeToConsoles(string message, MessageClassifier.MessageType type)
    {
        foreach (var consoleInstance in consoles)
        {
            if (consoleInstance.isActive && consoleInstance.consoleOutput != null)
            {
                consoleInstance.consoleOutput.AddMessage(message, type);
            }
        }
    }

    public void SwitchToConsole(int index)
    {
        if (index >= 0 && index < consoles.Count)
        {
            activeConsoleIndex = index;
            UpdateConsoleVisibility();
        }
    }

    private void UpdateConsoleVisibility()
    {
        for (int i = 0; i < consoles.Count; i++)
        {
            if (consoles[i].consoleOutput != null)
            {
                consoles[i].consoleOutput.gameObject.SetActive(i == activeConsoleIndex);
            }
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        if (webSocketManager != null)
        {
            webSocketManager.OnMessageReceived -= ProcessMessage;
        }
    }
}