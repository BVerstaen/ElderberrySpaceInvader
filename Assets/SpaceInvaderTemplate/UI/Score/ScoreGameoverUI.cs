using System.Text;
using TMPro;
using UnityEngine;

public class ScoreGameoverUI : MonoBehaviour
{
    private TMP_Text _textScore => GetComponent<TMP_Text>();

    public void Start()
    {
        int score = GameManager.Instance.PlayerScore;
        string scoreText = "";
        for (int i = 0; i < score.ToString().Length; i++)
        {
            char number = score.ToString()[i];
            StringBuilder sb = new StringBuilder();
            sb.Append($"<sprite={number}>");
            scoreText += sb.ToString();
        }
        _textScore.text = scoreText.ToString();
    }
}
