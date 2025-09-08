using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class SavedPart
{
    public string partId;
    public Vector3 position;
    public Quaternion rotation;
}

[System.Serializable]
public class SavedVehicle
{
    public List<SavedPart> parts = new List<SavedPart>();
}

public class SaveLoadManager : MonoBehaviour
{
    public BuilderManager builder;
    public string saveFileName = "saved_vehicle.json";

    string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    public void Save()
    {
        if (builder == null) { Debug.LogError("Builder reference missing"); return; }
        var list = builder.GetPlacedParts();
        SavedVehicle sv = new SavedVehicle();
        foreach (var go in list)
        {
            Part p = go.GetComponent<Part>();
            if (p == null || p.definition == null) continue;
            sv.parts.Add(new SavedPart { partId = p.definition.partId, position = go.transform.position, rotation = go.transform.rotation });
        }
        string json = JsonUtility.ToJson(sv, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Saved {sv.parts.Count} parts to {SavePath}");
    }

    public void Load()
    {
        if (builder == null) { Debug.LogError("Builder reference missing"); return; }
        if (!File.Exists(SavePath)) { Debug.LogWarning("Save file not found: " + SavePath); return; }

        string json = File.ReadAllText(SavePath);
        SavedVehicle sv = JsonUtility.FromJson<SavedVehicle>(json);

        builder.ClearAllPlaced();

        List<GameObject> created = new List<GameObject>();
        foreach (var sp in sv.parts)
        {
            PartDefinition def = FindDefinitionById(sp.partId);
            if (def == null) { Debug.LogWarning("Definition not found for id: " + sp.partId); continue; }

            GameObject go = Instantiate(def.prefab, sp.position, sp.rotation);
            Part partComp = go.GetComponent<Part>() ?? go.AddComponent<Part>();
            partComp.definition = def;

            // Rigidbody — только не для визуального колеса
            if (!def.isWheelVisual)
            {
                var rb = go.GetComponent<Rigidbody>() ?? go.AddComponent<Rigidbody>();
                rb.mass = Mathf.Max(0.0001f, def.mass);
                rb.isKinematic = true; rb.useGravity = false;
                partComp.rb = rb;
            }
            else
            {
                var rb = go.GetComponent<Rigidbody>();
                if (rb) Destroy(rb);
            }

            builder.GetPlacedParts().Add(go);
            created.Add(go);

            // Если есть ChassisPart, заново добавляем колёса
            ChassisPart chassis = go.GetComponent<ChassisPart>();
            if (chassis != null)
            {
                foreach (Transform wheelVisual in chassis.wheelVisuals)
                {
                    chassis.AddWheel(wheelVisual);
                }
            }
        }

        // Соединяем детали
        foreach (var go in created)
        {
            Part p = go.GetComponent<Part>();
            if (p != null) builder.CreateAttachJointsOrSnap(p);
        }

        Debug.Log($"Loaded {sv.parts.Count} parts from {SavePath}");
    }

    PartDefinition FindDefinitionById(string id)
    {
        var defs = Resources.LoadAll<PartDefinition>("");
        foreach (var d in defs) if (d.partId == id) return d;

        if (builder != null && builder.availableParts != null)
            foreach (var d in builder.availableParts) if (d != null && d.partId == id) return d;

        return null;
    }
}
