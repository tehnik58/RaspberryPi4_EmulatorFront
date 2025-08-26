using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GPIOVisualizer : MonoBehaviour
{
    [System.Serializable]
    public class PinVisual
    {
        public int pinNumber;
        public Image pinImage;
        public Text pinText;
    }

    [SerializeField] private List<PinVisual> pinVisuals = new List<PinVisual>();
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.gray;

    private Dictionary<int, PinVisual> pinDictionary = new Dictionary<int, PinVisual>();

    private void Start()
    {
        InitializePins();
    }

    private void InitializePins()
    {
        pinDictionary.Clear();
        foreach (var pinVisual in pinVisuals)
        {
            pinDictionary[pinVisual.pinNumber] = pinVisual;
            UpdatePinVisual(pinVisual, false);
        }
    }

    public void UpdatePinState(int pinNumber, bool isActive)
    {
        if (pinDictionary.TryGetValue(pinNumber, out PinVisual pinVisual))
        {
            UpdatePinVisual(pinVisual, isActive);
        }
        else
        {
            Debug.LogWarning($"Pin {pinNumber} not found in visualizer");
        }
    }

    private void UpdatePinVisual(PinVisual pinVisual, bool isActive)
    {
        if (pinVisual.pinImage != null)
        {
            pinVisual.pinImage.color = isActive ? activeColor : inactiveColor;
            
            // Добавляем свечение для активного состояния
            if (isActive)
            {
                pinVisual.pinImage.transform.localScale = Vector3.one * 1.2f;
            }
            else
            {
                pinVisual.pinImage.transform.localScale = Vector3.one;
            }
        }
    }

    public void ResetAllPins()
    {
        foreach (var pinVisual in pinVisuals)
        {
            UpdatePinVisual(pinVisual, false);
        }
    }

    // Автоматическое создание визуализации пинов
    public void CreatePinVisualization(Transform parent, int[] importantPins)
    {
        foreach (int pin in importantPins)
        {
            GameObject pinObject = new GameObject($"GPIO_Pin_{pin}");
            pinObject.transform.SetParent(parent);
            
            Image image = pinObject.AddComponent<Image>();
            image.color = inactiveColor;
            image.rectTransform.sizeDelta = new Vector2(60, 60);
            
            GameObject textObject = new GameObject("PinText");
            textObject.transform.SetParent(pinObject.transform);
            Text text = textObject.AddComponent<Text>();
            text.text = pin.ToString();
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.rectTransform.sizeDelta = new Vector2(60, 60);
            
            pinVisuals.Add(new PinVisual { pinNumber = pin, pinImage = image, pinText = text });
        }
        
        InitializePins();
    }
}