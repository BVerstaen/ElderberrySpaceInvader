using PLIbox.Audio;
using PLIbox.ListExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class ComboManager : MonoBehaviour
{
    const string SCORE_FEATURE = "ScoreFeature";

    public static ComboManager Instance;

    [Serializable]
    private struct ScorePalier
    {
        [Header("Score")]
        public int atPalier;
        public float scoreMultiplier;
        
        [Space(5)]
        [Header("Scale")]
        public float scoreScaleMultiplier;

        [Space(5)]
        [Header("Color")]
        public Material newTextMaterial;

        [Space(5)]
        [Header("Voicelines")]
        public float probaOfVoicelineToPlay;
        public List<string> voicelinesToPlay;
    }

    [Header("Score")]
    [SerializeField] private int _baseScore;
    [SerializeField] private Material _baseColorMaterial;
    [SerializeField] private List<ScorePalier> _scorePalierList;

    [Header("UI")]
    [SerializeField] private ComboText _comboTextPrefab;
    [SerializeField] private Vector2 _comboTextStartPos;
    [SerializeField] private Vector2 _comboTextEndPos;
    [SerializeField] private float _comboDuration;

    [Header("Scale animation")]
    [SerializeField] private float _scaleDuration;
    [SerializeField] private float _scaleMin;
    [SerializeField] private float _scaleMax;
    [SerializeField] private AnimationCurve _scaleCurve;

    private ComboText _currentComboText = null;

    private int _waitingScore = 0;
    private Material _currentColor;
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

        _currentColor = _baseColorMaterial;
    }

    public void AddScore()
    {
        if(!GameFeelManager.Instance.IsFeatureActive(SCORE_FEATURE))
            GameManager.Instance.AddScore(_baseScore);
        else
        {
            _waitingScore += GetScoreToAdd(out _currentColor, out float scaleMultiplier);
            _currentPalier++;

            if (_scoreCoroutine != null)
            {
                StopCoroutine(_scoreCoroutine);
                _scoreCoroutine = null;
            }
            _scoreCoroutine = StartCoroutine(TimerRoutine(scaleMultiplier));
        }
    }

    private int GetScoreToAdd(out Material scoreColor, out float scaleMultiplier)
    {
        int score = _baseScore;
        scoreColor = _baseColorMaterial;
        scaleMultiplier = 1f;
        string currentSoundToPlay = "";
        foreach (ScorePalier scorePalier in _scorePalierList)
        {
            if (_currentPalier >= scorePalier.atPalier)
            {
                score = (int)(_baseScore * scorePalier.scoreMultiplier);
                scoreColor = scorePalier.newTextMaterial;
                scaleMultiplier = scorePalier.scoreScaleMultiplier;

                if (Random.Range(0, 100) <= scorePalier.probaOfVoicelineToPlay && scorePalier.voicelinesToPlay.Count > 0)
                    currentSoundToPlay = scorePalier.voicelinesToPlay.GetRandomItem();
                else
                    currentSoundToPlay = "";
            }
            else
                break;
        }

        //Feedback
        if(currentSoundToPlay != "")
            AudioManager.Instance.PlaySound(currentSoundToPlay);

        return score;
    }

    private IEnumerator TimerRoutine(float _scaleMultiplier)
    {
        if (_currentComboText == null)
        {
            _currentComboText = Instantiate(_comboTextPrefab, transform);
            _currentComboText.Init(_comboTextStartPos, _comboTextEndPos);
        }

        _currentComboText.UpdateScoreText(_waitingScore.ToString(), _currentColor);

        float timeElapsed = 0.0f;
        while (timeElapsed < _comboDuration)
        {
            float progress = timeElapsed / _comboDuration;
            float scaleProgress = _scaleCurve.Evaluate(timeElapsed / _scaleDuration);
            _currentComboText.SetScoreTextScale(_scaleMultiplier * Mathf.Lerp(_scaleMin, _scaleMax, scaleProgress));

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
