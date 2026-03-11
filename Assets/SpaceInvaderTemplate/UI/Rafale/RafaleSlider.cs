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
    [SerializeField] private float sliderValueSpeed = 1f;
    private bool _isInRafale = false;
    private float _rafaleTime = 0;
    private float _rafaleClock = 0;
    private float _startRafaleValue = 0;
    private float _rafaleValueTarget = 0;
    private float _sliderSpeed = 0;
    
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
        else if (!Mathf.Approximately(_rafaleValueTarget, slider.value))
        {
            if (!GameFeelManager.Instance.IsFeatureActive("RafaleChargeSliderAnimation"))
            {
                slider.value = _rafaleValueTarget;
            }
            else
            {
                if (_rafaleValueTarget > slider.value)
                {
                    slider.value += _sliderSpeed * Time.deltaTime;
                    slider.value = Mathf.Min(slider.value, _rafaleValueTarget);
                }
                else
                {
                    slider.value -= _sliderSpeed * Time.deltaTime;
                    slider.value = Mathf.Max(slider.value, _rafaleValueTarget);
                }
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
        _rafaleValueTarget = value;
        fillBar.color = fillBarColor.Evaluate(value);
        //Calculate sliderSpeed according to difference of value
        _sliderSpeed = sliderValueSpeed * Mathf.Max(Mathf.Abs(value - slider.value), 0.2f);
    }

    private void RafaleTriggered(float rafaleTime, float rafaleIntensity)
    {
        if (!GameFeelManager.Instance.IsFeatureActive("RafaleChargeSliderAnimation")) return;
        _isInRafale = true;
        _rafaleTime = rafaleTime;
        fillBar.color = rafaleColor;
        slider.value = rafaleIntensity;
        _startRafaleValue = rafaleIntensity;
        _rafaleClock = 0;
    }
}
