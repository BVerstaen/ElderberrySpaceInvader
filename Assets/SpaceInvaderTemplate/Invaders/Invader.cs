using PLIbox.Audio;
using System;
using System.Reflection;
using UnityEngine;

public class Invader : MonoBehaviour
{
    private const string INVADER_KILL_SOUND = "InvaderDeath";

    [Header("References")]
    [SerializeField] private Bullet bulletPrefab = null;
    [SerializeField] private Transform shootAt = null;
    [SerializeField] private string collideWithTag = "Player";
    [SerializeField] private GameObject _eyeParticlesPrefab;

    [Header("Parameters")]
    [SerializeField] private int _score;

    [Header("Random movement")]
    [SerializeField] private float _angleSpeed = 1f;
    [SerializeField] private float _radiusVariation = 1f;
    [Space(10)]
    [SerializeField] private float _noiseScale = 1f;

    private int _currentLifeAmount;

    private int _waveIndex;
    private Vector2 _centerPoint;
    private float _angle;

    internal Action<Invader> onDestroy;

    public Vector2Int GridIndex { get; private set; }

    public void Initialize(Vector2Int gridIndex, int health, int index)
    {
        this.GridIndex = gridIndex;
        _currentLifeAmount = health;

        _waveIndex = index;
        _centerPoint = transform.localPosition;
    }

    private void Update()
    {
        _angle += _angleSpeed * Time.deltaTime;

        float noiseTime = Time.time * _noiseScale + _waveIndex;
        float noisyRadius = Mathf.PerlinNoise(noiseTime, 0f) * _radiusVariation;

        float x = _centerPoint.x + Mathf.Cos(_angle) * noisyRadius;
        float y = _centerPoint.y + Mathf.Sin(_angle) * noisyRadius;

        transform.localPosition = new Vector3(x, y, transform.localPosition.z);
    }

    public void OnDeath()
    {
        onDestroy?.Invoke(this);
        Destroy(Instantiate(_eyeParticlesPrefab, transform.position, Quaternion.identity), 2.0f);


        Destroy(gameObject);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag != collideWithTag) { return; }

        TakeDamage();
        Destroy(collision.gameObject);
    }
    
    public event Action OnTakeDamage;

    private void TakeDamage()
    {
        _currentLifeAmount--;
        OnTakeDamage?.Invoke();
        if (_currentLifeAmount <= 0)
        {
            Kill();
        }
    }

    private void Kill()
    {
        ComboManager.Instance.AddScore();
        AudioManager.Instance.PlaySound(INVADER_KILL_SOUND);
        OnDeath();
    }

    public void Shoot()
    {
        if (shootAt == null)
            return;

        Instantiate(bulletPrefab, shootAt.position, Quaternion.identity);
    }
}
