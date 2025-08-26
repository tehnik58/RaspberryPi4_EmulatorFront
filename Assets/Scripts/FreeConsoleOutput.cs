using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FreeConsoleOutput : MonoBehaviour
{
    [SerializeField]private TMP_Text scrollRect;
    [SerializeField]private GameObject consoleOutput;
    private void Start()
    {
        // Безопасная подписка на события
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Subscribe<ConsoleOutput.RawMessageEvent>(SetTextOnConsole);
        }
        else
        {
            StartCoroutine(SubscribeWhenManagerReady());
        }
    }

    private IEnumerator SubscribeWhenManagerReady()
    {
        while (EventManager.Instance == null)
        {
            yield return null;
        }
        
        EventManager.Instance.Subscribe<ConsoleOutput.RawMessageEvent>(SetTextOnConsole);
        consoleOutput.SetActive(false);
    }

    private void OnDestroy()
    {
        // Важно отписаться при отключении объекта
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe<ConsoleOutput.RawMessageEvent>(SetTextOnConsole);
        }
    }
    
    private void SetTextOnConsole(ConsoleOutput.RawMessageEvent rawMessageEvent)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        Color color = ConsoleOutput.GetColorForType(rawMessageEvent.Type);
        string coloredMessage = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>[{timestamp}] {rawMessageEvent.Message}</color>\n";
        scrollRect.text += coloredMessage;
    }
}
