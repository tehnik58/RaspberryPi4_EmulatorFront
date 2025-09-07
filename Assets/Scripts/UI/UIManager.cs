using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public BuilderManager builder;
    public SaveLoadManager saveLoad;
    public Text currentPartText;

    void Update()
    {
        if (builder == null || currentPartText == null) return;
        int idx = 0; // default
        // ���� ����� � �������� ������� ������ �� builder (�������� ����� ��������)
        currentPartText.text = "Selected part: " + (builder.availableParts != null && builder.availableParts.Length > 0 ? builder.availableParts[0].partId : "-");
    }

    public void OnSaveButton()
    {
        if (saveLoad != null) saveLoad.Save();
    }
    public void OnLoadButton()
    {
        if (saveLoad != null) saveLoad.Load();
    }
}
