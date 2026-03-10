using PLIbox.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    private const string FIRE_SOUND = "PlayerFire";
    private const string HIT_SOUND = "PlayerHit";
    private const string SHELL_SOUND = "ShellSound";

    [Header("Inputs")]
    [SerializeField] private InputActionReference _moveInput;
    [SerializeField] private InputActionReference _shootInput;
    [SerializeField] private InputActionReference _rafaleRightInput;
    [SerializeField] private InputActionReference _rafaleLeftInput;

    [Header("Properties")]
    [SerializeField] private float deadzone = 0.3f;
    [SerializeField] private float speed = 1f;

    [SerializeField] private Bullet bulletPrefab = null;
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
    private float _rafaleCharge = 0f;
    private bool _hasRafaleMaxBoost = false;

    [Header("Sound")]
    [SerializeField] private float _minShellDefferedTime;
    [SerializeField] private float _maxShellDefferedTime;

    [Header("VFX")]
    [SerializeField] private List<ParticleSystem> _fireParticles;

    [Header("Lean movement")]
    [SerializeField] private AnimationCurve _leanCurve;
    [SerializeField] private float _leanSpeed;
    [SerializeField] private float _leanMaxAngle;

    public static event Action<float /*RafaleAmount*/> OnRafaleChargeChanged;
    public static event Action<float /*RafaleDuration*/> OnRafaleTriggered;
    
    private bool _isShooting = false;
    private float lastShootTimestamp = Mathf.NegativeInfinity;
    private bool _isRafaleRightPressed = false;
    private bool _isRafaleLeftPressed = false;
    private bool _isInRafale = false;

    private float currentLean = 0f;

    private float _lastTimeKilledEnemy = 0;

    private void OnEnable()
    {
        _shootInput.action.started += InputShootStarted;
        _shootInput.action.canceled += InputShootCanceled;
        _rafaleLeftInput.action.started += context => { StartCoroutine(InputRafaleStarted(context, false)); };
        _rafaleLeftInput.action.canceled += context => { InputRafaleCanceled(context, false); };
        _rafaleRightInput.action.started += context => { StartCoroutine(InputRafaleStarted(context, true)); };
        _rafaleRightInput.action.canceled += context => { InputRafaleCanceled(context, true); };
    }

    private void OnDisable()
    {
        _shootInput.action.started -= InputShootStarted;
        _shootInput.action.canceled -= InputShootCanceled;
    }

    private void Start()
    {
        ResetPlayer();
        Wave.OnInvaderDeath += OnInvaderDeath;
    }

    private void OnInvaderDeath()
    {
        _lastTimeKilledEnemy = Time.time;
        if (_isInRafale) return;
        _rafaleCharge = Mathf.Clamp(_rafaleCharge + invaderDeathChargeAmount, 0f, rafaleMaximalCharge);
        OnRafaleChargeChanged?.Invoke(_rafaleCharge / rafaleMaximalCharge);
    }

    private void Update()
    {
        float moveSign = UpdateMovement();
        UpdateActions();
        
        //Rafale Charge update
        if (_rafaleCharge > 0.1f && _lastTimeKilledEnemy + timeBeforeChargeLost < Time.time)
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
        float move = _moveInput.action.ReadValue<float>();
        if (Mathf.Abs(move) < deadzone) { return 0; }

        move = Mathf.Sign(move);
        float delta = move * speed * Time.deltaTime;
        transform.position = GameManager.Instance.KeepInBounds(transform.position + Vector3.right * delta);
        return move;
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
        Bullet bullet = Instantiate(bulletPrefab, shootAt.position, Quaternion.identity);
        PlayFireEffect();
        bullet.SetCustomStartVelocity(bullet.GetStartVelocity() + new Vector3(UnityEngine.Random.Range(-rafaleBulletXOffset,rafaleBulletXOffset), 0, 0));
    }

    private void PlayFireEffect()
    {
        //Feedback sound
        AudioManager.Instance.PlaySound(FIRE_SOUND);
        StartCoroutine(DifferedShellSound());

        foreach (var fire in _fireParticles)
        {
            fire.Play();
        }
    }

    private IEnumerator Rafale()
    {
        _isInRafale = true;
        _isRafaleLeftPressed = false;
        _isRafaleRightPressed = false;
        _hasRafaleMaxBoost = Mathf.Approximately(_rafaleCharge, rafaleMaximalCharge);
        
        float rafaleTime = _rafaleCharge * rafaleTimeMultiplier + (_hasRafaleMaxBoost ? rafaleMaxChargeTimeBoost : 0);
        float clock = 0;

        //Haptic 
        HapticManager.Instance.StartRumble(100, 200, rafaleTime);
        CameraShake.Instance.StartShaking(rafaleTime);
        
        OnRafaleTriggered?.Invoke(rafaleTime);
        
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
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag(collideWithTag)) { return; }

        TakeDamage();
    }
    
    public event Action OnTakeDamage;
    public event Action OnPlayerDeath;


    private void TakeDamage()
    {
        _currentLife -= 1;
        if (_currentLife <= 0)
        {
            GameManager.Instance.PlayGameOver();
            OnPlayerDeath?.Invoke();
            return;
        }

        OnTakeDamage?.Invoke();
        //Feedback son
        AudioManager.Instance.PlaySound(HIT_SOUND);
    }
}
