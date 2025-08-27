using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotBody : MonoBehaviour
{
    public Transform[] mountPoints; // ������� ������������� ����� ��������� �����

    private WheelCollider[] wheels;

    void Start()
    {
        RefreshWheels();
    }

    void FixedUpdate()
    {
        float motor = Input.GetAxis("Vertical") * 1500f;
        float steer = Input.GetAxis("Horizontal") * 30f;

        foreach (var wc in wheels)
        {
            wc.motorTorque = motor;

            if (wc.transform.localPosition.z > 0)
                wc.steerAngle = steer;
        }
    }

    public void AttachWheel(GameObject wheel, Vector3 desiredPos)
    {
        // ���� ��������� MountPoint
        Transform nearest = null;
        float minDist = Mathf.Infinity;
        foreach (var mp in mountPoints)
        {
            float d = Vector3.Distance(mp.position, desiredPos);
            if (d < minDist)
            {
                minDist = d;
                nearest = mp;
            }
        }

        if (nearest == null) return;

        // ������ ������ �������� MountPoint
        wheel.transform.SetParent(nearest);

        // ������ �������, rotation � scale �� �������
        wheel.transform.position = nearest.position;

        RefreshWheels();
    }

    private void RefreshWheels()
    {
        wheels = GetComponentsInChildren<WheelCollider>();
    }
}

