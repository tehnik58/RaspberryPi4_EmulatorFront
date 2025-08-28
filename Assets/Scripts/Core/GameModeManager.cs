using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    public bool editMode = true;    // �� ��������� ��������
    public TMP_Text modeLabel;      // TMP ����� ��� ����������� ������
    public Button switchButton;     // ������ ������������

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (switchButton != null)
            switchButton.onClick.AddListener(ToggleMode);

        ApplyMode(); // ����� ��������� ��������� �����
    }

    public void ToggleMode()
    {
        editMode = !editMode;
        ApplyMode();
    }

    private void ApplyMode()
    {
        // ����������� ������ � ���� rigidbody
        Rigidbody[] allBodies = FindObjectsOfType<Rigidbody>();
        foreach (var rb in allBodies)
        {
            rb.isKinematic = editMode; // � edit mode ��� ���������
        }

        // ����������� Drag&Drop
        DragAndDrop[] draggables = FindObjectsOfType<DragAndDrop>();
        foreach (var d in draggables)
        {
            d.enabled = editMode;
        }

        // ��������� UI
        if (modeLabel != null)
            modeLabel.text = editMode ? "EDIT MODE" : "PLAY MODE";

        Debug.Log("Switched to " + (editMode ? "EDIT MODE" : "PLAY MODE"));
    }
}
