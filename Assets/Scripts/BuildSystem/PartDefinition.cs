using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Vehicle/PartDefinition")]
public class PartDefinition : ScriptableObject
{
    [Tooltip("Уникальный ID детали (строка)")]
    public string partId;
    [Tooltip("Префаб для инстанцирования")]
    public GameObject prefab;
    [Tooltip("Размер детали в клетках (целые значения) — используется по желанию")]
    public Vector3Int sizeInCells = Vector3Int.one;
    [Tooltip("Масса для Rigidbody при инстанцировании")]
    public float mass = 1f;
    [Tooltip("Тип сокета по умолчанию (можно оставить пустым)")]
    public string defaultSocketType = "default";
}
