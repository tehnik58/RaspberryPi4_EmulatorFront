using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JointCreator
{
    // ������ FixedJoint ����� host � target �� ������� hostAttachPoint
    public static FixedJoint CreateFixedJoint(Rigidbody host, Rigidbody target, Vector3 anchorWorldPos)
    {
        if (host == null || target == null) return null;
        var joint = host.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = target;
        // �������� ������ � ��������� ����������� host
        joint.anchor = host.transform.InverseTransformPoint(anchorWorldPos);
        // connectedAnchor � � ��������� ����������� target
        joint.connectedAnchor = target.transform.InverseTransformPoint(anchorWorldPos);
        // ��������� ������������
        joint.breakForce = Mathf.Infinity;
        joint.breakTorque = Mathf.Infinity;
        joint.enableCollision = false;
        return joint;
    }

    // ������ ConfigurableJoint ����� host � target, ����������� � �������� �������
    public static ConfigurableJoint CreateConfigurableJoint(Rigidbody host, Rigidbody target, Vector3 anchorWorldPos)
    {
        if (host == null || target == null) return null;
        var joint = host.gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = target;
        joint.anchor = host.transform.InverseTransformPoint(anchorWorldPos);
        joint.connectedAnchor = target.transform.InverseTransformPoint(anchorWorldPos);
        // �� ��������� ��������� �������� ��������
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        // angular: ��������� �������
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;
        SoftJointLimit lim = new SoftJointLimit { limit = 5f };
        joint.lowAngularXLimit = lim;
        joint.highAngularXLimit = lim;
        joint.angularYLimit = lim;
        joint.angularZLimit = lim;
        // ������ (������ ��������, ���������� ��� ��� ������)
        JointDrive drive = new JointDrive { positionSpring = 1000f, positionDamper = 50f, maximumForce = 10000f };
        joint.angularXDrive = drive;
        joint.angularYZDrive = drive;
        joint.enableCollision = false;
        return joint;
    }
}
