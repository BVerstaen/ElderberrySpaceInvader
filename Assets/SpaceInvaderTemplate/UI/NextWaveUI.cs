using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NextWaveUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _redScreen;
    [SerializeField] private TMP_Text _warningText;

    [Header("Screen")]
    [SerializeField] private Color _redScreenColor;
    [SerializeField] private float _redScreenSpeed;
    [SerializeField] private float _redScreenDuration;

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

    [ContextMenu("Feur")]
    public void StartWaveEffect()
    {
        _redScreenCoroutine = StartCoroutine(RedScreenWave());
    }

    private IEnumerator RedScreenWave()
    {
        _redScreen.gameObject.SetActive(true);
        _warningText.gameObject.SetActive(true);

        float timeElapsed = 0.0f;
        while (timeElapsed < _redScreenDuration)
        {
            Color redScreenColor = _redScreenColor;
            redScreenColor.a = Mathf.Sin(_redScreenSpeed * timeElapsed);
            _redScreen.color = redScreenColor;
                
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        _redScreen.gameObject.SetActive(false);
        _warningText.gameObject.SetActive(false);
    }
}
