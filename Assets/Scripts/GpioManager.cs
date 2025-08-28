using UnityEngine;
using UnityEngine.UI;

public class GpioPinManager : MonoBehaviour
{
    public GpioPinVisualization[] gpioPins;
    
    public void UpdatePinState(int pinNumber, bool state)
    {
        foreach (var pin in gpioPins)
        {
            if (pin.pinNumber == pinNumber)
            {
                pin.SetState(state);
                break;
            }
        }
    }
    
    public void TogglePinState(int pinNumber)
    {
        foreach (var pin in gpioPins)
        {
            if (pin.pinNumber == pinNumber)
            {
                bool newState = !(pin.pinImage.color == pin.activeColor);
                pin.SetState(newState);
                break;
            }
        }
    }
}