using PLIbox.Audio;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NextWaveUI : MonoBehaviour
{
    private const string NEW_WAVE_SOUND = "NewWaveAlarm";

    [Header("References")]
    [SerializeField] private Image _redScreen;
    [SerializeField] private Image _warningText;

    [Header("Screen")]
    [SerializeField] private Color _redScreenColor;
    [SerializeField] private float _redScreenSpeed;
    [SerializeField] private float _redScreenDuration;

    [Header("Fadeout")]
    [SerializeField] private AnimationCurve _fadeoutCurve;
    [SerializeField] private float _fadeOutDuration;

    private Coroutine _redScreenCoroutine;

    private void Awake()
    {
        _redScreen.gameObject.SetActive(false);
        _warningText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        Wave.OnNextWave += StartWaveEffect;
    }

    private void OnDisable()
    {
        Wave.OnNextWave -= StartWaveEffect;
    }

    public void StartWaveEffect()
    {
        if(GameFeelManager.Instance.IsFeatureActive("NextWaveEffect"))
        {
            AudioManager.Instance.PlaySound(NEW_WAVE_SOUND);
            _redScreenCoroutine = StartCoroutine(RedScreenWave());
        }
    }

    private IEnumerator RedScreenWave()
    {
        _redScreen.gameObject.SetActive(true);
        _warningText.gameObject.SetActive(true);

        float timeElapsed = 0.0f;
        while (timeElapsed < _redScreenDuration)
        {
            Color redScreenColor = _redScreenColor;
            redScreenColor.a = _redScreenColor.a * Mathf.Sin(_redScreenSpeed * timeElapsed);
            _redScreen.color = redScreenColor;
                
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        timeElapsed = 0.0f;
        Color oldColor = _redScreen.color;
        while (timeElapsed < _fadeOutDuration)
        {
            float progression = _fadeoutCurve.Evaluate(timeElapsed / _fadeOutDuration);
            Color redScreenColor = _redScreenColor;
            redScreenColor.a = Mathf.Lerp(_redScreen.color.a, 0, progression);
            _redScreen.color = redScreenColor;

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        _redScreen.gameObject.SetActive(false);
        _warningText.gameObject.SetActive(false);
    }
}
