using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RafaleSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillBar;
    [SerializeField] private Gradient fillBarColor;
    [SerializeField] private Color rafaleColor;
    private bool _isInRafale = false;
    private float _rafaleTime = 0;
    private float _rafaleClock = 0;
    private float _startRafaleValue = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player.OnRafaleChargeChanged += RafaleChargedChanged;
        Player.OnRafaleTriggered += RafaleTriggered;
    }

    private void Update()
    {
        if (_isInRafale)
        {
            _rafaleClock += Time.deltaTime;
            slider.value = Mathf.Lerp(0, _startRafaleValue, 1 - (_rafaleClock / _rafaleTime));
            if (_rafaleClock >= _rafaleTime)
            {
                _isInRafale = false;
                RafaleChargedChanged(0);
            }
        }
    }

    void OnDestroy()
    {
        Player.OnRafaleChargeChanged -= RafaleChargedChanged;
        Player.OnRafaleTriggered -= RafaleTriggered;
    }

    private void RafaleChargedChanged(float value)
    {
        if (_isInRafale) return;
        slider.value = value;
        fillBar.color = fillBarColor.Evaluate(value);
    }

    private void RafaleTriggered(float rafaleTime)
    {
        _isInRafale = true;
        _rafaleTime = rafaleTime;
        fillBar.color = rafaleColor;
        _startRafaleValue = slider.value;
        _rafaleClock = 0;
    }
}
