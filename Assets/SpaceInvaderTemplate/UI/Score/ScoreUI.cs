using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    const string SCORE_FEATURE = "ScoreFeature";

    [Header("Reference")]
    [SerializeField] private TMP_Text _scoreText;

    [Header("Parameters")]
    [SerializeField] private string _textPrefix;
    [SerializeField, Tooltip("Temps d'attente entre l'ajout progressif du score")] private float _addDuration;
    [Space(10)]
    [SerializeField] private AnimationCurve _scoreAddingCurve;
    [SerializeField] private int _batchSize;

    private int _currentScore = 0;
    private int _scoreToReach = 0;
    private Coroutine _scoreAddingRoutine = null;

    private void Start()
    {
        _scoreToReach = 0;
        _currentScore = 0;
        WriteScore();
    }

    private void OnEnable()
    {
        GameManager.Instance.OnUpdateScore += AddScore;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnUpdateScore -= AddScore;   
    }

    public void AddScore(int newScore)
    {
        if(GameFeelManager.Instance.IsFeatureActive(SCORE_FEATURE))
        {
            _scoreToReach = newScore;

            if (_scoreAddingRoutine == null)
            {
                _scoreAddingRoutine = StartCoroutine(ScoreAdd());
            }
        }
        else
        {
            if (_scoreAddingRoutine != null)
            {
                StopCoroutine(_scoreAddingRoutine);
                _scoreAddingRoutine = null;
            }

            _scoreToReach = newScore;
            _currentScore = newScore;
            WriteScore();
        }
    }

    private IEnumerator ScoreAdd()
    {
        while ((_scoreToReach - _currentScore) > 0)
        {
            int diff = _scoreToReach - _currentScore;
            _currentScore += Mathf.Max(1, diff / _batchSize);
            if(_currentScore > _scoreToReach)
                _currentScore = _scoreToReach;

            WriteScore();

            float curveProgression = _scoreAddingCurve.Evaluate((float)diff / _scoreToReach);
            yield return new WaitForSeconds((1 - curveProgression) * _addDuration);
        }

        _scoreAddingRoutine = null;
        _currentScore = _scoreToReach;
        WriteScore();
    }

    private void WriteScore() => _scoreText.text = _textPrefix + " " + _currentScore.ToString();
}
