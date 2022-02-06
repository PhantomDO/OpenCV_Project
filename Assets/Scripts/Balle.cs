using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// Fonction :
///     - Rebondit
///     - Inflige des dégats lors d'une collision avec une brique
///     - Elle se colle sur la palette au départ, rebondit sinon
/// </summary>
[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Balle : MonoBehaviour
{
    public delegate void BalleDestroy();
    public static event BalleDestroy OnBallDestroy;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float maxMoveSpeed = 15f;

    private float _multiplierMoveSpeed = 1.5f;

    private Collider _collider;
    private Rigidbody _rigidbody;

    private Vector3 _normal = Vector3.zero;
    private Vector3 _reflect = Vector3.zero;

    public bool IsLaunch { get; private set; } = false;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Death"))
        {
            Destroy(gameObject);
            return;
        }

        _normal = collision.GetContact(0).normal;
        _reflect = Rebond(transform.forward, _normal);
        _reflect.y = 0;
    }

    private void OnDestroy()
    {
        OnBallDestroy?.Invoke();
    }

    private void OnDrawGizmos()
    {
        if (!_rigidbody) return;

        var tempPosition = _rigidbody.position + transform.forward * 10f;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(_rigidbody.position, tempPosition);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(tempPosition, tempPosition + _normal * 10f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(tempPosition, _rigidbody.position + _reflect * 10f);
    }

    private Vector3 Rebond(Vector3 dir, Vector3 normal)
    {
        //_rigidbody.velocity = /*_rigidbody.angularVelocity =*/ Vector3.zero;
        var reflect = Vector3.Reflect(dir, normal); 
        transform.rotation = Quaternion.LookRotation(reflect, Vector3.up);
        moveSpeed = Mathf.Min(moveSpeed * _multiplierMoveSpeed, maxMoveSpeed);
        _multiplierMoveSpeed *= _multiplierMoveSpeed;
        return reflect;
    }

    public void Launch()
    {
        if (!IsLaunch) IsLaunch = true;
        StartCoroutine(MoveUntilCollision());
    }

    private IEnumerator MoveUntilCollision()
    {
        while (IsLaunch)
        {
            var newPosition = transform.position + transform.forward;
            var step = moveSpeed * Time.fixedDeltaTime;
            transform.position = Vector3.MoveTowards(transform.position, newPosition, step);

            yield return new WaitForFixedUpdate();
        }
    }
}
