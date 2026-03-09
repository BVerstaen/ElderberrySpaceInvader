using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private TMP_Text _scoreText;
    
    [Header("Parameters")]
    [SerializeField] private string _textPrefix;
    [SerializeField, Tooltip("Temps d'attente entre l'ajout progressif du score")] private float _addDuration;
    [SerializeField] private AnimationCurve _scoreAddingCurve;

    private int _scoreToAdd = 0;
    private int _currentScore = 0;
    private int _scoreToReach = 0;
    private Coroutine _scoreAddingRoutine = null;

    private void Start()
    {
        _scoreToAdd = 0;
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
        _scoreToReach = newScore;
        _scoreToAdd += _scoreToReach - _currentScore;

        if(_scoreAddingRoutine == null)
        {
            _scoreAddingRoutine = StartCoroutine(ScoreAdd());
        }
    }

    private IEnumerator ScoreAdd()
    {
        while(_scoreToAdd > 0)
        {
            _currentScore++;
            _scoreToAdd--;
            WriteScore();
            float curveProgression = _scoreAddingCurve.Evaluate((float)_scoreToAdd / (float)(_scoreToAdd + _currentScore));
            print(1 - curveProgression);
            yield return new WaitForSeconds((1 - curveProgression) * _addDuration);
        }

        _scoreAddingRoutine = null;
        _scoreToAdd = 0;
        _currentScore = _scoreToReach;
        WriteScore();
    }

    private void WriteScore() => _scoreText.text = _textPrefix + " " + _currentScore.ToString();
}
