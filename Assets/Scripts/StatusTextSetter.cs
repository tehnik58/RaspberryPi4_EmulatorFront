using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct StatusConnection
{
    [SerializeField] private string statusText;
    [SerializeField] private Color statusColor;
    
    public string StatusText => statusText;
    public Color StatusColor => statusColor;
}

public class StatusTextSetter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMesh;
    [SerializeField] private Image _image;
    
    [SerializeField] private StatusConnection activeStatus;
    [SerializeField] private StatusConnection deActiveStatus;

    private void OnEnable()
    {
        // Безопасная подписка на события
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Subscribe<ConnectionStatusEvent>(SetUIStatus);
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
        
        EventManager.Instance.Subscribe<ConnectionStatusEvent>(SetUIStatus);
    }

    private void OnDisable()
    {
        // Важно отписаться при отключении объекта
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe<ConnectionStatusEvent>(SetUIStatus);
        }
    }

    private void SetUIStatus(ConnectionStatusEvent statusEvent)
    {
        if (_textMesh == null || _image == null)
        {
            Debug.LogWarning("Components not initialized in StatusTextSetter");
            return;
        }
        
        if (statusEvent.IsConnected)
        {
            _textMesh.text = activeStatus.StatusText;
            _image.color = activeStatus.StatusColor;
        }
        else
        {
            _textMesh.text = deActiveStatus.StatusText;
            _image.color = deActiveStatus.StatusColor;
        }
    }
}