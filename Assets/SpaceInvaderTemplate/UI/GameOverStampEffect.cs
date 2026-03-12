using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOverStampEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _stampImage;
    [SerializeField] private Image _fadeOutImage;

    [Header("Animation")]
    [SerializeField] private AnimationCurve _animationCurve;
    [SerializeField] private float _animationDuration;
    [SerializeField] private float _startScale;
    [SerializeField] private float _endScale;

    [Header("Fade out")]
    [SerializeField] private float _fadeOutDuration;
    [SerializeField] private AnimationCurve _fadeOutCurve;

    private Coroutine _stampAnimationCoroutine;

    private void OnEnable()
    {
        GameManager.Instance.OnGameOver += StartAnimation;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnGameOver -= StartAnimation;
    }

    public void StartAnimation()
    {
        _stampImage.SetActive(true);
        _fadeOutImage.gameObject.SetActive(true);
        _stampAnimationCoroutine = StartCoroutine(StampAnimationCoroutine());
    }

    private IEnumerator StampAnimationCoroutine()
    {
        float timeElapsed = 0.0f;

        _stampImage.transform.localScale = new Vector3(_startScale, _startScale, _startScale);
        while (timeElapsed < _animationDuration)
        {
            float progress = _animationCurve.Evaluate(timeElapsed / _animationDuration);

            if(timeElapsed < _fadeOutDuration)
            {
                Color fadeOutColor = _fadeOutImage.color;
                fadeOutColor.a = Mathf.Lerp(0, 1, progress);
                _fadeOutImage.color = fadeOutColor;
            }
            else
            {
                _fadeOutImage.color = new Color(0, 0, 0, 1);
                float lerpedScale = Mathf.Lerp(_startScale, _endScale, progress);
                _stampImage.transform.localScale = new Vector3(progress, progress, progress);
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        _stampImage.transform.localScale = new Vector3(_endScale, _endScale, _endScale);
    }
}
