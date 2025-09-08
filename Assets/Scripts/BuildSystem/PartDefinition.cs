using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Vehicle/PartDefinition")]
public class PartDefinition : ScriptableObject
{
    [Tooltip("Уникальный ID детали")]
    public string partId;

    [Tooltip("Префаб для инстанцирования")]
    public GameObject prefab;

    [Tooltip("Размер детали в клетках (необязательно)")]
    public Vector3Int sizeInCells = Vector3Int.one;

    [Tooltip("Масса (только для деталей с Rigidbody — корпус, блоки и т.п.)")]
    public float mass = 1f;

    [Tooltip("Тип сокета по умолчанию (опционально)")]
    public string defaultSocketType = "default";

    [Header("Тип детали")]
    [Tooltip("Если это ВИЗУАЛЬНОЕ колесо (без Rigidbody/WheelCollider) — включи.")]
    public bool isWheelVisual = false;
}
