using UnityEngine;
using UnityEngine.UI;

public class RafaleSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillBar;
    [SerializeField] private Gradient fillBarColor;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player.OnRafaleChargeChanged += RafaleChargedChanged;
    }

    void OnDestroy()
    {
        Player.OnRafaleChargeChanged -= RafaleChargedChanged;
    }

    private void RafaleChargedChanged(float value)
    {
        slider.value = value;
        fillBar.color = fillBarColor.Evaluate(value);
    }
}
