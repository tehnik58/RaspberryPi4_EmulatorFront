using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ChassisPart : MonoBehaviour
{
    [Tooltip("Список визуальных колес для синхронизации с WheelCollider")]
    public List<Transform> wheelVisuals = new List<Transform>();

    [Tooltip("Список WheelCollider для управления колесами")]
    public List<WheelCollider> wheelColliders = new List<WheelCollider>();

    [Tooltip("Радиус по умолчанию, если не удалось вычислить из меша")]
    public float defaultWheelRadius = 0.35f;

    [Tooltip("Ширина по умолчанию, если не удалось вычислить из меша")]
    public float defaultWheelWidth = 0.2f;

    /// <summary>
    /// Добавляет колесо к шасси и создаёт WheelCollider
    /// </summary>
    public void AddWheel(Transform wheelVisual)
    {
        if (wheelVisual == null)
        {
            Debug.LogWarning("WheelVisual is null");
            return;
        }

        // Создаём holder для WheelCollider
        GameObject holder = new GameObject($"WheelHolder_{wheelVisual.name}");
        holder.transform.SetParent(transform);
        holder.transform.position = wheelVisual.position;
        holder.transform.rotation = wheelVisual.rotation;

        // Создаём WheelCollider
        GameObject wcGO = new GameObject($"WheelCollider_{wheelVisual.name}");
        wcGO.transform.SetParent(holder.transform);
        wcGO.transform.localPosition = Vector3.zero;
        wcGO.transform.localRotation = Quaternion.identity;

        WheelCollider wc = wcGO.AddComponent<WheelCollider>();

        // Вычисляем радиус и ширину из меша
        float radius = ComputeWheelRadiusFromMesh(wheelVisual);
        wc.radius = radius > 0.01f ? radius : defaultWheelRadius;

        float width = ComputeWheelWidthFromMesh(wheelVisual);
        Vector3 scale = wcGO.transform.localScale;
        scale.x = width > 0.01f ? width : defaultWheelWidth;
        wcGO.transform.localScale = scale;

        // Настройки подвески и физики
        wc.suspensionDistance = 0.2f;
        wc.mass = 20f;

        JointSpring spring = wc.suspensionSpring;
        spring.spring = 20000f;
        spring.damper = 4500f;
        wc.suspensionSpring = spring;

        WheelFrictionCurve fForward = wc.forwardFriction;
        fForward.stiffness = 1.5f;
        wc.forwardFriction = fForward;

        WheelFrictionCurve fSide = wc.sidewaysFriction;
        fSide.stiffness = 2f;
        wc.sidewaysFriction = fSide;

        // Сохраняем ссылки
        wheelColliders.Add(wc);
        wheelVisuals.Add(wheelVisual);

        Debug.Log($"[Chassis] WheelCollider created for '{wheelVisual.name}' (radius={wc.radius:F3}, width={scale.x:F3})");
    }

    /// <summary>
    /// Радиус колеса = половина наибольшего размера bounding box
    /// </summary>
    float ComputeWheelRadiusFromMesh(Transform wheelVisual)
    {
        var rend = wheelVisual.GetComponentInChildren<MeshRenderer>();
        if (rend == null) return defaultWheelRadius;

        Bounds b = rend.bounds;
        float radius = Mathf.Max(b.size.x, b.size.y, b.size.z) * 0.5f;
        return radius;
    }

    /// <summary>
    /// Ширина колеса = минимальный размер bounding box
    /// </summary>
    float ComputeWheelWidthFromMesh(Transform wheelVisual)
    {
        var rend = wheelVisual.GetComponentInChildren<MeshRenderer>();
        if (rend == null) return defaultWheelWidth;

        Bounds b = rend.bounds;
        float width = Mathf.Min(b.size.x, b.size.y, b.size.z);
        return width;
    }

    /// <summary>
    /// Синхронизируем визуал с WheelCollider
    /// </summary>
    void Update()
    {
        for (int i = 0; i < wheelColliders.Count && i < wheelVisuals.Count; i++)
        {
            WheelCollider wc = wheelColliders[i];
            Transform vis = wheelVisuals[i];
            if (wc == null || vis == null) continue;

            wc.GetWorldPose(out Vector3 pos, out Quaternion rot);
            vis.position = pos;
            vis.rotation = rot;
        }
    }

    /// <summary>
    /// Очистка всех колес
    /// </summary>
    public void ClearWheels()
    {
        foreach (var wc in wheelColliders)
        {
            if (wc != null) Destroy(wc.gameObject);
        }
        wheelColliders.Clear();
        wheelVisuals.Clear();
    }
}