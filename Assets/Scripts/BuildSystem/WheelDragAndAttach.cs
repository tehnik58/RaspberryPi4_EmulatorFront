using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))] // Теперь требуем именно BoxCollider
public class WheelDragAndAttach : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float snapRadius = 0.4f;      // Радиус захвата точки
    [SerializeField] private float scrollSensitivity = 1.5f; // Чувствительность прокрутки

    [Header("Ссылки")]
    [SerializeField] private Transform wheelColliderObject;
    [SerializeField, Tooltip("Перетащите сюда пустой объект с вашими точками крепления")]
    private Transform attachmentPointsContainer; // Контейнер с РУЧНЫМИ точками

    private bool isDragging;
    private Transform originalParent;
    private WheelCollider wheelCollider;
    private float currentDragDistance;
    private Vector3 lastValidPosition;

    void Start()
    {
        wheelCollider = wheelColliderObject.GetComponent<WheelCollider>();
        wheelCollider.enabled = false;
    }

    void OnMouseDown()
    {
        originalParent = transform.parent;
        transform.SetParent(null);
        currentDragDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
        isDragging = true;
    }

    void Update()
    {
        if (!isDragging) return;

        // Регулировка глубины колесом мыши
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            currentDragDistance -= scroll * scrollSensitivity;
            currentDragDistance = Mathf.Clamp(currentDragDistance, 0.3f, 20f);
        }
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPos = ray.GetPoint(currentDragDistance);

        // Если есть контейнер с точками - привязываемся к ближайшей
        if (attachmentPointsContainer != null)
        {
            targetPos = SnapToNearestPoint(targetPos);
        }

        lastValidPosition = targetPos;
        transform.position = targetPos;
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        if (attachmentPointsContainer != null && TryAttachToNearestPoint())
        {
            Debug.Log($"Колесо прикреплено к {transform.parent.name}");
        }
        else
        {
            transform.SetParent(originalParent);
            transform.position = lastValidPosition;
        }
    }

    Vector3 SnapToNearestPoint(Vector3 position)
    {
        Transform nearestPoint = FindNearestPoint(position, snapRadius * 2);
        return nearestPoint ? nearestPoint.position : position;
    }

    bool TryAttachToNearestPoint()
    {
        Transform nearestPoint = FindNearestPoint(transform.position, snapRadius);
        if (nearestPoint == null) return false;

        // Сохраняем мировую позицию при смене родителя
        transform.SetParent(nearestPoint, worldPositionStays: true);

        // Авто-коррекция поворота (ось колеса перпендикулярна поверхности)
        transform.rotation = Quaternion.LookRotation(
            Vector3.Cross(nearestPoint.forward, Vector3.up),
            nearestPoint.forward
        );

        wheelCollider.enabled = true;
        return true;
    }

    Transform FindNearestPoint(Vector3 position, float maxDistance)
    {
        Transform nearestPoint = null;
        float minDistance = maxDistance;

        foreach (Transform point in attachmentPointsContainer)
        {
            float distance = Vector3.Distance(position, point.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPoint = point;
            }
        }
        return nearestPoint;
    }
}
