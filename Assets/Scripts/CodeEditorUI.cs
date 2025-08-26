using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodeEditorUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private Button executeButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button clearButton;

    private RaspberryPiEmulator emulator;

    private void Start()
    {
        emulator = FindObjectOfType<RaspberryPiEmulator>();
        
        executeButton.onClick.AddListener(ExecuteCode);
        stopButton.onClick.AddListener(StopExecution);
        clearButton.onClick.AddListener(ClearCode);

        // Загружаем начальный код
        if (emulator != null)
        {
            codeInputField.text = emulator.GetInitialCode();
        }
    }

    public void ExecuteCode()
    {
        if (emulator != null)
        {
            emulator.ExecuteCode(codeInputField.text);
        }
    }

    public void StopExecution()
    {
        if (emulator != null)
        {
            //emulator.StopExecution();
        }
    }

    public void ClearCode()
    {
        codeInputField.text = "";
    }

    public void SetCode(string code)
    {
        codeInputField.text = code;
    }

    public string GetCode()
    {
        return codeInputField.text;
    }
}