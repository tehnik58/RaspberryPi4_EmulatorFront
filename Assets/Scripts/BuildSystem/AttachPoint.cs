using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class AttachPoint : MonoBehaviour
{
    [Tooltip("Тип сокета. Для колёс укажи одинаковый, например \"wheel\".")]
    public string socketType = "default";

    [Tooltip("Добавить маленький триггер-коллайдер для поиска соседних точек.")]
    public bool ensureTriggerCollider = true;

    [Tooltip("Радиус служебного триггера для детекции (м).")]
    public float triggerRadius = 0.05f;

    public Vector3 WorldPosition => transform.position;

    void OnValidate()
    {
        if (!ensureTriggerCollider) return;
        var col = GetComponent<SphereCollider>();
        if (col == null) col = gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = Mathf.Max(0.001f, triggerRadius);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * (triggerRadius * 1.5f),
            $"Attach[{socketType}]");
#endif
    }
}
