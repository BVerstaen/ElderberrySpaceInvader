using System.Collections;
using UnityEngine;

public class ComboText : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform _rect;

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
