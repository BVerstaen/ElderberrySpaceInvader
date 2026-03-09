using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameFeelManager : MonoBehaviour
{
    public static GameFeelManager Instance = null;

    [SerializeField] private GameFeelFeature[] _gamefeelFeatures;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        for (int i = 0; i < _gamefeelFeatures.Length; i++)
        {
            if (Keyboard.current[_gamefeelFeatures[i].ActivationKey].wasPressedThisFrame)
            {
                _gamefeelFeatures[i].IsActive = !_gamefeelFeatures[i].IsActive;
                Debug.Log((_gamefeelFeatures[i].IsActive ? "Activate" : "Deactivate") + _gamefeelFeatures[i].Name);
            }
        }
    }

    public bool IsFeatureActive(string name)
    {
        foreach (GameFeelFeature feature in _gamefeelFeatures)
        {
            if (feature.Name == name) return feature.IsActive;
        }

        return false;
    }
    
    [Serializable]
    struct GameFeelFeature
    {
        public bool IsActive;
        public string Name;
        public Key ActivationKey;
    }
}
