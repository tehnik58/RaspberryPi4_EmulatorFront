using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachDebugger : MonoBehaviour
{
    public float radius = 0.2f; // радиус поиска, должен совпадать с attachDetectionRadius в BuilderManager

    void Update()
    {
        var points = FindObjectsOfType<AttachPoint>();
        foreach (var p in points)
        {
            Collider[] hits = Physics.OverlapSphere(p.WorldPosition, radius);
            foreach (var h in hits)
            {
                var other = h.GetComponentInParent<AttachPoint>();
                if (other != null && other != p)
                {
                    Debug.Log($"[AttachDebugger] {p.name} ({p.socketType}) ↔ {other.name} ({other.socketType})");
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        var points = FindObjectsOfType<AttachPoint>();
        Gizmos.color = Color.yellow;
        foreach (var p in points)
        {
            Gizmos.DrawSphere(p.WorldPosition, 0.05f);
        }
    }
}
