using System;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private AudioSource _musicSource;

    [Header("Music")]
    [SerializeField] private AudioClip _menuMusic;
    [SerializeField] private AudioClip _gameMusic;


    private void Awake()
    {
        _musicSource.clip = _menuMusic;
        _musicSource.Play();
    }

    private void OnEnable()
    {
        GameManager.Instance.OnStartGame += ChangeToDarkXXXMusic;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnStartGame -= ChangeToDarkXXXMusic;
    }

    private void ChangeToDarkXXXMusic()
    {
        _musicSource.clip = _gameMusic;
        _musicSource.Play();
    }
}
