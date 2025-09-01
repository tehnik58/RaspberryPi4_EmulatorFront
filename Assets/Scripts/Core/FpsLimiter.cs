using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsLimiter : MonoBehaviour
{
    [Tooltip("������������ ���������� ������ � �������. ���������� 0 ��� ���������� ����������� (������������ Application.targetFrameRate).")]
    [Range(0, 300)]
    [SerializeField] private int maxFPS = 60;

    private void OnValidate()
    {
        // ��������� ����������� ��� ��������� �������� � ����������
        ApplyFrameRate();
    }

    private void Start()
    {
        ApplyFrameRate();
    }

    private void ApplyFrameRate()
    {
        if (maxFPS > 0)
        {
            Application.targetFrameRate = maxFPS;
        }
        else
        {
            Application.targetFrameRate = -1; // ��� ����������� (�� ���������)
        }
    }
}
    