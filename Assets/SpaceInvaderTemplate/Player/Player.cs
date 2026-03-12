using PLIbox.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    private const string FIRE_SOUND = "PlayerFire";
    private const string HIT_SOUND = "PlayerHit";
    private const string SHELL_SOUND = "ShellSound";

    private const string VL_DAMAGE_SOUND = "VL_Damage";
    private const string VL_RAFALE_SOUND = "VL_ActiveBonus";
    private const string VL_BEGIN_SOUND = "VL_Begin";
    private const string VL_NEXTWAVE_SOUND = "VL_NextWave";


    private const string FIRE_EFFECT_FEATURE = "FireEffect";
    private const string MOVE_EFFECT_FEATURE = "PlayerMovement";

    [Serializable]
    private struct SmokeEffect
    {
        public ParticleSystem particle;
        public Vector2 leftPosition;
        public Vector2 centerPosition;
        public Vector2 rightPosition;
    }

    [Header("References")]
    [SerializeField] private SpriteRenderer _planeSpriteRenderer;

    [Header("Inputs")]
    [SerializeField] private InputActionReference _moveInput;
    [SerializeField] private InputActionReference _shootInput;
    [SerializeField] private InputActionReference _rafaleRightInput;
    [SerializeField] private InputActionReference _rafaleLeftInput;

    [Header("Sprites")]
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _leftSprite;
    [SerializeField] private Sprite _rightSprite;

    [Header("Properties")]
    [SerializeField] private float deadzone = 0.3f;
    [SerializeField] private float speed = 1f;

    [SerializeField] private Bullet bulletPrefab = null;
    [SerializeField] private Bullet rafaleBulletPrefab = null;
    [SerializeField] private Transform shootAt = null;
    [SerializeField] private float shootCooldown = 1f;
    [SerializeField] private string collideWithTag = "Untagged";
    
    [SerializeField] private int maxLifeAmount = 4;
    private int _currentLife = 4;

    [Header("Rafale")]
    //Time before input is reinitialize
    [SerializeField] private float inputActivationTime = 0.2f;
    [SerializeField] private float rafaleMinimalCharge = 25f;
    [SerializeField] private float rafaleMaximalCharge = 100f;
    [SerializeField] private float rafaleTimeMultiplier = 0.2f;
    [SerializeField] private float rafaleMaxChargeTimeBoost = 5f;
    // Time after which Enemy Death 
    [SerializeField] private float timeBeforeChargeLost = 3f;
    //Amount of Charge won by killing an Invader
    [SerializeField] private float invaderDeathChargeAmount = 5f;
    [SerializeField] private float lostChargePerSeconds = 5f;
    //Time between each bullets during rafale
    [SerializeField] private float rafaleBulletCooldown = 0.05f;
    //X Offset of bullets velocity
    [SerializeField] private float rafaleBulletXOffset = 1f;
    [SerializeField] private AudioSource rafaleLoopAudioSource;
    private float _rafaleCharge = 0f;
    private bool _hasRafaleMaxBoost = false;

    [Header("Sound")]
    [SerializeField] private float _minShellDefferedTime;
    [SerializeField] private float _maxShellDefferedTime;

    [Header("VFX")]
    [SerializeField] private List<ParticleSystem> _fireParticles;
    [SerializeField] private List<ParticleSystem> _rafaleFireParticles;
    [SerializeField] private GameObject _bulletShellPrefab;

    [Header("Lean movement")]
    [SerializeField] private AnimationCurve _leanCurve;
    [SerializeField] private float _leanSpeed;
    [SerializeField] private float _leanMaxAngle;

    [Header("Player Ascending")]
    [SerializeField] private AnimationCurve _playerAscendingCurve;
    [SerializeField] private Vector2 _startAscendingPosition;
    [SerializeField] private float _playerAscendingDuration;

    [Header("Audio")]
    [SerializeField] private AudioMixerGroup _audioMixer;
    [SerializeField] private float _rafaleVolume;
    [SerializeField] private string _exposedMusic;
    [SerializeField] private string _exposedBullet;

    [Header("VFX")]
    [SerializeField] private List<SmokeEffect> _smokeParticles;
    [SerializeField] private float _hitShakeDuration;
    [SerializeField] private GameObject _explostionEffect;

    public static event Action<float /*RafaleAmount*/> OnRafaleChargeChanged;
    public static event Action<float /*RafaleDuration*/, float /*Intensity*/> OnRafaleTriggered;
    public static event Action<int> OnUpdateHealth;

    private bool _isShooting = false;
    private float lastShootTimestamp = Mathf.NegativeInfinity;
    private bool _isRafaleRightPressed = false;
    private bool _isRafaleLeftPressed = false;
    private bool _isInRafale = false;
    private float _lastTimeEnemyHit = 0;

    private float currentLean = 0f;
    private bool _controlsBinded = false;

    private AudioSource _lastUsedVLSource;
    private Vector2 _defaultLocation;

    private void Start()
    {
        ResetPlayer();
        Invader.OnInvaderTookDamage += OnInvaderHit;
        Wave.OnNextWave += PlayNextWaveSound;
        _defaultLocation = transform.position;

        AudioManager.Instance.PlaySound(VL_BEGIN_SOUND);
        StartCoroutine(PlayerAscendingAnimation());
    }

    private void OnDisable()
    {
        if(_controlsBinded)
            UnbindControls();
        Invader.OnInvaderTookDamage -= OnInvaderHit;
        Wave.OnNextWave -= PlayNextWaveSound;
    }

    private IEnumerator PlayerAscendingAnimation()
    {
        float timeElapsed = 0.0f;
        while (timeElapsed < _playerAscendingDuration)
        {
            float progression = _playerAscendingCurve.Evaluate(timeElapsed / _playerAscendingDuration);
            Vector2 newPlayerPosition = Vector2.Lerp(_startAscendingPosition, _defaultLocation, progression);
            transform.position = new Vector3(newPlayerPosition.x, newPlayerPosition.y, transform.position.z);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = _defaultLocation;
        BindControls();
    }

    private void BindControls()
    {
        _controlsBinded = true;

        _shootInput.action.started += InputShootStarted;
        _shootInput.action.canceled += InputShootCanceled;
        _rafaleLeftInput.action.started += OnRafaleLeftInputStarted;
        _rafaleLeftInput.action.canceled += OnRafaleLeftInputCanceled;
        _rafaleRightInput.action.started += OnRafaleRightInputStarted;
        _rafaleRightInput.action.canceled += OnRafaleRightInputCanceled;
    }

    private void OnRafaleLeftInputStarted(InputAction.CallbackContext context) => StartCoroutine(InputRafaleStarted(context, false));
    private void OnRafaleRightInputStarted(InputAction.CallbackContext context) => StartCoroutine(InputRafaleStarted(context, true));
    private void OnRafaleLeftInputCanceled(InputAction.CallbackContext context) => InputRafaleCanceled(context, false);
    private void OnRafaleRightInputCanceled(InputAction.CallbackContext context) => InputRafaleCanceled(context, true);

    private void UnbindControls()
    {
        _shootInput.action.started -= InputShootStarted;
        _shootInput.action.canceled -= InputShootCanceled;
        _rafaleLeftInput.action.started -= OnRafaleLeftInputStarted;
        _rafaleLeftInput.action.canceled -= OnRafaleLeftInputCanceled;
        _rafaleRightInput.action.started -= OnRafaleRightInputStarted;
        _rafaleRightInput.action.canceled -= OnRafaleRightInputCanceled;
        _controlsBinded = false;
    }
    
    

    private void OnInvaderHit(bool bIsRafaleBullet)
    {
        _lastTimeEnemyHit = Time.time;
        if (bIsRafaleBullet) return;
        _rafaleCharge = Mathf.Clamp(_rafaleCharge + invaderDeathChargeAmount, 0f, rafaleMaximalCharge);
        OnRafaleChargeChanged?.Invoke(_rafaleCharge / rafaleMaximalCharge);
    }

    private void Update()
    {
        float moveSign = UpdateMovement();
        UpdateActions();
        
        //Rafale Charge update
        if (_rafaleCharge > 0.1f && _lastTimeEnemyHit + timeBeforeChargeLost < Time.time)
        {
            _rafaleCharge -=  lostChargePerSeconds * Time.deltaTime;
            OnRafaleChargeChanged?.Invoke(_rafaleCharge / rafaleMaximalCharge);
        }

        //lean movement
        float targetLean = -moveSign * _leanMaxAngle;
        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * _leanSpeed);

        Vector3 rotation = transform.localEulerAngles;
        rotation.z = currentLean;
        transform.localEulerAngles = rotation;

        ChangeSprite(moveSign);
        ChangeSmokePosition(moveSign);
    }

    private void ResetPlayer()
    {
        _currentLife = maxLifeAmount;
        _isRafaleLeftPressed = false;
        _isRafaleRightPressed = false;
        _rafaleCharge = 0;
    }

#region Inputs
    
    private void InputShootStarted(InputAction.CallbackContext context) => _isShooting = true;
    private void InputShootCanceled(InputAction.CallbackContext context) => _isShooting = false;

    private IEnumerator InputRafaleStarted(InputAction.CallbackContext context, bool bIsRight)
    {
        if (bIsRight) _isRafaleRightPressed = true;
        else _isRafaleLeftPressed = true;

        yield return new WaitForSeconds(inputActivationTime);
            
        if (bIsRight) _isRafaleRightPressed = false;
        else _isRafaleLeftPressed = false;
    }

    private void InputRafaleCanceled(InputAction.CallbackContext context, bool bIsRight)
    {
        if (bIsRight) _isRafaleRightPressed = false;
        else _isRafaleLeftPressed = false;
    }
#endregion

    private float UpdateMovement()
    {
        if (!_controlsBinded)
            return 0.0f;

        float move = _moveInput.action.ReadValue<float>();
        if (Mathf.Abs(move) < deadzone) { return 0; }

        move = Mathf.Sign(move);
        float delta = move * speed * Time.deltaTime;
        transform.position = GameManager.Instance.KeepInBounds(transform.position + Vector3.right * delta);
        return move;
    }

    private void ChangeSprite(float dir)
    {
        _planeSpriteRenderer.sprite = _normalSprite;
        if (!GameFeelManager.Instance.IsFeatureActive(MOVE_EFFECT_FEATURE))
            return;

        if (dir < 0)
            _planeSpriteRenderer.sprite = _leftSprite;
        else if (dir > 0)
            _planeSpriteRenderer.sprite = _rightSprite;
    }

    private void ChangeSmokePosition(float dir)
    {
        foreach(var smoke in _smokeParticles)
        {
            if (GameFeelManager.Instance.IsFeatureActive("PlayerHit"))
            {
                if (smoke.particle.isStopped)
                    smoke.particle.Play();
                smoke.particle.gameObject.transform.localPosition = dir > 0 ? smoke.rightPosition : dir < 0 ? smoke.leftPosition : smoke.centerPosition;
            }
            else if (!smoke.particle.isStopped)
                smoke.particle.Stop();
        }
    }

    private void UpdateActions()
    {
        if (_isShooting && !_isInRafale)
        {
            Shoot();
        }

        if (_isRafaleLeftPressed && _isRafaleRightPressed && CanRafale())
        {
            StartCoroutine(Rafale());
        }
    }

    private bool CanRafale()
    {
        return !_isInRafale && _rafaleCharge > rafaleMinimalCharge;
    }

    private void Shoot()
    {
        if (Time.time <= lastShootTimestamp + shootCooldown)
            return;

        Instantiate(bulletPrefab, shootAt.position, Quaternion.identity);
        lastShootTimestamp = Time.time;

        PlayFireEffect();
    }

    private IEnumerator DifferedShellSound()
    {
        yield return new WaitForSeconds(Random.Range(_minShellDefferedTime, _maxShellDefferedTime));
        AudioManager.Instance.PlaySound(SHELL_SOUND);
    }

    private void RafaleShoot()
    {
        Bullet bullet = Instantiate(rafaleBulletPrefab, shootAt.position, Quaternion.identity);
        PlayFireEffect(true);
        bullet.SetCustomStartVelocity(bullet.GetStartVelocity() + new Vector3(Random.Range(-rafaleBulletXOffset,rafaleBulletXOffset), 0, 0));
    }

    private void PlayFireEffect(bool bIsRafale = false)
    {
        if (!GameFeelManager.Instance.IsFeatureActive(bIsRafale ? "RafaleEffect" : FIRE_EFFECT_FEATURE))
            return;

        //Feedback sound
        if (!bIsRafale) AudioManager.Instance.PlaySound(FIRE_SOUND);
        StartCoroutine(DifferedShellSound());

        foreach (var fire in bIsRafale ? _rafaleFireParticles : _fireParticles)
        {
            fire.Play();
        }

        Destroy(Instantiate(_bulletShellPrefab, transform.position + new Vector3(0, 0, -10f), Quaternion.identity), 2);
    }

    private IEnumerator Rafale()
    {
        _isInRafale = true;
        _isRafaleLeftPressed = false;
        _isRafaleRightPressed = false;
        _hasRafaleMaxBoost = Mathf.Approximately(_rafaleCharge, rafaleMaximalCharge);
        rafaleLoopAudioSource.Play();
        
        float rafaleTime = _rafaleCharge * rafaleTimeMultiplier + (_hasRafaleMaxBoost ? rafaleMaxChargeTimeBoost : 0);
        float clock = 0;

        //Sound
        _audioMixer.audioMixer.SetFloat(_exposedMusic, _rafaleVolume);
        _audioMixer.audioMixer.SetFloat(_exposedBullet, _rafaleVolume);

        //Haptic 
        if (GameFeelManager.Instance.IsFeatureActive("RafaleEffect"))
        {
            HapticManager.Instance.StartRumble(100, 200, rafaleTime);
            CameraShake.Instance.StartShaking(rafaleTime);
            AudioManager.Instance.PlaySound(VL_RAFALE_SOUND);
        }

        OnRafaleTriggered?.Invoke(rafaleTime, _rafaleCharge / rafaleMaximalCharge);
        
        //Reset Rafale Charge 
        _rafaleCharge = 0;
        OnRafaleChargeChanged?.Invoke(_rafaleCharge / rafaleMaximalCharge);
        
        Debug.Log("Start Rafale");
        while (clock < rafaleTime)
        {
            RafaleShoot();
            yield return new WaitForSeconds(rafaleBulletCooldown);
            clock += rafaleBulletCooldown;
        }
        Debug.Log("Stop Rafale");
        _isInRafale = false;
        rafaleLoopAudioSource.Stop();

        //Reset sound
        _audioMixer.audioMixer.SetFloat(_exposedMusic, 0);
        _audioMixer.audioMixer.SetFloat(_exposedBullet, 0);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag(collideWithTag)) { return; }
        
        TakeDamage();
        Destroy(collision.gameObject);
    }
    
    public event Action OnTakeDamage;
    public event Action OnPlayerDeath;


    private void TakeDamage()
    {
        _currentLife -= 1;
        if (_currentLife <= 0)
        {
            GameManager.Instance.PlayGameOver();

            //Feedback
            _planeSpriteRenderer.enabled = false;
            if (GameFeelManager.Instance.IsFeatureActive("PlayerHit"))
                _explostionEffect.SetActive(true);

            UnbindControls();
            OnPlayerDeath?.Invoke();
            return;
        }


        OnTakeDamage?.Invoke();
        OnUpdateHealth?.Invoke(_currentLife);

        //Feedback son

        if(GameFeelManager.Instance.IsFeatureActive("PlayerHit"))
        {
            AudioManager.Instance.PlaySound(HIT_SOUND);
            AudioManager.Instance.PlaySound(VL_DAMAGE_SOUND);
            CameraShake.Instance.StartShaking(_hitShakeDuration);
        }

        //Feedback smoke
        if (_currentLife > 0)
            _smokeParticles[_currentLife - 1].particle.gameObject.SetActive(true);
    }

    private void PlayNextWaveSound()
    {
        AudioManager.Instance.PlaySound(VL_NEXTWAVE_SOUND);
    }
}
