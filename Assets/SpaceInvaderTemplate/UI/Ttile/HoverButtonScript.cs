using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float _growScale;
    [SerializeField] private float _duration;

    Coroutine growRoutine;
    Coroutine degrowRoutine;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(degrowRoutine != null)
            StopCoroutine(degrowRoutine);
        growRoutine = StartCoroutine(Grow());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (growRoutine != null)
            StopCoroutine(growRoutine);
        degrowRoutine = StartCoroutine(DeGrow());
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
