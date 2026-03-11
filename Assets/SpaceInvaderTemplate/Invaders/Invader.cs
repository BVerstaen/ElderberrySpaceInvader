using PLIbox.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Invader : MonoBehaviour
{
    private const string INVADER_KILL_SOUND = "InvaderDeath";
    private const string INVADER_DESTROY_FEATURE = "EnnemyExplosion";
    private const string BLOB_FEATURE = "EnnemyBlobAnimation";


    [Header("References")]
    [SerializeField] private Bullet bulletPrefab = null;
    [SerializeField] private Transform shootAt = null;
    [SerializeField] private string collideWithTag = "Player";
    [SerializeField] private GameObject _eyeParticlesPrefab;
    [SerializeField] private SpriteRenderer[] _invaderSprites;

    [Header("Eye rotation")]
    [SerializeField] private List<Transform> _invaderEye;
    [SerializeField] private float _eyeAmplitude;
    [SerializeField] private float _eyeSpeed;

    [Header("Parameters")]
    [SerializeField] private int _score;

    [Header("Random movement")]
    [SerializeField] private float _angleSpeed = 1f;
    [SerializeField] private float _radiusVariation = 1f;
    [Space(10)]
    [SerializeField] private float _noiseScale = 1f;

    [Header("Ennemy blob Effect")]
    [SerializeField] private SpriteRenderer _blobSprite;
    [SerializeField] private Material _spriteMat;
    [SerializeField] private Material _blobMat;

    private List<float> _eyeOffset = new List<float>();

    private int _currentLifeAmount;

    private int _waveIndex;
    private Vector2 _centerPoint;
    private float _angle;

    internal Action<Invader> onDestroy;

    public Vector2Int GridIndex { get; private set; }

    private void Awake()
    {
        foreach (Transform t in _invaderEye)
            _eyeOffset.Add(Random.Range(-100, 100));
    }

    private void Start()
    {
        _blobSprite.material = _spriteMat;
        if (GameFeelManager.Instance.IsFeatureActive("EnnemyBlobAnimation"))
            _blobSprite.material = _blobMat;
    }

    private void OnEnable()
    {
        GameFeelManager.Instance.OnFeatureToggled += CheckChangeBlobAnim;
    }

    private void OnDisable()
    {
        GameFeelManager.Instance.OnFeatureToggled -= CheckChangeBlobAnim;
    }

    private void CheckChangeBlobAnim(string name, bool active)
    {
        if (name == BLOB_FEATURE)
        {
            _blobSprite.material = active ? _blobMat : _spriteMat;
        }
    }

    public void Initialize(Vector2Int gridIndex, int health, int index)
    {
        this.GridIndex = gridIndex;
        _currentLifeAmount = health;

        _waveIndex = index;
        _centerPoint = transform.localPosition;
    }

    private void Update()
    {
        if (GameFeelManager.Instance.IsFeatureActive("EnnemyBlobAnimation"))
        {
            //Ennemy random movement
            _angle += _angleSpeed * Time.deltaTime;

            float noiseTime = Time.time * _noiseScale + _waveIndex;
            float noisyRadius = Mathf.PerlinNoise(noiseTime, 0f) * _radiusVariation;

            float x = _centerPoint.x + Mathf.Cos(_angle) * noisyRadius;
            float y = _centerPoint.y + Mathf.Sin(_angle) * noisyRadius;

            transform.localPosition = new Vector3(x, y, transform.localPosition.z);

            //Eye rotation
            for (int i = 0; i < _invaderEye.Count; i++)
            {
                Vector3 newRotation = _invaderEye[i].localEulerAngles;
                newRotation.z = Mathf.Sin((Time.time + _eyeOffset[i]) * _eyeSpeed) * _eyeAmplitude;
                _invaderEye[i].localEulerAngles = newRotation;
            }
        }
        else
            transform.localPosition = _centerPoint;
    }

    public void OnDeath()
    {
        onDestroy?.Invoke(this);

        if (GameFeelManager.Instance.IsFeatureActive(INVADER_DESTROY_FEATURE))
        {
            Destroy(Instantiate(_eyeParticlesPrefab, transform.position, Quaternion.identity), 2.0f);
        }

        Destroy(gameObject);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag != collideWithTag) { return; }

        TakeDamage();
        Destroy(collision.gameObject);
    }

    public event Action OnTakeDamage;
    public static event Action OnInvaderTookDamage;

    private void TakeDamage()
    {
        _currentLifeAmount--;
        OnTakeDamage?.Invoke();
        OnInvaderTookDamage?.Invoke();
        StartCoroutine(DamageColorFeedback());
        if (_currentLifeAmount <= 0)
        {
            Kill();
        }
    }

    private IEnumerator DamageColorFeedback()
    {
        SwitchSpritesColor(Color.darkRed);
        yield return new WaitForSeconds(0.1f);
        SwitchSpritesColor(Color.white);
        yield return new WaitForSeconds(0.1f);
        SwitchSpritesColor(Color.darkRed);
        yield return new WaitForSeconds(0.1f);
        SwitchSpritesColor(Color.white);
    }

    private void SwitchSpritesColor(Color color)
    {
        foreach (SpriteRenderer sprite in _invaderSprites)
        {
            sprite.color = color;
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
