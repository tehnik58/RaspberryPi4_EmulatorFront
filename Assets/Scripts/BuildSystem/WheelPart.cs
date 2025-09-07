using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Part))]
public class WheelPart : MonoBehaviour
{
    [Tooltip("���� true � ����� ������ WheelCollider ��� ���������")]
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
        // ��������� �� ���������; ������������ suspension/damper/force.
    }
}
