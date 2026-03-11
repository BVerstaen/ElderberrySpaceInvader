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
    private Rigidbody2D _rb;

    // Start is called before the first frame update
    void Awake()
    {
        _rb= GetComponent<Rigidbody2D>();
        _rb.linearVelocity = startVelocity;
    }

    private void Start()
    {
        Destroy(gameObject, _lifeTime);

        if(_trail)
            _trail.enabled = GameFeelManager.Instance.IsFeatureActive(_featureName);
    }
    
    private void OnEnable()
    {
        GameFeelManager.Instance.OnFeatureToggled += ToggleTrail;
    }

    private void OnDisable()
    {
        GameFeelManager.Instance.OnFeatureToggled -= ToggleTrail;
    }

    public Vector3 GetStartVelocity() => startVelocity;

    public void SetCustomStartVelocity(Vector3 velocity)
    {
        startVelocity = velocity;
        _rb.linearVelocity = startVelocity;
    }

    private void ToggleTrail(string name, bool toggle)
    {
        if(_featureName == name && _trail)
        {
            _trail.enabled = toggle;
        }
    }
}
