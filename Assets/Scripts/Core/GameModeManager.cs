using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    [SerializeField] private GameObject[] robotParts; // Все части робота


    private bool isPlayMode = false;
    private Rigidbody[] rigidbodies;
    private Vector3[] startPositions;
    private Quaternion[] startRotations;

    void Start()
    {
        // Инициализируем массивы
        rigidbodies = new Rigidbody[robotParts.Length];
        startPositions = new Vector3[robotParts.Length];
        startRotations = new Quaternion[robotParts.Length];

        for (int i = 0; i < robotParts.Length; i++)
        {
            rigidbodies[i] = robotParts[i].GetComponent<Rigidbody>();
            startPositions[i] = robotParts[i].transform.position;
            startRotations[i] = robotParts[i].transform.rotation;
        }

        SetEditMode();
    }

    public void ToggleMode()
    {
        if (isPlayMode)
            SetEditMode();
        else
            SetPlayMode();

        isPlayMode = !isPlayMode;
    }

    private void SetEditMode()
    {
        // Выключаем физику
        foreach (var rb in rigidbodies)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }



        // Сбрасываем позиции
        for (int i = 0; i < robotParts.Length; i++)
        {
            robotParts[i].transform.position = startPositions[i];
            robotParts[i].transform.rotation = startRotations[i];
        }
    }

    private void SetPlayMode()
    {


        // Включаем физику
        foreach (var rb in rigidbodies)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }
    }
}
