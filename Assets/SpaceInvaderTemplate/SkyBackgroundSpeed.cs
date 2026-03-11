using System;
using UnityEngine;

public class SkyBackgroundSpeed : MonoBehaviour
{
    [SerializeField] private Material _skyBackgroundMat;

    [Header("Menu")]
    [SerializeField] private float _N_X_Pam_Menu;
    [SerializeField] private float _Texture2_N_X_Pam_Menu;

    [Header("Game")]
    [SerializeField] private float _N_X_Pam_Game;
    [SerializeField] private float _Texture2_N_X_Pam_Game;

    private void Awake()
    {
        ChangeSpeed(_N_X_Pam_Menu, _Texture2_N_X_Pam_Menu);
    }

    private void OnEnable()
    {
        GameManager.Instance.OnStartGame += GoToGame;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnStartGame -= GoToGame;
    }

    private void GoToGame()
    {
        ChangeSpeed(_N_X_Pam_Game, _Texture2_N_X_Pam_Game);

    }

    private void ChangeSpeed(float N_X, float texture2)
    {
        _skyBackgroundMat.SetFloat("_N_X_Pam", N_X);
        _skyBackgroundMat.SetFloat("_Texture2_N_X_Pam", texture2);
    }
}
