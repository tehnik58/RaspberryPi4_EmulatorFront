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
        // очистим сцену
        builder.ClearAllPlaced();

        // ƒл€ каждой записи Ч найдем PartDefinition с совпадающим partId
        foreach (var sp in sv.parts)
        {
            PartDefinition def = FindDefinitionById(sp.partId);
            if (def == null) { Debug.LogWarning("Definition not found for id: " + sp.partId); continue; }
            GameObject go = Instantiate(def.prefab, sp.position, sp.rotation);
            if (go.GetComponent<Rigidbody>() == null) go.AddComponent<Rigidbody>();
            Part partComp = go.GetComponent<Part>();
            if (partComp == null) partComp = go.AddComponent<Part>();
            partComp.definition = def;
            partComp.rb = go.GetComponent<Rigidbody>();
            partComp.rb.mass = Mathf.Max(0.0001f, def.mass);

            // после того как все части будут инстанцированы, можно создавать соединени€.
            builder.GetPlacedParts().Add(go);
        }

        // ¬торой проход Ч создаЄм соединени€ между новыми част€ми (необходимо, потому что теперь все объекты в сцене)
        foreach (var go in builder.GetPlacedParts())
        {
            Part p = go.GetComponent<Part>();
            if (p != null) builder.Invoke("CreateAttachJointsFor", 0.01f); // ассинхронный вызов простым способом
        }

        Debug.Log($"Loaded {sv.parts.Count} parts from {SavePath}");
    }

    PartDefinition FindDefinitionById(string id)
    {
        // ищем по ресурсам или в сцене Ч быстрый способ: Resources.LoadAll<PartDefinition>
        var defs = Resources.LoadAll<PartDefinition>("");
        foreach (var d in defs) if (d.partId == id) return d;
        // fallback: поиск в сцене у builder
        if (builder != null && builder.availableParts != null)
        {
            foreach (var d in builder.availableParts) if (d != null && d.partId == id) return d;
        }
        return null;
    }
}
