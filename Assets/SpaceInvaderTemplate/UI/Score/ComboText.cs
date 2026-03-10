using System.Collections;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;

public class ComboText : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform _rect;
    [SerializeField] private TMP_Text _scoreText;

    [Header("Movement animation")]
    [SerializeField] private AnimationCurve _animationCurve;
    [SerializeField] private float _animationDuration;

    private Vector2 _startPosition;
    private Vector2 _endPosition;

    private Coroutine _movementCoroutine;

    public void Init(Vector2 startPos, Vector2 endPos)
    {
        _startPosition = startPos;
        _endPosition = endPos;
        _rect.anchoredPosition = _startPosition;
    }

    public void UpdateScoreText(string scoreText, Color scoreColor)
    {
        string convertedScoreText = $"<color=#{ColorUtility.ToHtmlStringRGB(scoreColor)}>";

        for (int i = 0; i < scoreText.Length; i++)
        {
            char number = scoreText[i];
            StringBuilder sb = new StringBuilder();
            sb.Append($"<sprite={number}>");
            convertedScoreText += sb.ToString();
        }
        _scoreText.text = convertedScoreText;
    }

    public void SetScoreTextScale(float scale)
    {
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public void StartAnimation()
    {
        _movementCoroutine = StartCoroutine(MovementRoutine());
    }

    private IEnumerator MovementRoutine()
    {
        float timeElapsed = 0.0f;
        while (timeElapsed < _animationDuration)
        {
            float progression = _animationCurve.Evaluate(timeElapsed / _animationDuration);
            _rect.anchoredPosition = Vector2.Lerp(_startPosition, _endPosition, progression);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
