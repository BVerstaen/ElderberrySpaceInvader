using System;
using System.Collections;
using UnityEngine;

public class SkyBackgroundSpeed : MonoBehaviour
{
    [SerializeField] private Material _skyBackgroundMat;
    [SerializeField] private ParticleSystem _rainSystem;

    [Header("Menu")]
    [SerializeField] private float _X_Pam_Menu;
    [SerializeField] private float _X_Pam_Cloud_Menu;
    [SerializeField] private float _Y_Pam_Menu;
    [SerializeField] private float _Y_Pam_Cloud_Menu;

    [Header("Game")]
    [SerializeField] private float _X_Pam_Game;
    [SerializeField] private float _X_Pam_Cloud_Game;
    [SerializeField] private float _Y_Pam_Game;
    [SerializeField] private float _Y_Pam_Cloud_Game;

    [SerializeField] private float _duration;

    private bool _isInMenu = true; 

    private void Awake()
    {
        _skyBackgroundMat.SetFloat("_X_Pan", _X_Pam_Menu);
        _skyBackgroundMat.SetFloat("_X_Pan_Cloud", _X_Pam_Cloud_Menu);

        _skyBackgroundMat.SetFloat("_Y_Pan1", _Y_Pam_Menu);
        _skyBackgroundMat.SetFloat("_Y_Pan_Cloud", _Y_Pam_Cloud_Menu);
    }

    private void OnEnable()
    {
        GameManager.Instance.OnStartGame += GoToGame;
        GameFeelManager.Instance.OnFeatureToggled += QueRIDER;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnStartGame -= GoToGame;
        
        if(GameFeelManager.Instance)
            GameFeelManager.Instance.OnFeatureToggled -= QueRIDER;
    }

    private void QueRIDER(string feature, bool toggle)
    {
        if(feature == "PlayerMovement")
        {
            if(toggle)
            {
                _skyBackgroundMat.SetFloat("_X_Pan", _isInMenu ? _X_Pam_Menu: _X_Pam_Game);
                _skyBackgroundMat.SetFloat("_X_Pan_Cloud", _isInMenu ? _X_Pam_Cloud_Menu : _X_Pam_Cloud_Game);

                _skyBackgroundMat.SetFloat("_Y_Pan1", _isInMenu ? _Y_Pam_Menu : _Y_Pam_Game);
                _skyBackgroundMat.SetFloat("_Y_Pan_Cloud", _isInMenu ? _Y_Pam_Cloud_Menu : _Y_Pam_Cloud_Game);
                
                _rainSystem.Play();
            }
            else
            {
                _skyBackgroundMat.SetFloat("_X_Pan", 0);
                _skyBackgroundMat.SetFloat("_X_Pan_Cloud", 0);

                _skyBackgroundMat.SetFloat("_Y_Pan1", 0);
                _skyBackgroundMat.SetFloat("_Y_Pan_Cloud", 0);

                _rainSystem.Stop();
            }
        }
    }

    private void GoToGame()
    {
        _isInMenu = false;
        if(GameFeelManager.Instance.IsFeatureActive("PlayerMovement"))
            StartCoroutine(TransitionVOlo());
    }

    private IEnumerator TransitionVOlo()
    {
        float timeElasped = 0.0f;
        while (timeElasped < _duration)
        {
            float progress = timeElasped / _duration;

            _skyBackgroundMat.SetFloat("_X_Pan", Mathf.Lerp(_X_Pam_Menu, _X_Pam_Game, progress));
            _skyBackgroundMat.SetFloat("_Y_Pan1", Mathf.Lerp(_Y_Pam_Menu, _Y_Pam_Game, progress));

            _skyBackgroundMat.SetFloat("_X_Pan_Cloud", Mathf.Lerp(_X_Pam_Cloud_Menu, _X_Pam_Cloud_Game, progress));
            _skyBackgroundMat.SetFloat("_Y_Pan_Cloud", Mathf.Lerp(_Y_Pam_Cloud_Menu, _Y_Pam_Cloud_Game, progress));

            timeElasped += Time.deltaTime;
            yield return null;
        }
        _skyBackgroundMat.SetFloat("_X_Pan", _X_Pam_Game);
        _skyBackgroundMat.SetFloat("_X_Pan_Cloud", _X_Pam_Cloud_Game);

        _skyBackgroundMat.SetFloat("_Y_Pan1", _Y_Pam_Game);
        _skyBackgroundMat.SetFloat("_Y_Pan_Cloud", _Y_Pam_Cloud_Game);
    }
}
