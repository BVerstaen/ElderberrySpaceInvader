using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
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

    private float lastShootTimestamp = Mathf.NegativeInfinity;

    private void OnEnable()
    {
        _shootInput.action.performed += InputShoot;
    }

    private void OnDisable()
    {
        _shootInput.action.performed -= InputShoot;
    }

    void Update()
    {
        UpdateMovement();
    }

    void UpdateMovement()
    {
        
        float move = _moveInput.action.ReadValue<float>();
        if (Mathf.Abs(move) < deadzone) { return; }

        move = Mathf.Sign(move);
        float delta = move * speed * Time.deltaTime;
        transform.position = GameManager.Instance.KeepInBounds(transform.position + Vector3.right * delta);
    }

    private void InputShoot(InputAction.CallbackContext context) => Shoot();
    void Shoot()
    {
        if (Time.time <= lastShootTimestamp + shootCooldown)
            return;

        Instantiate(bulletPrefab, shootAt.position, Quaternion.identity);
        lastShootTimestamp = Time.time;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag != collideWithTag) { return; }

        GameManager.Instance.PlayGameOver();
    }
}
