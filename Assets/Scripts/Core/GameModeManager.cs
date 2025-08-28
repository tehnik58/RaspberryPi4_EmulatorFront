using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    public bool editMode = true;    // по умолчанию редактор
    public TMP_Text modeLabel;      // TMP текст для отображения режима
    public Button switchButton;     // кнопка переключения

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (switchButton != null)
            switchButton.onClick.AddListener(ToggleMode);

        ApplyMode(); // сразу применяем стартовый режим
    }

    public void ToggleMode()
    {
        editMode = !editMode;
        ApplyMode();
    }

    private void ApplyMode()
    {
        // Переключаем физику у всех rigidbody
        Rigidbody[] allBodies = FindObjectsOfType<Rigidbody>();
        foreach (var rb in allBodies)
        {
            rb.isKinematic = editMode; // в edit mode ВСЕ выключены
        }

        // Переключаем Drag&Drop
        DragAndDrop[] draggables = FindObjectsOfType<DragAndDrop>();
        foreach (var d in draggables)
        {
            d.enabled = editMode;
        }

        // Обновляем UI
        if (modeLabel != null)
            modeLabel.text = editMode ? "EDIT MODE" : "PLAY MODE";

        Debug.Log("Switched to " + (editMode ? "EDIT MODE" : "PLAY MODE"));
    }
}
