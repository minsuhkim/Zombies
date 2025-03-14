using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public enum EZombieState
{
    Patrol, Chase, Attack, Evade, Damage, Idle, Die,
}

public class ZombieManager : MonoBehaviour
{
    private float zombieHP = 100;

    public float HP
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
    public float moveSpeed = 0.3f;
    private float defaultSpeed = 0.3f;
    private float chaseRange = 3.0f;                     // 추적 범위 설정
    private bool isAttack = false;                          // 공격 상태
    private float evadeRange = 5.0f;                        // 도망 상태 회피 거리
    //private float zombieHP =10;
    private float distanceToTarget;                         // target과의 거리 계산값
    private bool isWaiting = false;                         // 상태 전환 후 대기 상태 여부
    public float idleTime = 2.0f;                           // 각 상태 전환 후 대기 시간

    private Animator animator;
    private AudioSource audioSource;
    public AudioClip audioClipAttack;

    private Coroutine stateRoutine;

    private bool isLive = true;

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        ChangeState(EZombieState.Idle);
    }

    private void Update()
    {
        distanceToTarget = Vector3.Distance(transform.position, target.position);
    }

    public void ChangeState(EZombieState newState, float damage = 0)
    {
        if (!isLive)
        {
            return;
        }

        if (stateRoutine != null)
        {
            StopCoroutine(stateRoutine);
        }
        currentState = newState;

        switch (currentState)
        {
            case EZombieState.Patrol:
                stateRoutine = StartCoroutine(Patrol());
                break;
            case EZombieState.Chase:
                stateRoutine = StartCoroutine(Chase());
                break;
            case EZombieState.Attack:
                stateRoutine = StartCoroutine(Attack());
                break;
            case EZombieState.Evade:
                stateRoutine = StartCoroutine(Evade());
                break;
            case EZombieState.Damage:
                stateRoutine = StartCoroutine(TakeDamage(damage));
                break;
            case EZombieState.Idle:
                stateRoutine = StartCoroutine(Idle());
                break;
            case EZombieState.Die:
                stateRoutine = StartCoroutine(Die());
                break;
        }
    }

    private IEnumerator Idle()
    {
        Debug.Log($"{gameObject.name} : 대기중");
        animator.Play("ZombieIdle");

        while (currentState == EZombieState.Idle)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            if (distance < chaseRange)
            {
                ChangeState(EZombieState.Chase);
            }
            else if (distance < attackRange)
            {
                ChangeState(EZombieState.Attack);
            }
            else
            {
                ChangeState(EZombieState.Patrol);
            }

            yield return null;
        }
    }

    private IEnumerator Patrol()
    {
        Debug.Log($"{gameObject.name} : 순찰중");
        animator.SetBool("isWalk", true);

        while (currentState == EZombieState.Patrol)
        {
            if (patrolPoints.Length > 0)
            {
                Transform targetPoint = patrolPoints[currentPoint];
                Vector3 direction = (targetPoint.position - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;
                transform.LookAt(targetPoint.position);

                if (Vector3.Distance(transform.position, targetPoint.position) < 0.3f)
                {
                    currentPoint = (currentPoint + 1) % patrolPoints.Length;
                }
            }

            float distance = Vector3.Distance(transform.position, target.position);

            if (distance < chaseRange)
            {
                ChangeState(EZombieState.Chase);
            }
            else if (distance < attackRange)
            {
                ChangeState(EZombieState.Attack);
            }

            yield return null;
        }
    }

    private IEnumerator Chase()
    {
        Debug.Log($"{gameObject.name} : 추적중");
        animator.SetBool("isWalk", true);

        while (currentState == EZombieState.Chase)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.LookAt(transform.position + direction);
            transform.position += direction * moveSpeed * Time.deltaTime;

            float distance = Vector3.Distance(transform.position, target.position);

            if (distance > chaseRange)
            {
                ChangeState(EZombieState.Patrol);
            }
            else if (distance < attackRange)
            {
                ChangeState(EZombieState.Attack);
            }

            yield return null;
        }
    }

    private IEnumerator Attack()
    {
        Debug.Log($"{gameObject.name} : 공격중");
        animator.Play("ZombieAttack");
        transform.LookAt(target.position);
        yield return new WaitForSeconds(attackDelay);

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > attackRange)
        {
            ChangeState(EZombieState.Chase);
        }
        else
        {
            ChangeState(EZombieState.Attack);
        }
    }

    private IEnumerator Evade()
    {
        Debug.Log($"{gameObject.name} : 도망중");

        animator.SetBool("isWalk", true);

        Vector3 evadeDirection = (transform.position - target.position).normalized;
        float evadeTime = 3.0f;
        float timer = 0.0f;

        Quaternion targetRotation = Quaternion.LookRotation(evadeDirection);
        transform.rotation = targetRotation;

        while (currentState == EZombieState.Evade && timer < evadeTime)
        {
            transform.position += evadeDirection * moveSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        ChangeState(EZombieState.Idle);
    }

    private IEnumerator TakeDamage(float damage)
    {
        Debug.Log($"{gameObject.name} : {damage} 데미지 받음");
        moveSpeed = 0f;
        audioSource.PlayOneShot(audioClipAttack);
        animator.SetTrigger("Hit");
        zombieHP -= damage;

        if (zombieHP <= 0)
        {
            ChangeState(EZombieState.Die);
        }
        else
        {
            ChangeState(EZombieState.Idle);
        }

        yield return null;
    }

    public void OnTakeDamageEnd()
    {
        moveSpeed = defaultSpeed;
    }

    private IEnumerator Die()
    {
        Debug.Log($"{gameObject.name} : 사망");
        isLive = false;
        animator.SetTrigger("Die");
        yield return new WaitForSeconds(3.0f);
        gameObject.SetActive(false);

    }
}