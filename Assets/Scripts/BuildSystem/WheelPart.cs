using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Part))]
public class WheelPart : MonoBehaviour
{
    [Tooltip("Если true — будет создан WheelCollider при установке")]
    public bool createWheelCollider = true;
    [Tooltip("Radius for the wheel collider")]
    public float wheelRadius = 0.35f;

    WheelCollider wc;

    void Start()
    {
        if (createWheelCollider) CreateWheelColliderAt(transform.position);
    }

    public void CreateWheelColliderAt(Vector3 worldPos)
    {
        GameObject go = new GameObject("WheelCollider_");
        go.transform.SetParent(transform);
        go.transform.position = worldPos;
        go.transform.localRotation = Quaternion.identity;
        wc = go.AddComponent<WheelCollider>();
        wc.radius = wheelRadius;
        // Настройки по умолчанию; настраивайте suspension/damper/force.
    }
}
