using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Brique : MonoBehaviour
{
    private Collider _collider;

    [SerializeField] private float _maxHealth;
    private float _currentHealth;
    public float CurrentHealth
    {
        get => _currentHealth;
        private set
        {
            _currentHealth = Mathf.Max(0, Mathf.Min(value, _maxHealth));
            if (_currentHealth <= 0.0f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        CurrentHealth = _maxHealth;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Balle")) return;

        if (!GameManager.Instance) return;

        CurrentHealth -= GameManager.Instance.damageToBrique;
    }
}