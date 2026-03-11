using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [SerializeField] private float _camDepth = -10;

    [Header("Shake properties")]
    [SerializeField] private Vector2 _shakeOffsetLimits;
    [SerializeField] private float _shakeSpeedLerp = 1;
    [SerializeField] private float _shakeDistanceThreshold = .5f;

    [Space(10)]
    [SerializeField] private AnimationCurve _fallBackToOriginCurve;
    [SerializeField] private float _fallBackToOriginDuration;

    private Coroutine _shakeCoroutine;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [ContextMenu("Test shake")]
    public void TestShake() => StartShaking(2);
    public void StartShaking(float duration)
    {
        if (!GameFeelManager.Instance.IsFeatureActive("RafaleEffect")) return;
        if(_shakeCoroutine != null)
        {
            Debug.LogWarning("a shake is already starting");
            StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = null;
        }
        _shakeCoroutine = StartCoroutine(ShakeRoutine(duration));
    }

    private IEnumerator ShakeRoutine(float duration)
    {
        float timeElapsed = 0.0f;
        bool reachedOffset = true;
        Vector3 offset = Vector3.zero;

        //Shake
        while (timeElapsed < duration)
        {
            if(reachedOffset)
            {
                offset = new Vector3(Random.Range(-_shakeOffsetLimits.x, _shakeOffsetLimits.x),
                                     Random.Range(-_shakeOffsetLimits.y, _shakeOffsetLimits.y),
                                     _camDepth);
                reachedOffset = false;
            }
            transform.position = Vector3.Lerp(transform.position, offset, Time.deltaTime * _shakeSpeedLerp);
            reachedOffset = Vector3.Distance(transform.position, offset) <= _shakeDistanceThreshold;

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        //Go back to origin
        float fallBackTimeElapsed = 0.0f;
        while (fallBackTimeElapsed < _fallBackToOriginDuration)
        {
            float progression = _fallBackToOriginCurve.Evaluate(fallBackTimeElapsed / _fallBackToOriginDuration);
            transform.position = Vector3.Lerp(transform.position, new Vector3(0, 0, _camDepth), progression);
            fallBackTimeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = new Vector3(0,0, _camDepth);
        _shakeCoroutine = null;
    }
}
