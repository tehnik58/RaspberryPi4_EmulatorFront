using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))] // ������ ������� ������ BoxCollider
public class WheelDragAndAttach : MonoBehaviour
{
    [Header("���������")]
    [SerializeField] private float snapRadius = 0.4f;      // ������ ������� �����
    [SerializeField] private float scrollSensitivity = 1.5f; // ���������������� ���������

    [Header("������")]
    [SerializeField] private Transform wheelColliderObject;
    [SerializeField, Tooltip("���������� ���� ������ ������ � ������ ������� ���������")]
    private Transform attachmentPointsContainer; // ��������� � ������� �������

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

        // ����������� ������� ������� ����
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

        // ���� ���� ��������� � ������� - ������������� � ���������
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
            Debug.Log($"������ ����������� � {transform.parent.name}");
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

        // ��������� ������� ������� ��� ����� ��������
        transform.SetParent(nearestPoint, worldPositionStays: true);

        // ����-��������� �������� (��� ������ ��������������� �����������)
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
