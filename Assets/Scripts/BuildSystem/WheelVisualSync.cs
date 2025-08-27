using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelVisualSync : MonoBehaviour
{
    public WheelCollider wheelCollider;
    public Transform wheelVisual;

    void Start()
    {
        UpdateWheelPose(); // сразу при старте выставляем позицию визуала
    }

    void Update()
    {
        UpdateWheelPose();
    }

    private void UpdateWheelPose()
    {
        if (wheelCollider == null || wheelVisual == null) return;

        wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
        wheelVisual.position = pos;
        wheelVisual.rotation = rot;
    }
}
