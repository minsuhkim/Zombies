using System;
using UnityEngine;

public enum EZombieState
{
    Patrol, Chase, Attack, Evade, Damage, Idle, Die,
}

public class ZombieManager : MonoBehaviour
{
    private int zombieHP = 100;

    public int HP
    {
        get
        {
            return zombieHP;
        }
        set
        {
            if (zombieHP < 0)
            {
                zombieHP = 0;
            }
            else
            {
                zombieHP = value;
            }
        }
    }

    public EZombieState currentState = EZombieState.Idle;
    public Transform target;
    public float attackRange = 1.0f;                        // 공격범위
    public float attackDelay = 2.0f;                        // 공격딜레이

    private float nextAttackTime = 0.0f;                    // 다음 공격 시간관리
    public Transform[] patrolPoints;                        // 순찰 경로 지점들
    private int currentPoint = 0;                           // 현재 순찰 경로 지점 인덱스
    public float moveSpeed = 2.0f;
    private float trackingRange = 3.0f;                     // 추적 범위 설정
    private bool isAttack = false;                          // 공격 상태
    private float evadeRange = 5.0f;                        // 도망 상태 회피 거리
    //private float zombieHP =10;
    private float distanceToTarget;                         // target과의 거리 계산값
    private bool isWaiting = false;                         // 상태 전환 후 대기 상태 여부
    public float idleTime = 2.0f;                           // 각 상태 전환 후 대기 시간

    private void Update()
    {
        //target = GameObject.FindGameObjectWithTag("Player").transform;
        distanceToTarget = Vector3.Distance(transform.position, target.position);
        if(distanceToTarget <= attackRange)
        {
            currentState = EZombieState.Attack;
        }
        else if (distanceToTarget <= trackingRange)
        {
            currentState = EZombieState.Chase;
        }
        else if (distanceToTarget <= evadeRange)
        {
            currentState = EZombieState.Evade;
        }
        else
        {
            currentState = EZombieState.Patrol;
        }


            switch (currentState)
            {
                case EZombieState.Chase:
                    Move();
                    break;
                case EZombieState.Idle:
                    break;
                case EZombieState.Evade:
                    //Evade();
                    break;
                case EZombieState.Attack:
                    Attack();
                    break;
                case EZombieState.Patrol:
                    Patrol();
                    break;
                default:
                    break;
            }

    }

    private void Patrol()
    {
        if(patrolPoints.Length > 0)
        {
            Debug.Log("순찰중");
            Transform targetPoint = patrolPoints[currentPoint];
            Vector3 direction = (targetPoint.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.LookAt(targetPoint.position);

            if(Vector3.Distance(transform.position, targetPoint.position) < 0.3f)
            {
                currentPoint = (currentPoint + 1) % patrolPoints.Length;
            }
        }
    }

    private void Attack()
    {
    }

    private void Move()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.LookAt(transform.position + direction);
    }

    private void Evade()
    {

    }
}
