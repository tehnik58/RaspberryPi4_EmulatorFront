using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Part : MonoBehaviour
{
    [Tooltip("—сылка на ScriptableObject с данными детали")]
    public PartDefinition definition;

    [Tooltip("—писок точек креплени€ на префабе (заполнить в инспекторе)")]
    public AttachPoint[] attachPoints;

    [HideInInspector] public Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (definition != null) rb.mass = Mathf.Max(0.0001f, definition.mass);
        if (attachPoints == null || attachPoints.Length == 0)
            attachPoints = GetComponentsInChildren<AttachPoint>(true);
    }
}
