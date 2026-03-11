using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class VignetteHandler : MonoBehaviour
{
    private Volume _volume;
    private Vignette _vignette;

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
        Player.OnRafaleTriggered += OnRafaleTriggered;
        GameFeelManager.Instance.OnFeatureToggled += OnFeatureToggled;
    }

    private void OnFeatureToggled(string feature, bool bIsActive)
    {
        if (feature == "RafaleEffect")
        {
            _targetedVignetteIntensity = bIsActive ? _rafaleVignetteIntensity : 0;
        }
    }

    private void OnRafaleTriggered(float rafaleTime, float rafaleIntensity)
    {
        _rafaleVignetteClock = 0;
        _rafaleVignetteIntensity = Mathf.Lerp(minVignetteIntensity, maxVignetteIntensity, rafaleIntensity);
        _targetedVignetteIntensity = GameFeelManager.Instance.IsFeatureActive("RafaleEffect") ? _rafaleVignetteIntensity : 0;
        _isInRafale = true;
        StartCoroutine(RafaleCoroutine(rafaleTime));
    }

    private IEnumerator RafaleCoroutine(float rafaleTime)
    {
        yield return new WaitForSeconds(rafaleTime);
        _isInRafale = false;
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
