using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsLimiter : MonoBehaviour
{
    [Tooltip("ћаксимальное количество кадров в секунду. ”становите 0 дл€ отключени€ ограничени€ (используетс€ Application.targetFrameRate).")]
    [Range(0, 300)]
    [SerializeField] private int maxFPS = 60;

    private void OnValidate()
    {
        // ѕримен€ем ограничение при изменении значени€ в инспекторе
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
            Application.targetFrameRate = -1; // Ѕез ограничений (по умолчанию)
        }
    }
}
    