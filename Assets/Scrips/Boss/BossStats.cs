using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;


public class BossStats : MonoBehaviour
{
    private bool _bStunned;
    private bool _bAttacking;
    private bool _bPatrolling;
    private bool _bPlayerOnHighland;
    [SerializeField] private float _Health;
    [SerializeField] private float _Damage;
    [SerializeField] private Vector3 _LeftCorner;
    [SerializeField] private Vector3 _RightCorner;
    [SerializeField] private Rigidbody2D rb;
    
}
