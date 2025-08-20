using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// ����������� ���������� ������ � �������������� �����������������
/// </summary>
public class ButtonHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Button Events")]
    public UnityEvent OnClick;
    public UnityEvent OnHoverEnter;
    public UnityEvent OnHoverExit;
    public UnityEvent OnPress;
    public UnityEvent OnRelease;

    [Header("Button Settings")]
    public bool interactable = true;
    public float pressScale = 0.95f;
    public float hoverScale = 1.05f;

    [Header("Visual Feedback")]
    public Image targetImage;
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    public Color pressColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private Vector3 originalScale;
    private Button button;

    /// <summary>
    /// ������������� ��� ������
    /// </summary>
    private void Start()
    {
        InitializeButton();
        originalScale = transform.localScale;

        // ������� ��������� Button ���� �� ����
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(HandleUnityButtonClick);
        }
    }

    /// <summary>
    /// ������������� ������
    /// </summary>
    private void InitializeButton()
    {
        // ������������� ������� Image ���� �� ����������
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        // ������������� ��������� ����
        UpdateButtonAppearance();
    }

    /// <summary>
    /// ���������� ����� ��� Unity Button
    /// </summary>
    private void HandleUnityButtonClick()
    {
        HandleButtonClick();
    }

    /// <summary>
    /// �������� ���������� �����
    /// </summary>
    public void HandleButtonClick()
    {
        if (!interactable) return;

        OnClick.Invoke();
    }

    /// <summary>
    /// ���������� ��������� �������
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable) return;

        transform.localScale = originalScale * hoverScale;
        if (targetImage != null) targetImage.color = hoverColor;
        OnHoverEnter.Invoke();
    }

    /// <summary>
    /// ���������� ����� �������
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!interactable) return;

        transform.localScale = originalScale;
        if (targetImage != null) targetImage.color = normalColor;
        OnHoverExit.Invoke();
    }

    /// <summary>
    /// ���������� ������� ������
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable) return;

        transform.localScale = originalScale * pressScale;
        if (targetImage != null) targetImage.color = pressColor;
        OnPress.Invoke();
    }

    /// <summary>
    /// ���������� ���������� ������
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactable) return;

        transform.localScale = originalScale * hoverScale;
        if (targetImage != null) targetImage.color = hoverColor;
        OnRelease.Invoke();
    }

    /// <summary>
    /// ��������� interactable ���������
    /// </summary>
    public void SetInteractable(bool value)
    {
        interactable = value;
        UpdateButtonAppearance();

        // ������������� � Unity Button ���� �� ����
        if (button != null)
        {
            button.interactable = value;
        }
    }

    /// <summary>
    /// ���������� �������� ���� ������
    /// </summary>
    private void UpdateButtonAppearance()
    {
        if (targetImage != null)
        {
            targetImage.color = interactable ? normalColor : disabledColor;
        }

        transform.localScale = originalScale;
    }

    /// <summary>
    /// ���������� ��� �����������, ��������� �������
    /// </summary>
    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleUnityButtonClick);
        }
    }
}