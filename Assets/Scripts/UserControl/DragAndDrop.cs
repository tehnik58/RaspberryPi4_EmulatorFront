using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DragAndDrop : MonoBehaviour
{
    private Camera cam;
    private bool isDragging = false;
    private float dragDistance;

    public float scrollSpeed = 2f;
    public float rotateSpeed = 90f; // визуальный поворот колеса до прикрепления

    private void Start()
    {
        cam = Camera.main;
    }

    private void OnMouseDown()
    {
        isDragging = true;
        dragDistance = Vector3.Distance(transform.position, cam.transform.position);
    }

    private void OnMouseUp()
    {
        isDragging = false;

        // SphereCast для нахождения RobotBody
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.SphereCast(ray, 0.1f, out RaycastHit hit, 100f))
        {
            RobotBody rbTarget = hit.collider.GetComponent<RobotBody>();
            if (rbTarget != null)
            {
                rbTarget.AttachWheel(gameObject, transform.position);
            }
        }
    }

    private void Update()
    {
        if (!isDragging) return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = dragDistance;
        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);
        transform.position = worldPos;

        // Приближение/отдаление колесом мыши
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            dragDistance -= scroll * scrollSpeed;
            dragDistance = Mathf.Clamp(dragDistance, 1f, 20f);
        }

        // Визуальный поворот колеса до прикрепления (не влияет на WheelCollider)
        if (Input.GetKey(KeyCode.Q))
            transform.Rotate(Vector3.up, -rotateSpeed * Time.deltaTime, Space.World);
        if (Input.GetKey(KeyCode.E))
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }
}
