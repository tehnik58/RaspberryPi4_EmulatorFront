using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelAttacher : MonoBehaviour
{
    [SerializeField] private float wheelOffset = 0.1f; // Небольшой отступ от поверхности (чтобы колесо не "проваливалось" внутрь)

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Проверяем, кликнули ли именно по корпусу
                if (hit.collider.gameObject == this.gameObject)
                {
                    AttachWheelTo(hit);
                }
            }
        }
    }

    void AttachWheelTo(RaycastHit hit)
    {
        // Находим колесо по тегу
        GameObject wheel = GameObject.FindWithTag("Module");
        if (wheel == null)
        {
            Debug.LogError("Колесо не найдено! Назначьте тег 'Module' на объект колеса.");
            return;
        }

        // 1. Перемещаем колесо в точку клика + небольшой отступ НАРУЖУ
        Vector3 offsetDirection = hit.normal * wheelOffset;
        wheel.transform.position = hit.point + offsetDirection;

        // 2. Делаем колесо дочерним объектом корпуса
        wheel.transform.SetParent(this.transform);

        // 3. Автоматически корректируем поворот колеса:
        //    - Ось колеса (ось вращения) должна быть перпендикулярна поверхности корпуса
        //    - "Верх" колеса направлен вверх по миру (Vector3.up)
        wheel.transform.rotation = Quaternion.LookRotation(
            Vector3.Cross(hit.normal, Vector3.up), // Направление "вправо" для колеса
            hit.normal                            // Нормаль поверхности = направление "наружу"
        );
    }
}
