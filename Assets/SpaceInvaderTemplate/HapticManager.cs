using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance;

    private Coroutine _waitForEndCoroutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [ContextMenu("Test haptic")]
    internal void TestRumble() => StartRumble(100, 200, 5);

    public void StartRumble(float lowFreq, float highFreq, float duration)
    {
        if (!GameFeelManager.Instance.IsFeatureActive("Haptics")) return;
        Gamepad pad = Gamepad.current;
        if(pad != null)
        {
            pad.SetMotorSpeeds(lowFreq, highFreq);

            if(_waitForEndCoroutine != null)
            {
                StopCoroutine(_waitForEndCoroutine);
                _waitForEndCoroutine = null;
            }
            _waitForEndCoroutine = StartCoroutine(WaitEndRumble(duration, pad));
        }
    }

    private IEnumerator WaitEndRumble(float duration, Gamepad pad)
    {
        yield return new WaitForSeconds(duration);
        pad.SetMotorSpeeds(0, 0);
    }
}
