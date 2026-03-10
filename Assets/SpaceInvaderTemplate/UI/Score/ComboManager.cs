using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance;

    [Serializable]
    private struct ScorePalier
    {
        public int atPalier;
        public float scoreMultiplier;
    }

    [Header("Score")]
    [SerializeField] private int _baseScore;
    [SerializeField] private List<ScorePalier> _scorePalierList;

    [Header("UI")]
    [SerializeField] private ComboText _comboTextPrefab;
    [SerializeField] private Vector2 _comboTextStartPos;
    [SerializeField] private Vector2 _comboTextEndPos;
    [SerializeField] private string _comboPrefix;
    [SerializeField] private float _comboDuration;

    [Header("Scale animation")]
    [SerializeField] private float _scaleDuration;
    [SerializeField] private float _scaleMin;
    [SerializeField] private float _scaleMax;
    [SerializeField] private AnimationCurve _scaleCurve;

    private ComboText _currentComboText = null;

    private int _waitingScore = 0;
    private int _currentPalier = 0;
    private Coroutine _scoreCoroutine;

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
            Instance = this;
    }

    public void AddScore()
    {
        if(!GameFeelManager.Instance.IsFeatureActive("Combo"))
            GameManager.Instance.AddScore(_baseScore);
        else
        {
            _waitingScore += GetScoreToAdd();
            _currentPalier++;

            if (_scoreCoroutine != null)
            {
                StopCoroutine(_scoreCoroutine);
                _scoreCoroutine = null;
            }
            _scoreCoroutine = StartCoroutine(TimerRoutine());
        }
    }

    private int GetScoreToAdd()
    {
        int score = _baseScore;
        foreach (ScorePalier scorePalier in _scorePalierList)
        {
            if (_currentPalier >= scorePalier.atPalier)
                score = (int)(_baseScore * scorePalier.scoreMultiplier);
            else
                break;
        }
        return score;
    }

    private IEnumerator TimerRoutine()
    {
        if (_currentComboText == null)
        {
            _currentComboText = Instantiate(_comboTextPrefab, transform);
            _currentComboText.Init(_comboTextStartPos, _comboTextEndPos);
        }

        _currentComboText.UpdateScoreText(_comboPrefix + _waitingScore);

        float timeElapsed = 0.0f;
        while (timeElapsed < _comboDuration)
        {
            float progress = timeElapsed / _comboDuration;
            float scaleProgress = _scaleCurve.Evaluate(timeElapsed / _scaleDuration);
            _currentComboText.SetScoreTextScale(Mathf.Lerp(_scaleMin, _scaleMax, scaleProgress));

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        GameManager.Instance.AddScore(_waitingScore);
        _waitingScore = 0;
        _currentPalier = 0;
        _currentComboText.StartAnimation();
        _currentComboText = null;
    }
}
