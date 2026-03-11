using System.Text;
using TMPro;
using UnityEngine;

public class TitleScreenUI : MonoBehaviour
{
    private const string PLAYER_SCORE_PREF_KEY = "Highscore";

    [Header("Reference")]
    [SerializeField] private TMP_Text _highscoreText;

    [Header("Feedback")]
    [SerializeField] private float _highTextSpeed;
    [SerializeField] private float _highTextAmplitude;
    [SerializeField] private float _highTextMinimumScale;

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
