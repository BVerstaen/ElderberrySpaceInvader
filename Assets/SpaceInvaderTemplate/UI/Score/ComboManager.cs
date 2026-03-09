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
    [SerializeField] private TMP_Text _comboTextObject;
    [SerializeField] private string _comboPrefix;
    [SerializeField] private float _comboDuration;

    [Header("Scale animation")]
    [SerializeField] private float _scaleDuration;
    [SerializeField] private float _scaleMin;
    [SerializeField] private float _scaleMax;
    [SerializeField] private AnimationCurve _scaleCurve;


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
            GameManager.Instance.AddScore(_waitingScore);
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
        _comboTextObject.gameObject.SetActive(true);
        _comboTextObject.text = _comboPrefix + _waitingScore;

        float timeElapsed = 0.0f;
        while (timeElapsed < _comboDuration)
        {
            float progress = timeElapsed / _comboDuration;
            float scaleProgress = _scaleCurve.Evaluate(timeElapsed / _scaleDuration);
            SetScale(scaleProgress);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        GameManager.Instance.AddScore(_waitingScore);
        _waitingScore = 0;
        _currentPalier = 0;
        _comboTextObject.gameObject.SetActive(false);
    }

    private void SetScale(float progression)
    {
        float scale = Mathf.Lerp(_scaleMin, _scaleMax, progression);
        _comboTextObject.transform.localScale = new Vector3(scale, scale, scale);
    }
}
