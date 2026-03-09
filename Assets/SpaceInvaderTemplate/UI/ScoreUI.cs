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
        _scoreToReach = newScore;

        if(_scoreAddingRoutine == null)
        {
            _scoreAddingRoutine = StartCoroutine(ScoreAdd());
        }
    }

    private IEnumerator ScoreAdd()
    {
        while ((_scoreToReach - _currentScore) > 0)
        {
            _currentScore++;
            WriteScore();

            float curveProgression = _scoreAddingCurve.Evaluate((float)(_scoreToReach - _currentScore) / _scoreToReach);
            yield return new WaitForSeconds((1 - curveProgression) * _addDuration);
        }

        _scoreAddingRoutine = null;
        _currentScore = _scoreToReach;
        WriteScore();
    }

    private void WriteScore() => _scoreText.text = _textPrefix + " " + _currentScore.ToString();
}
