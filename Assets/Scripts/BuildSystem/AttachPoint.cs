using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class AttachPoint : MonoBehaviour
{
    [Tooltip("“ип сокета Ч используетс€ дл€ проверки совместимости")]
    public string socketType = "default";
    [Tooltip("≈сли true Ч этот attach €вл€етс€ вход€щим (accept) или исход€щим")]
    public bool isConnector = true;

    // √лобальна€ позици€ и ориентаци€ удобны дл€ проверки
    public Vector3 WorldPosition => transform.position;
    public Quaternion WorldRotation => transform.rotation;
}
