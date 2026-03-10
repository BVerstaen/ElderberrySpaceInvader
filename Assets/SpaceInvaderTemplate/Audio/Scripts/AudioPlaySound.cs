using UnityEngine;

namespace PLIbox.Audio
{
    public class AudioPlaySound : MonoBehaviour
    {
        [SerializeField] private string _soundName;
        [Tooltip("Leave -1 to be random")]
        [SerializeField] private int _audioDataIndex = -1;
        [Space(10)]
        [SerializeField] private bool _playOnEnable;

        private void OnEnable()
        {
            if (_playOnEnable)
            {
                PlaySound();
            }
        }

        public void PlaySound()
        {
            if (AudioManager.Instance)
            {
                AudioManager.Instance.PlaySound(_soundName, _audioDataIndex);
            }
        }
    }
}