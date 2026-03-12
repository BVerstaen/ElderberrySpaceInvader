using System;
using System.Collections.Generic;
using UnityEngine;

public class BrokenGlassEffect : MonoBehaviour
{
    [Serializable]
    private struct BrokenGlassHealth
    {
        public Texture2D Tex;
        public Texture2D TexDraw;
        public float Power;

        public BrokenGlassHealth(Texture2D texture, Texture2D textureDraw, float power)
        {
            Tex = texture;
            TexDraw = textureDraw;
            Power = power;
        }
    }

    [SerializeField] private Material _brokenGlassMat;
    [SerializeField] private Texture2D _baseTex;
    [SerializeField] private Texture2D _baseTexDraw;
    [SerializeField] private float _basePower;

    [SerializeField] private List<BrokenGlassHealth> _brokenGlassPalier;

    private int _currentHealth = 4;

    private void Awake()
    {
        _brokenGlassPalier.Add(new BrokenGlassHealth(_baseTex, _baseTexDraw, _basePower));
        SetTobaseEffect();
    }

    private void OnEnable()
    {
        Player.OnUpdateHealth += CheckNewGlass;
        GameFeelManager.Instance.OnFeatureToggled += ToggleBrokenGlass;
    }

    private void OnDisable()
    {
        Player.OnUpdateHealth -= CheckNewGlass;
        if (GameFeelManager.Instance)
            GameFeelManager.Instance.OnFeatureToggled -= ToggleBrokenGlass;

        SetTobaseEffect();
    }

    private void SetTobaseEffect()
    {
        //Reset
        _brokenGlassMat.SetTexture("_BrokenGlass", _baseTex);
        _brokenGlassMat.SetTexture("_BrokenGlass_Draw", _baseTexDraw);
        _brokenGlassMat.SetFloat("_Power", _basePower);
    }

    private void ToggleBrokenGlass(string feature, bool toggle)
    {
        if(feature == "PlayerHit")
        {
            if (toggle)
                ChangeMaterial(_brokenGlassPalier[_currentHealth]);
            else
                SetTobaseEffect();
        }
    }

    private void CheckNewGlass(int health)
    {
        if (health >= _brokenGlassPalier.Count)
            throw new Exception("Error health");

        _currentHealth = health;
        ChangeMaterial(_brokenGlassPalier[health]);
    }

    private void ChangeMaterial(BrokenGlassHealth data)
    {
        _brokenGlassMat.SetTexture("_BrokenGlass", data.Tex);
        _brokenGlassMat.SetTexture("_BrokenGlass_Draw", data.TexDraw);
        _brokenGlassMat.SetFloat("_Power", data.Power);
    }
}
