using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class AttachPoint : MonoBehaviour
{
    [Tooltip("��� ������ � ������������ ��� �������� �������������")]
    public string socketType = "default";
    [Tooltip("���� true � ���� attach �������� �������� (accept) ��� ���������")]
    public bool isConnector = true;

    // ���������� ������� � ���������� ������ ��� ��������
    public Vector3 WorldPosition => transform.position;
    public Quaternion WorldRotation => transform.rotation;
}
