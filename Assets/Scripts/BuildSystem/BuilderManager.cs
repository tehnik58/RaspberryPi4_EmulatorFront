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
    [Tooltip("Если установлен — будет использован как общий ghost. Если пусто — клонируется prefab из PartDefinition.")]
    public GameObject ghostPrefab;
    public Camera builderCamera;

    [Header("Placement")]
    public LayerMask placementRaycastMask = ~0;
    public float attachDetectionRadius = 0.05f;

    [Header("Height control")]
    public int currentHeightOffset = 0; // шаги по cellSize
    public int minHeightOffset = -10;
    public int maxHeightOffset = 50;
    public bool showHeightUI = true;

    GameObject currentGhost;
    int currentPartIndex = 0;
    List<GameObject> placedParts = new List<GameObject>();
    Vector3 lastGhostPosition = Vector3.zero; // чтобы хранить позицию когда raycast не попадает

    void Start()
    {
        // Надёжное назначение камеры
        if (builderCamera == null)
        {
            builderCamera = Camera.main;
            if (builderCamera == null)
            {
                var cams = FindObjectsOfType<Camera>();
                if (cams.Length > 0) builderCamera = cams[0];
            }
        }

        if (builderCamera == null)
        {
            Debug.LogError("[BuilderManager] Camera not assigned and no Camera found in scene. Assign BuilderCamera in Inspector.");
            return;
        }

        if (availableParts == null || availableParts.Length == 0)
        {
            Debug.LogWarning("[BuilderManager] availableParts is empty - assign PartDefinition assets.");
        }

        SpawnGhost();
    }

    void Update()
    {
        if (builderCamera == null) return;
        if (availableParts == null || availableParts.Length == 0) return;

        HandleQuickKeys();
        UpdateGhostPositionAndPlacement();
        HandleHeightKeys(); // отдельно, чтобы работало всегда
    }

    void HandleQuickKeys()
    {
        for (int i = 0; i < availableParts.Length && i < 9; i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
            {
                Debug.Log($"[BuilderManager] Key {i + 1} pressed -> select part index {i}");
                SetCurrentPartIndex(i);
            }
        }
        if (Input.GetKeyDown(KeyCode.R) && currentGhost != null)
        {
            currentGhost.transform.Rotate(Vector3.up, 90f, Space.World);
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (currentGhost != null)
            {
                bool colliding = CheckCollisionAt(currentGhost);
                if (!colliding)
                {
                    PlaceCurrentAt(currentGhost.transform.position, currentGhost.transform.rotation);
                }
                else
                {
                    Debug.Log("[BuilderManager] Cannot place - collision detected.");
                }
            }
        }
    }

    void HandleHeightKeys()
    {
        // F = вверх, G = вниз
        if (Input.GetKeyDown(KeyCode.F))
        {
            currentHeightOffset = Mathf.Min(maxHeightOffset, currentHeightOffset + 1);
            ApplyHeightOffsetToGhost();
            Debug.Log($"[BuilderManager] HeightOffset -> {currentHeightOffset} (y = {currentHeightOffset * cellSize})");
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            currentHeightOffset = Mathf.Max(minHeightOffset, currentHeightOffset - 1);
            ApplyHeightOffsetToGhost();
            Debug.Log($"[BuilderManager] HeightOffset -> {currentHeightOffset} (y = {currentHeightOffset * cellSize})");
        }
    }

    void UpdateGhostPositionAndPlacement()
    {
        Ray ray = builderCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, placementRaycastMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 snapped = SnapToGrid(hit.point);
            snapped += Vector3.up * (currentHeightOffset * cellSize);
            lastGhostPosition = snapped;
            if (currentGhost != null) currentGhost.transform.position = snapped;
            if (currentGhost != null)
            {
                bool colliding = CheckCollisionAt(currentGhost);
                SetGhostValid(!colliding);
            }
        }
        else
        {
            // raycast не попал — оставляем ghost там, где был последний раз (с учётом высоты)
            if (currentGhost != null)
            {
                // обновим Y в соответствии с offset, оставив XZ прежними
                Vector3 pos = lastGhostPosition;
                pos.y = gridOrigin.y + Mathf.Round((currentGhost.transform.position.y - gridOrigin.y) / cellSize) * cellSize;
                pos += Vector3.up * (currentHeightOffset * cellSize);
                currentGhost.transform.position = pos;
                lastGhostPosition = currentGhost.transform.position;
            }
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
            Debug.LogWarning("[BuilderManager] PartDefinition or prefab is null for index " + currentPartIndex);
            return;
        }

        GameObject source = ghostPrefab != null ? ghostPrefab : def.prefab;
        currentGhost = Instantiate(source);
        currentGhost.name = "GHOST_" + def.partId;

        // Сделать Rigidbody у ghost kinematic (вместо удаления)
        foreach (var rb in currentGhost.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Делаем все коллайдеры триггерами (чтобы ghost не физичил)
        foreach (var col in currentGhost.GetComponentsInChildren<Collider>()) col.isTrigger = true;

        // Безопасно инстанцируем материалы и делаем прозрачными
        foreach (var rend in currentGhost.GetComponentsInChildren<Renderer>())
        {
            if (rend == null) continue;
            Material mat = rend.material; // .material создает экземпляр материала
            MakeMaterialTransparent(mat, 0.6f);
        }

        // Поставим ghost чуть выше origin, пока raycast не попадёт
        currentGhost.transform.position = gridOrigin + Vector3.up * (1f * cellSize);
        lastGhostPosition = currentGhost.transform.position;
    }

    void MakeMaterialTransparent(Material mat, float alpha)
    {
        if (mat == null) return;
        if (mat.HasProperty("_Mode"))
        {
            mat.SetFloat("_Mode", 3f);
        }
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        if (mat.HasProperty("_Color"))
        {
            Color c = mat.color; c.a = alpha; mat.color = c;
        }
    }

    bool CheckCollisionAt(GameObject ghost)
    {
        foreach (var col in ghost.GetComponentsInChildren<Collider>())
        {
            if (col == null) continue;
            if (col is BoxCollider box)
            {
                Vector3 worldCenter = box.transform.TransformPoint(box.center);
                Vector3 worldHalf = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale);
                Collider[] hits = Physics.OverlapBox(worldCenter, worldHalf, box.transform.rotation, ~0, QueryTriggerInteraction.Ignore);
                foreach (var h in hits) if (!h.transform.IsChildOf(ghost.transform)) return true;
            }
            else if (col is SphereCollider sph)
            {
                Vector3 worldCenter = sph.transform.TransformPoint(sph.center);
                float worldRadius = sph.radius * Mathf.Max(sph.transform.lossyScale.x, sph.transform.lossyScale.y, sph.transform.lossyScale.z);
                Collider[] hits = Physics.OverlapSphere(worldCenter, worldRadius, ~0, QueryTriggerInteraction.Ignore);
                foreach (var h in hits) if (!h.transform.IsChildOf(ghost.transform)) return true;
            }
            else
            {
                var b = col.bounds;
                Collider[] hits = Physics.OverlapBox(b.center, b.extents, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
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
            if (!rend) continue;
            if (!rend.material.HasProperty("_Color")) continue;
            Color c = rend.material.color;
            c.r = ok ? 0f : 1f;
            c.g = ok ? 1f : 0f;
            rend.material.color = c;
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
        if (go.GetComponent<Rigidbody>() == null) go.AddComponent<Rigidbody>();
        Part partComp = go.GetComponent<Part>() ?? go.AddComponent<Part>();
        partComp.definition = def;
        partComp.rb = go.GetComponent<Rigidbody>();

        // оставляем массу, но делаем кинематическим и без гравитации
        partComp.rb.mass = Mathf.Max(0.0001f, def.mass);
        partComp.rb.isKinematic = true;
        partComp.rb.useGravity = false;

        CreateAttachJointsFor(partComp);
        placedParts.Add(go);
        Destroy(currentGhost);
        SpawnGhost();
    }

    public void PlaceAtMouse()
    {
        Ray ray = builderCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, placementRaycastMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 snapped = SnapToGrid(hit.point);
            snapped += Vector3.up * (currentHeightOffset * cellSize);
            bool colliding = currentGhost != null && CheckCollisionAt(currentGhost);
            if (!colliding) PlaceCurrentAt(snapped, currentGhost != null ? currentGhost.transform.rotation : Quaternion.identity);
        }
    }

    void CreateAttachJointsFor(Part newPart)
    {
        if (newPart == null) return;
        var points = newPart.GetComponentsInChildren<AttachPoint>(true);
        foreach (var p in points)
        {
            Collider[] hits = Physics.OverlapSphere(p.WorldPosition, attachDetectionRadius, ~0, QueryTriggerInteraction.Collide);
            foreach (var h in hits)
            {
                var otherAP = h.GetComponentInParent<AttachPoint>();
                if (otherAP == null) continue;
                if (otherAP.transform.IsChildOf(newPart.transform)) continue;
                if (p.socketType != otherAP.socketType) continue;
                var otherRb = otherAP.GetComponentInParent<Rigidbody>();
                if (otherRb == null) continue;
                JointCreator.CreateFixedJoint(newPart.rb, otherRb, p.WorldPosition);
                Debug.Log($"[BuilderManager] Attached {newPart.name} ↔ {otherRb.name} at {p.WorldPosition}");
            }
        }
    }

    // Очистить все установленные детали
    public void ClearAllPlaced()
    {
        foreach (var go in placedParts)
        {
            if (go != null) Destroy(go);
        }
        placedParts.Clear();

        if (currentGhost != null) Destroy(currentGhost);
        SpawnGhost();
    }

    // Получить список всех установленных деталей
    public List<GameObject> GetPlacedParts()
    {
        return placedParts;
    }

    void ApplyHeightOffsetToGhost()
    {
        if (currentGhost == null) return;
        Vector3 pos = currentGhost.transform.position;
        // округляем XZ к сетке, а Y устанавливаем по offset
        Vector3 snappedXZ = SnapToGrid(new Vector3(pos.x, gridOrigin.y, pos.z));
        snappedXZ.y = gridOrigin.y + (currentHeightOffset * cellSize);
        currentGhost.transform.position = snappedXZ;
        lastGhostPosition = currentGhost.transform.position;
    }

    void OnGUI()
    {
        if (!showHeightUI) return;
        GUIStyle s = new GUIStyle(GUI.skin.box) { fontSize = 14 };
        GUILayout.BeginArea(new Rect(10, 10, 260, 60));
        GUILayout.Box($"Ghost height offset: {currentHeightOffset}  (y = {currentHeightOffset * cellSize:F2})\nPress F = up, G = down", s);
        GUILayout.EndArea();
    }
}
