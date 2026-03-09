using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Bullet bulletPrefab = null;
    [SerializeField] private Transform shootAt = null;
    [SerializeField] private string collideWithTag = "Player";

    [Header("Parameters")]
    [SerializeField] private int _score;
    [SerializeField] private int maxLifeAmount = 4;
    private int _currentLifeAmount;
    
    internal Action<Invader> onDestroy;

    public Vector2Int GridIndex { get; private set; }

    public void Initialize(Vector2Int gridIndex)
    {
        this.GridIndex = gridIndex;
        //TODO: Add Multiplier for Waves
        _currentLifeAmount =  maxLifeAmount; 
    }

    public void OnDestroy()
    {
        onDestroy?.Invoke(this);
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
        GameManager.Instance.AddScore(_score);
        Destroy(gameObject);
    }

    public void Shoot()
    {
        Instantiate(bulletPrefab, shootAt.position, Quaternion.identity);
    }
}
