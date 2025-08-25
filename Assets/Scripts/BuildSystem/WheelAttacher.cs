using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelAttacher : MonoBehaviour
{
    [SerializeField] private float wheelOffset = 0.1f; // ��������� ������ �� ����������� (����� ������ �� "�������������" ������)

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // ���������, �������� �� ������ �� �������
                if (hit.collider.gameObject == this.gameObject)
                {
                    AttachWheelTo(hit);
                }
            }
        }
    }

    void AttachWheelTo(RaycastHit hit)
    {
        // ������� ������ �� ����
        GameObject wheel = GameObject.FindWithTag("Module");
        if (wheel == null)
        {
            Debug.LogError("������ �� �������! ��������� ��� 'Module' �� ������ ������.");
            return;
        }

        // 1. ���������� ������ � ����� ����� + ��������� ������ ������
        Vector3 offsetDirection = hit.normal * wheelOffset;
        wheel.transform.position = hit.point + offsetDirection;

        // 2. ������ ������ �������� �������� �������
        wheel.transform.SetParent(this.transform);

        // 3. ������������� ������������ ������� ������:
        //    - ��� ������ (��� ��������) ������ ���� ��������������� ����������� �������
        //    - "����" ������ ��������� ����� �� ���� (Vector3.up)
        wheel.transform.rotation = Quaternion.LookRotation(
            Vector3.Cross(hit.normal, Vector3.up), // ����������� "������" ��� ������
            hit.normal                            // ������� ����������� = ����������� "������"
        );
    }
}
