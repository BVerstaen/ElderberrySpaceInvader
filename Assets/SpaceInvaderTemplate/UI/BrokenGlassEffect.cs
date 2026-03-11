using System;
using System.Collections.Generic;
using UnityEngine;

public class BrokenGlassEffect : MonoBehaviour
{
    [Serializable]
    private struct BrokenGlassHealth
    {
        public Texture2D Tex;
        public float Power;
    }

    [SerializeField] private Material _brokenGlassMat;
    [SerializeField] private Texture2D _baseTex;
    [SerializeField] private float _basePower;

    [SerializeField] private List<BrokenGlassHealth> _brokenGlassPalier;

    private void Awake()
    {
        _brokenGlassMat.SetTexture("_BrokenGlass", _baseTex);
        _brokenGlassMat.SetFloat("_Power", _basePower);
    }

    private void OnEnable()
    {
        Player.OnUpdateHealth += CheckNewGlass;
    }

    private void OnDisable()
    {
        Player.OnUpdateHealth -= CheckNewGlass;

        //Reset
        _brokenGlassMat.SetTexture("_BrokenGlass", _baseTex);
        _brokenGlassMat.SetFloat("_Power", _basePower);
    }

    private void CheckNewGlass(int health)
    {
        if (health >= _brokenGlassPalier.Count)
            throw new Exception("Error health");

        ChangeMaterial(_brokenGlassPalier[health]);
    }

    private void ChangeMaterial(BrokenGlassHealth data)
    {
        _brokenGlassMat.SetTexture("_BrokenGlass", data.Tex);
        _brokenGlassMat.SetFloat("_Power", data.Power);
    }
}
