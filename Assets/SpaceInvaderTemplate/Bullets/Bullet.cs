using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Vector3 startVelocity;
    [SerializeField] private float _lifeTime;

    [Header("Trail")]
    [SerializeField] private string _featureName;
    [SerializeField] private TrailRenderer _trail;

    // Start is called before the first frame update
    void Awake()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = startVelocity;
    }

    private void Start()
    {
        Destroy(gameObject, _lifeTime);
    }

    private void OnEnable()
    {
        GameFeelManager.Instance.OnFeatureToggled += ToggleTrail;
    }

    private void OnDisable()
    {
        GameFeelManager.Instance.OnFeatureToggled -= ToggleTrail;
    }

    private void ToggleTrail(string name, bool toggle)
    {
        if(_featureName == name)
        {
            _trail.enabled = toggle;
        }
    }
}
