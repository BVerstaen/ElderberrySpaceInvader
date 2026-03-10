using PLIbox.Audio;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private const string FIRE_SOUND = "PlayerFire";

    [Header("Inputs")]
    [SerializeField] private InputActionReference _moveInput;
    [SerializeField] private InputActionReference _shootInput;

    [Header("Properties")]
    [SerializeField] private float deadzone = 0.3f;
    [SerializeField] private float speed = 1f;

    [SerializeField] private Bullet bulletPrefab = null;
    [SerializeField] private Transform shootAt = null;
    [SerializeField] private float shootCooldown = 1f;
    [SerializeField] private string collideWithTag = "Untagged";
    
    [SerializeField] private int maxLifeAmount = 4;
    private int _currentLife = 4;

    private bool _isShooting = false;
    private float lastShootTimestamp = Mathf.NegativeInfinity;

    private void OnEnable()
    {
        _shootInput.action.started += InputShootStarted;
        _shootInput.action.canceled += InputShootCanceled;
    }

    private void OnDisable()
    {
        _shootInput.action.started -= InputShootStarted;
        _shootInput.action.canceled -= InputShootCanceled;
    }

    private void Start()
    {
        ResetLife();
    }

    private void Update()
    {
        UpdateMovement();
        UpdateActions();
    }

    private void ResetLife()
    {
        _currentLife = maxLifeAmount;
    }

    private void InputShootStarted(InputAction.CallbackContext context) => _isShooting = true;
    private void InputShootCanceled(InputAction.CallbackContext context) => _isShooting = false;

    private void UpdateMovement()
    {
        
        float move = _moveInput.action.ReadValue<float>();
        if (Mathf.Abs(move) < deadzone) { return; }

        move = Mathf.Sign(move);
        float delta = move * speed * Time.deltaTime;
        transform.position = GameManager.Instance.KeepInBounds(transform.position + Vector3.right * delta);
    }

    private void UpdateActions()
    {
        if (_isShooting)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (Time.time <= lastShootTimestamp + shootCooldown)
            return;

        Instantiate(bulletPrefab, shootAt.position, Quaternion.identity);
        lastShootTimestamp = Time.time;

        //Feedback sound
        AudioManager.Instance.PlaySound(FIRE_SOUND);
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
        }
        OnTakeDamage?.Invoke();
    }
}
