using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Part : MonoBehaviour
{
    [Tooltip("ScriptableObject с данными детали")]
    public PartDefinition definition;

    [Tooltip("Точки крепления на префабе")]
    public AttachPoint[] attachPoints;

    [HideInInspector] public Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>(); // теперь НЕ обязателен
        if (definition != null && rb != null)
            rb.mass = Mathf.Max(0.0001f, definition.mass);

        if (attachPoints == null || attachPoints.Length == 0)
            attachPoints = GetComponentsInChildren<AttachPoint>(true);
    }
}
