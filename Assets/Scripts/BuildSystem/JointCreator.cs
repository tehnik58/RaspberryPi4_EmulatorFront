using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JointCreator
{
    // —оздаЄт FixedJoint между host и target на позиции hostAttachPoint
    public static FixedJoint CreateFixedJoint(Rigidbody host, Rigidbody target, Vector3 anchorWorldPos)
    {
        if (host == null || target == null) return null;
        var joint = host.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = target;
        // смещение анкера в локальных координатах host
        joint.anchor = host.transform.InverseTransformPoint(anchorWorldPos);
        // connectedAnchor Ч в локальных координатах target
        joint.connectedAnchor = target.transform.InverseTransformPoint(anchorWorldPos);
        // настройка стабильности
        joint.breakForce = Mathf.Infinity;
        joint.breakTorque = Mathf.Infinity;
        joint.enableCollision = false;
        return joint;
    }

    // —оздаЄт ConfigurableJoint между host и target, закреплЄнный в заданной позиции
    public static ConfigurableJoint CreateConfigurableJoint(Rigidbody host, Rigidbody target, Vector3 anchorWorldPos)
    {
        if (host == null || target == null) return null;
        var joint = host.gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = target;
        joint.anchor = host.transform.InverseTransformPoint(anchorWorldPos);
        joint.connectedAnchor = target.transform.InverseTransformPoint(anchorWorldPos);
        // по умолчанию фиксируем линейное движение
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        // angular: ограничим немного
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;
        SoftJointLimit lim = new SoftJointLimit { limit = 5f };
        joint.lowAngularXLimit = lim;
        joint.highAngularXLimit = lim;
        joint.angularYLimit = lim;
        joint.angularZLimit = lim;
        // ƒрайвы (пример значений, подбирайте под ваш проект)
        JointDrive drive = new JointDrive { positionSpring = 1000f, positionDamper = 50f, maximumForce = 10000f };
        joint.angularXDrive = drive;
        joint.angularYZDrive = drive;
        joint.enableCollision = false;
        return joint;
    }
}
