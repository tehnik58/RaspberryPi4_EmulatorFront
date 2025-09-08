using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector3 gridOrigin = Vector3.zero;
    public float cellSize = 0.25f;

    [Header("Parts")]
    public PartDefinition[] availableParts;

    [Header("Preview")]
    public GameObject ghostPrefab;
    public Camera builderCamera;

    [Header("Placement")]
    public LayerMask placementRaycastMask = ~0;
    [Tooltip("Слои, по которым ищем соседние AttachPoint (включи слой Parts)")]
    public LayerMask attachDetectionMask = ~0;
    public float attachDetectionRadius = 0.06f;

    [Header("Height control")]
    public int currentHeightOffset = 0;
    public int minHeightOffset = -10;
    public int maxHeightOffset = 50;
    public bool showHeightUI = true;

    GameObject currentGhost;
    int currentPartIndex = 0;
    List<GameObject> placedParts = new List<GameObject>();
    Vector3 lastGhostPosition = Vector3.zero;

    void Start()
    {
        if (builderCamera == null)
        {
            builderCamera = Camera.main;
            if (builderCamera == null)
            {
                var cams = FindObjectsOfType<Camera>();
                if (cams.Length > 0) builderCamera = cams[0];
            }
        }
        if (availableParts == null || availableParts.Length == 0)
            Debug.LogWarning("[BuilderManager] availableParts is empty");

        SpawnGhost();
    }

    void Update()
    {
        if (builderCamera == null || availableParts == null || availableParts.Length == 0) return;
        HandleQuickKeys();
        UpdateGhostPositionAndPlacement();
        HandleHeightKeys();
    }

    void HandleQuickKeys()
    {
        for (int i = 0; i < availableParts.Length && i < 9; i++)
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i))) SetCurrentPartIndex(i);

        if (Input.GetKeyDown(KeyCode.R) && currentGhost != null)
            currentGhost.transform.Rotate(Vector3.up, 90f, Space.World);

        if (Input.GetMouseButtonDown(0)) PlaceAtMouse();
    }

    void HandleHeightKeys()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            currentHeightOffset = Mathf.Min(maxHeightOffset, currentHeightOffset + 1);
            ApplyHeightOffsetToGhost();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            currentHeightOffset = Mathf.Max(minHeightOffset, currentHeightOffset - 1);
            ApplyHeightOffsetToGhost();
        }
    }

    void UpdateGhostPositionAndPlacement()
    {
        Ray ray = builderCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, placementRaycastMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 snapped = SnapToGrid(hit.point) + Vector3.up * (currentHeightOffset * cellSize);
            lastGhostPosition = snapped;
            if (currentGhost != null) currentGhost.transform.position = snapped;

            if (currentGhost != null)
            {
                bool colliding = CheckCollisionAt(currentGhost);
                SetGhostValid(!colliding);
            }
        }
        else if (currentGhost != null)
        {
            Vector3 pos = lastGhostPosition;
            pos.y = gridOrigin.y + Mathf.Round((currentGhost.transform.position.y - gridOrigin.y) / cellSize) * cellSize;
            pos += Vector3.up * (currentHeightOffset * cellSize);
            currentGhost.transform.position = pos;
            lastGhostPosition = currentGhost.transform.position;
        }
    }

    public void SetCurrentPartIndex(int idx)
    {
        if (availableParts == null || availableParts.Length == 0) return;
        currentPartIndex = Mathf.Clamp(idx, 0, availableParts.Length - 1);
        if (currentGhost != null) Destroy(currentGhost);
        SpawnGhost();
    }

    void SpawnGhost()
    {
        if (availableParts == null || availableParts.Length == 0) return;
        var def = availableParts[currentPartIndex];
        if (def == null || def.prefab == null)
        {
            Debug.LogWarning("[BuilderManager] PartDefinition or prefab is null");
            return;
        }

        GameObject source = ghostPrefab != null ? ghostPrefab : def.prefab;
        currentGhost = Instantiate(source);
        currentGhost.name = "GHOST_" + def.partId;

        foreach (var rb in currentGhost.GetComponentsInChildren<Rigidbody>()) { rb.isKinematic = true; rb.useGravity = false; }
        foreach (var col in currentGhost.GetComponentsInChildren<Collider>()) col.isTrigger = true;

        foreach (var rend in currentGhost.GetComponentsInChildren<Renderer>())
        {
            if (!rend) continue;
            var mat = rend.material;
            MakeMaterialTransparent(mat, 0.6f);
        }

        currentGhost.transform.position = gridOrigin + Vector3.up * (1f * cellSize);
        lastGhostPosition = currentGhost.transform.position;
    }

    void MakeMaterialTransparent(Material mat, float alpha)
    {
        if (mat == null) return;
        if (mat.HasProperty("_Mode")) mat.SetFloat("_Mode", 3f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        if (mat.HasProperty("_Color")) { var c = mat.color; c.a = alpha; mat.color = c; }
    }

    bool CheckCollisionAt(GameObject ghost)
    {
        foreach (var col in ghost.GetComponentsInChildren<Collider>())
        {
            if (!col) continue;
            if (col is BoxCollider box)
            {
                Vector3 worldCenter = box.transform.TransformPoint(box.center);
                Vector3 worldHalf = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale);
                Collider[] hits = Physics.OverlapBox(worldCenter, worldHalf, box.transform.rotation, placementRaycastMask, QueryTriggerInteraction.Ignore);
                foreach (var h in hits) if (!h.transform.IsChildOf(ghost.transform)) return true;
            }
            else if (col is SphereCollider sph)
            {
                Vector3 worldCenter = sph.transform.TransformPoint(sph.center);
                float worldRadius = sph.radius * Mathf.Max(sph.transform.lossyScale.x, sph.transform.lossyScale.y, sph.transform.lossyScale.z);
                Collider[] hits = Physics.OverlapSphere(worldCenter, worldRadius, placementRaycastMask, QueryTriggerInteraction.Ignore);
                foreach (var h in hits) if (!h.transform.IsChildOf(ghost.transform)) return true;
            }
            else
            {
                var b = col.bounds;
                Collider[] hits = Physics.OverlapBox(b.center, b.extents, col.transform.rotation, placementRaycastMask, QueryTriggerInteraction.Ignore);
                foreach (var h in hits) if (!h.transform.IsChildOf(ghost.transform)) return true;
            }
        }
        return false;
    }

    void SetGhostValid(bool ok)
    {
        if (currentGhost == null) return;
        foreach (var rend in currentGhost.GetComponentsInChildren<Renderer>())
        {
            if (!rend || !rend.material.HasProperty("_Color")) continue;
            Color c = rend.material.color; c.r = ok ? 0f : 1f; c.g = ok ? 1f : 0f; rend.material.color = c;
        }
    }

    Vector3 SnapToGrid(Vector3 worldPos)
    {
        Vector3 local = (worldPos - gridOrigin) / cellSize;
        local.x = Mathf.Round(local.x);
        local.y = Mathf.Round(local.y);
        local.z = Mathf.Round(local.z);
        return gridOrigin + local * cellSize;
    }

    public void PlaceCurrentAt(Vector3 worldPos, Quaternion rot)
    {
        var def = availableParts[currentPartIndex];
        if (def == null || def.prefab == null) return;

        GameObject go = Instantiate(def.prefab, worldPos, rot);
        go.name = def.partId + "_" + placedParts.Count;

        // Part-компонент
        Part partComp = go.GetComponent<Part>() ?? go.AddComponent<Part>();
        partComp.definition = def;

        // Rigidbody добавляем ТОЛЬКО если это не визуальное колесо
        if (!def.isWheelVisual)
        {
            if (go.GetComponent<Rigidbody>() == null) go.AddComponent<Rigidbody>();
            partComp.rb = go.GetComponent<Rigidbody>();
            partComp.rb.mass = Mathf.Max(0.0001f, def.mass);
            partComp.rb.isKinematic = true;
            partComp.rb.useGravity = false;
        }
        else
        {
            // На визуальном колесе не должно быть Rigidbody
            var rb = go.GetComponent<Rigidbody>();
            if (rb) Destroy(rb);
        }

        CreateAttachJointsOrSnap(partComp);
        placedParts.Add(go);
        Destroy(currentGhost);
        SpawnGhost();
    }

    public void PlaceAtMouse()
    {
        Ray ray = builderCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, placementRaycastMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 snapped = SnapToGrid(hit.point) + Vector3.up * (currentHeightOffset * cellSize);
            bool colliding = currentGhost != null && CheckCollisionAt(currentGhost);
            if (!colliding) PlaceCurrentAt(snapped, currentGhost != null ? currentGhost.transform.rotation : Quaternion.identity);
        }
    }

    // === ГЛАВНОЕ: крепление. Для обычных деталей — FixedJoint, для колеса — снап + WheelCollider на корпусе. ===
    public void CreateAttachJointsOrSnap(Part newPart)
    {
        if (newPart == null) return;

        var points = newPart.GetComponentsInChildren<AttachPoint>(true);
        foreach (var p in points)
        {
            Collider[] hits = Physics.OverlapSphere(p.WorldPosition, attachDetectionRadius, attachDetectionMask, QueryTriggerInteraction.Collide);
            foreach (var h in hits)
            {
                var otherAP = h.GetComponentInParent<AttachPoint>();
                if (otherAP == null) continue;
                if (otherAP.transform.IsChildOf(newPart.transform)) continue;
                if (p.socketType != otherAP.socketType) continue;

                var otherRb = otherAP.GetComponentInParent<Rigidbody>();
                // Если у другой части нет Rigidbody — это скорее всего не корпус, пропускаем
                if (otherRb == null) continue;

                // --- Колесо? ---
                bool newIsWheel = newPart.definition != null && newPart.definition.isWheelVisual;
                bool otherIsChassis = otherRb.GetComponent<ChassisPart>() != null;

                // Симметричный случай: если новая часть — корпус, а другая — визуальное колесо
                var otherPart = otherAP.GetComponentInParent<Part>();
                bool otherIsWheel = otherPart != null && otherPart.definition != null && otherPart.definition.isWheelVisual;
                bool newIsChassis = (newPart.rb != null && newPart.GetComponent<ChassisPart>() != null);

                if ((newIsWheel && otherIsChassis) || (newIsChassis && otherIsWheel))
                {
                    var chassis = otherIsChassis ? otherRb.GetComponent<ChassisPart>() : newPart.GetComponent<ChassisPart>();
                    var wheelVisual = newIsWheel ? newPart.transform : otherPart.transform;

                    if (chassis == null)
                    {
                        // если на корпусе нет ChassisPart — добавим
                        var chassisHolder = otherIsChassis ? otherRb.gameObject : newPart.gameObject;
                        chassis = chassisHolder.GetComponent<ChassisPart>() ?? chassisHolder.AddComponent<ChassisPart>();
                    }

                    // Снап визуала на attach корпуса
                    var chassisAttach = otherIsChassis ? otherAP.transform : p.transform;
                    wheelVisual.position = chassisAttach.position;
                    wheelVisual.rotation = chassisAttach.rotation;
                    wheelVisual.SetParent(chassis.transform, worldPositionStays: true);

                    chassis.AddWheel(wheelVisual);
                    Debug.Log($"[Attach] Wheel '{wheelVisual.name}' attached to chassis '{chassis.name}'");
                }
                else
                {
                    // Обычная деталь → FixedJoint (нужны оба Rigidbody)
                    if (newPart.rb == null)
                    {
                        // safety: вдруг на префабе не было RB, но это не колесо — добавим
                        var rb = newPart.gameObject.AddComponent<Rigidbody>();
                        rb.isKinematic = true; rb.useGravity = false; rb.mass = Mathf.Max(0.0001f, newPart.definition != null ? newPart.definition.mass : 1f);
                        newPart.rb = rb;
                    }
                    JointCreator.CreateFixedJoint(newPart.rb, otherRb, p.WorldPosition);
                    Debug.Log($"[Attach] FixedJoint {newPart.name} ↔ {otherRb.name} at {p.WorldPosition}");
                }
            }
        }
    }

    public void ClearAllPlaced()
    {
        foreach (var go in placedParts) if (go) Destroy(go);
        placedParts.Clear();
        if (currentGhost) Destroy(currentGhost);
        SpawnGhost();
    }

    public List<GameObject> GetPlacedParts() => placedParts;

    void ApplyHeightOffsetToGhost()
    {
        if (currentGhost == null) return;
        Vector3 pos = currentGhost.transform.position;
        Vector3 snappedXZ = SnapToGrid(new Vector3(pos.x, gridOrigin.y, pos.z));
        snappedXZ.y = gridOrigin.y + (currentHeightOffset * cellSize);
        currentGhost.transform.position = snappedXZ;
        lastGhostPosition = currentGhost.transform.position;
    }

    void OnGUI()
    {
        if (!showHeightUI) return;
        GUIStyle s = new GUIStyle(GUI.skin.box) { fontSize = 14 };
        GUILayout.BeginArea(new Rect(10, 10, 300, 60));
        GUILayout.Box($"Ghost height offset: {currentHeightOffset}  (y = {currentHeightOffset * cellSize:F2})\nF = up, G = down", s);
        GUILayout.EndArea();
    }
}
