using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Vehicle/PartDefinition")]
public class PartDefinition : ScriptableObject
{
    [Tooltip("���������� ID ������")]
    public string partId;

    [Tooltip("������ ��� ���������������")]
    public GameObject prefab;

    [Tooltip("������ ������ � ������� (�������������)")]
    public Vector3Int sizeInCells = Vector3Int.one;

    [Tooltip("����� (������ ��� ������� � Rigidbody � ������, ����� � �.�.)")]
    public float mass = 1f;

    [Tooltip("��� ������ �� ��������� (�����������)")]
    public string defaultSocketType = "default";

    [Header("��� ������")]
    [Tooltip("���� ��� ���������� ������ (��� Rigidbody/WheelCollider) � ������.")]
    public bool isWheelVisual = false;
}
