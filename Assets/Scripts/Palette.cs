using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Palette : MonoBehaviour
{
    [field: SerializeField] public Transform BalleStart { get; private set; }

    [SerializeField] private Balle ballePrefab;
    [SerializeField] private float moveSpeed = 5f;

    private Collider _collider;
    private Rigidbody _rigidbody;

    private bool _isColliding = false;
    private float _horizontalAxis;
    private float _verticalAxis;

    private Collider _normalCollider;
    private Vector3 _normalCollision;

    public Balle Balle { get; private set; }

    private void OnEnable()
    {
        GameManager.OnInstantiateBall += InstantiateBall;
    }

    private void OnDisable()
    {
        GameManager.OnInstantiateBall -= InstantiateBall;
    }

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        if (TryGetComponent(out _rigidbody))
        {
            _rigidbody.useGravity = false;
        }
    }

    private void Update()
    {
        if (!Balle) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Balle.transform.parent = null;
            Balle.Launch();
        }

        _horizontalAxis = Input.GetAxis("Horizontal");
        var dot = Vector3.Dot(_normalCollision, transform.right * _horizontalAxis);
        
        if (!_isColliding || (_isColliding && dot > 0.0f))
        {
            transform.position = Vector3.MoveTowards(transform.position,
                transform.position + transform.right * _horizontalAxis,
                moveSpeed * Time.fixedDeltaTime);
        }
        
        _verticalAxis = Input.GetAxis("Vertical");

        if (!Balle.IsLaunch)
        {
            // rotate ball
            Balle.transform.Rotate(Balle.transform.up, 45f * _verticalAxis * Time.fixedDeltaTime);
        }
    }

    private void OnDrawGizmos()
    {
        if (_normalCollider == null) return;

        var pos = _normalCollider.bounds.ClosestPoint(transform.position);
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(pos, _normalCollision);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right * _horizontalAxis);
    }

    private void OnCollisionExit(Collision other)
    {
        _isColliding = false;
    }

    private void OnCollisionStay(Collision collisionInfo)
    {
        _rigidbody.velocity = Vector3.zero;
        
        if (collisionInfo == null)
        {
            _isColliding = false;
            //_normalCollider = null;
            return;
        }

        _normalCollider = collisionInfo.collider;

        Debug.Log($"{collisionInfo.gameObject.name}");

        _isColliding = collisionInfo.gameObject.CompareTag("Wall");
        _normalCollision = _isColliding ? collisionInfo.GetContact(0).normal : Vector3.zero;
    }

    private void InstantiateBall()
    {
        Balle = GameObject.Instantiate(ballePrefab, BalleStart.position, Quaternion.identity, BalleStart);
    }
}
