using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class GameManager : Tools.MonoSingleton<GameManager>
{
    public delegate void InstantiateBall();
    public static event InstantiateBall OnInstantiateBall;

    [field: SerializeField] public uint maxBall { get; private set; } = 3;
    [field: SerializeField] public float damageToBrique { get; private set; } = 1;
    
    [SerializeField] private Vector2Int gridBrique;
    [SerializeField] private Vector2 offsetBrique;
    
    [SerializeField] private Transform startGrid;
    [SerializeField] private Brique briquePrefab;
    private List<Brique> _instanceBrique = new List<Brique>();

    private int _currentBallLeft;

    private void OnEnable()
    {
        Balle.OnBallDestroy += BallDestroy;
    }

    private void OnDisable()
    {
        Balle.OnBallDestroy -= BallDestroy;
    }

    private void BallDestroy()
    {
        _currentBallLeft--;
        if (_currentBallLeft > 0)
        {
            OnInstantiateBall?.Invoke();
        }
    }

    private void Start()
    {
        _currentBallLeft = (int)maxBall;
        // Init grid
        for (int y = 0; y < gridBrique.y; y++)
        {
            for (int x = 0; x < gridBrique.x; x++)
            {
                var go = GameObject.Instantiate(briquePrefab, startGrid.position, Quaternion.identity, transform);
                var scale = go.transform.localScale;
                var position = startGrid.position + new Vector3(scale.x * x, 0, -scale.z * y) + new Vector3(offsetBrique.x * x, 0, -offsetBrique.y * y);
                go.transform.position = position;
                _instanceBrique.Add(go);
            }
        }
        
        OnInstantiateBall?.Invoke();
    }
}
