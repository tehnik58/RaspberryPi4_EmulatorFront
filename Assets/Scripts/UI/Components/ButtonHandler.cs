using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Расширенный обработчик кнопок с дополнительной функциональностью
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
    /// Инициализация при старте
    /// </summary>
    private void Start()
    {
        InitializeButton();
        originalScale = transform.localScale;

        // Находим компонент Button если он есть
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(HandleUnityButtonClick);
        }
    }

    /// <summary>
    /// Инициализация кнопки
    /// </summary>
    private void InitializeButton()
    {
        // Автоматически находим Image если не установлен
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        // Устанавливаем начальный цвет
        UpdateButtonAppearance();
    }

    /// <summary>
    /// Обработчик клика для Unity Button
    /// </summary>
    private void HandleUnityButtonClick()
    {
        HandleButtonClick();
    }

    /// <summary>
    /// Основной обработчик клика
    /// </summary>
    public void HandleButtonClick()
    {
        if (!interactable) return;

        OnClick.Invoke();
    }

    /// <summary>
    /// Обработчик наведения курсора
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable) return;

        transform.localScale = originalScale * hoverScale;
        if (targetImage != null) targetImage.color = hoverColor;
        OnHoverEnter.Invoke();
    }

    /// <summary>
    /// Обработчик ухода курсора
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!interactable) return;

        transform.localScale = originalScale;
        if (targetImage != null) targetImage.color = normalColor;
        OnHoverExit.Invoke();
    }

    /// <summary>
    /// Обработчик нажатия кнопки
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable) return;

        transform.localScale = originalScale * pressScale;
        if (targetImage != null) targetImage.color = pressColor;
        OnPress.Invoke();
    }

    /// <summary>
    /// Обработчик отпускания кнопки
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactable) return;

        transform.localScale = originalScale * hoverScale;
        if (targetImage != null) targetImage.color = hoverColor;
        OnRelease.Invoke();
    }

    /// <summary>
    /// Установка interactable состояния
    /// </summary>
    public void SetInteractable(bool value)
    {
        interactable = value;
        UpdateButtonAppearance();

        // Синхронизация с Unity Button если он есть
        if (button != null)
        {
            button.interactable = value;
        }
    }

    /// <summary>
    /// Обновление внешнего вида кнопки
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
    /// Вызывается при уничтожении, выполняет очистку
    /// </summary>
    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleUnityButtonClick);
        }
    }
}