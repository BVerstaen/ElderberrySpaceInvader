using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private float _rotationAmplitude;

    [Header("Scale")]
    [SerializeField] private float _growScale;
    [SerializeField] private float _duration;

    private bool _isGrowing;
    private RectTransform _rect => GetComponent<RectTransform>();

    Coroutine growRoutine;
    Coroutine degrowRoutine;

    void Update()
    {
        if (!_isGrowing)
            return;

        Vector3 newRotation = Vector3.zero;
        newRotation.z = Mathf.Sin(Time.time * _rotationSpeed) * _rotationAmplitude;
        _rect.localEulerAngles = newRotation;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(degrowRoutine != null)
            StopCoroutine(degrowRoutine);
        growRoutine = StartCoroutine(Grow());
        _isGrowing = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (growRoutine != null)
            StopCoroutine(growRoutine);
        degrowRoutine = StartCoroutine(DeGrow());

        _isGrowing = false;
        _rect.localEulerAngles = Vector3.zero;
    }

    IEnumerator Grow()
    {
        float timeElapsed = 0;
        Vector3 startingPosition = transform.localScale;
        Vector3 endPosition = new Vector3(_growScale, _growScale, _growScale);
        while (timeElapsed < _duration)
        {
            transform.localScale = Vector3.Lerp(startingPosition, endPosition, timeElapsed / _duration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = endPosition;
    }

    IEnumerator DeGrow()
    {
        float timeElapsed = 0;
        Vector3 startingPosition = transform.localScale;
        Vector3 endPosition = new Vector3(1, 1, 1);
        while (timeElapsed < _duration)
        {
            transform.localScale = Vector3.Lerp(startingPosition, endPosition, timeElapsed / _duration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = endPosition;
    }
}
