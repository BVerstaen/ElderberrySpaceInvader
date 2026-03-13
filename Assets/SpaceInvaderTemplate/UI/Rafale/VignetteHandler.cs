using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class VignetteHandler : MonoBehaviour
{
    private Volume _volume;
    private Vignette _vignette;
    private Bloom _bloom;

    [Header("Rafale Vignette Effect")]
    [SerializeField] private float minVignetteIntensity = 0.3f;
    [SerializeField] private float maxVignetteIntensity = 0.5f;
    [SerializeField] private float timeToReachVignetteIntensity = 0.5f;
    private float _targetedVignetteIntensity = 0f;
    private float _rafaleVignetteIntensity;
    private float _rafaleVignetteClock;
    private bool _isInRafale = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _volume = GetComponent<Volume>();
        if (!_volume.profile.TryGet(out _vignette))
        {
            Debug.LogError("No Vignette Effect found");
        }
        if (!_volume.profile.TryGet(out _bloom))
        {
            Debug.LogError("No Bloom Effect found");
        }
        Player.OnRafaleTriggered += OnRafaleTriggered;
        Player.OnRafaleStopped += OnRafaleStopped;
        GameFeelManager.Instance.OnFeatureToggled += OnFeatureToggled;
    }

    private void OnRafaleStopped()
    {
        _isInRafale = false;
    }

    private void OnDestroy()
    {
        Player.OnRafaleTriggered -= OnRafaleTriggered;
        Player.OnRafaleStopped -= OnRafaleStopped;
        if (GameFeelManager.Instance != null) GameFeelManager.Instance.OnFeatureToggled -= OnFeatureToggled;
    }

    private void OnFeatureToggled(string feature, bool bIsActive)
    {
        if (feature == "RafaleEffect")
        {
            _targetedVignetteIntensity = bIsActive ? _rafaleVignetteIntensity : 0;
        }
        else if (feature == "Bloom")
        {
            _bloom.active = bIsActive;
        }
    }

    private void OnRafaleTriggered(float rafaleTime, float rafaleIntensity)
    {
        _rafaleVignetteClock = 0;
        _rafaleVignetteIntensity = Mathf.Lerp(minVignetteIntensity, maxVignetteIntensity, rafaleIntensity);
        _targetedVignetteIntensity = GameFeelManager.Instance.IsFeatureActive("RafaleEffect") ? _rafaleVignetteIntensity : 0;
        _isInRafale = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isInRafale)
        {
            if (!_vignette) return;
            _rafaleVignetteClock = Mathf.Min(_rafaleVignetteClock + Time.deltaTime, timeToReachVignetteIntensity);
            _vignette.intensity.value = Mathf.Lerp(0, _targetedVignetteIntensity, (_rafaleVignetteClock / timeToReachVignetteIntensity));
        }
        else if (_rafaleVignetteClock > 0)
        {
            _rafaleVignetteClock = Mathf.Max(_rafaleVignetteClock - Time.deltaTime, 0);
            _vignette.intensity.value = Mathf.Lerp(0, _targetedVignetteIntensity, (_rafaleVignetteClock / timeToReachVignetteIntensity));
        }
    }
}
