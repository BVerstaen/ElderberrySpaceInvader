using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenUI : MonoBehaviour
{
    private const string PLAYER_SCORE_PREF_KEY = "Highscore";

    [Header("Reference")]
    [SerializeField] private TMP_Text _highscoreText;
    [SerializeField] private Image _fadeInImage;

    [Header("Feedback")]
    [SerializeField] private float _highTextSpeed;
    [SerializeField] private float _highTextAmplitude;
    [SerializeField] private float _highTextMinimumScale;

    [Header("Fade In")]
    [SerializeField] private AnimationCurve _fadeInCurve;
    [SerializeField] private float _fadeInDuration;

    private Coroutine _fadeInCoroutine;

    public static Action OnStartGame; 

    private void Start()
    {
        if (PlayerPrefs.HasKey(PLAYER_SCORE_PREF_KEY))
        {
            string highscoreText = PlayerPrefs.GetInt(PLAYER_SCORE_PREF_KEY).ToString();
            string convertedScoreText = "";
            for (int i = 0; i < highscoreText.Length; i++)
            {
                char number = highscoreText[i];
                StringBuilder sb = new StringBuilder();
                sb.Append($"<sprite={number}>");
                convertedScoreText += sb.ToString();
            }
            _highscoreText.text = convertedScoreText;
        }
        else
            _highscoreText.text = "";

        _fadeInCoroutine = StartCoroutine(FadeInAnimation());
    }

    private void Update()
    {
        //highscore scale text
        if(GameFeelManager.Instance.IsFeatureActive("MainMenuAnimation"))
        {
            float highScoreScale = ((Mathf.Sin(Time.time * _highTextSpeed) + 1) * _highTextAmplitude) + _highTextMinimumScale;
            _highscoreText.rectTransform.localScale = new Vector3(highScoreScale, highScoreScale, highScoreScale);
        }
    }

    private IEnumerator FadeInAnimation()
    {
        float timeElasped = 0.0f;

        _fadeInImage.gameObject.SetActive(true);
        _fadeInImage.color = Color.black;
        while (timeElasped < _fadeInDuration)
        {
            float progress = _fadeInCurve.Evaluate(timeElasped / _fadeInDuration);
            _fadeInImage.color = Color.Lerp(Color.black, new Color(0, 0, 0, 0), progress);

            timeElasped += Time.deltaTime;
            yield return null;
        }
        _fadeInImage.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        GameManager.Instance.StartGame();
        gameObject.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
