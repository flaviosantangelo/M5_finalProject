using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StandingGuardEnemy : MonoBehaviour
{
    private GameManager _gameManager;
    [SerializeField] private enum EnemyState { StandingGuard, Chasing, Searching }
    [SerializeField] private EnemyState currentState;
    private NavMeshAgent _agent;
    private Transform _player;
    private Coroutine currentBehaviorRoutine;
    [SerializeField] private float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 90f;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask obstacleMask;
    private Vector3 lastKnownPlayerPosition;
    private Vector3 _initialPosition;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.isStopped = true;
    }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            _player = playerObject.transform;
        }
        _initialPosition = transform.position;
        _gameManager = FindObjectOfType<GameManager>();


        SetState(EnemyState.StandingGuard);
    }

    private void SetState(EnemyState newState)
    {
        if (currentBehaviorRoutine != null)
        {
            StopCoroutine(currentBehaviorRoutine);
        }

        currentState = newState;

        switch (currentState)
        {
            case EnemyState.StandingGuard:
                _agent.isStopped = true;
                currentBehaviorRoutine = StartCoroutine(StandingGuardRoutine());
                break;
            case EnemyState.Chasing:
                _agent.isStopped = false;
                currentBehaviorRoutine = StartCoroutine(ChaseRoutine());
                break;
            case EnemyState.Searching:
                _agent.isStopped = false;
                currentBehaviorRoutine = StartCoroutine(SearchRoutine());
                break;
        }
    }

    private IEnumerator StandingGuardRoutine()
    {
        Quaternion originalRotation = transform.rotation;
        while (currentState == EnemyState.StandingGuard)
        {
            if (CanSeePlayer())
            {
                SetState(EnemyState.Chasing);
                yield break;
            }

            Quaternion newRotation = transform.rotation * Quaternion.Euler(0, 90, 0);
            float rotationTime = 1f;
            float elapsed = 0f;

            while (elapsed < rotationTime)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, elapsed / rotationTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(5f);
        }
    }

    private IEnumerator ChaseRoutine()
    {
        while (currentState == EnemyState.Chasing)
        {
            if (_player == null) { SetState(EnemyState.StandingGuard); yield break; }

            if (CanSeePlayer())
            {
                _agent.SetDestination(_player.position);
                lastKnownPlayerPosition = _player.position;
            }
            else
            {
                SetState(EnemyState.Searching);
                yield break;
            }
            yield return null;
        }
    }



    private IEnumerator SearchRoutine()
    {
        _agent.SetDestination(lastKnownPlayerPosition);

        while (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance)
        {
            if (CanSeePlayer())
            {
                SetState(EnemyState.Chasing);
                yield break;
            }
            yield return null;
        }

        yield return new WaitForSeconds(2f);
        _agent.SetDestination(_initialPosition);
        _agent.isStopped = false;
        while (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance)
        {
            if (CanSeePlayer()) 
            {
                SetState(EnemyState.Chasing);
                yield break;
            }
            yield return null;
        }

        
        SetState(EnemyState.StandingGuard);
    }

    
    private bool CanSeePlayer()
    {
        if (_player == null) return false;
        Vector3 directionToPlayer = (_player.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, directionToPlayer) < viewAngle / 2f)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, _player.position);
            if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask))
            {
                return true;
            }
        }
        return false;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (_gameManager != null)
            {
                _gameManager.OnPlayerCaught();
                _agent.isStopped = true;
                if (currentBehaviorRoutine != null)
                {
                    StopCoroutine(currentBehaviorRoutine);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        if (_player != null)
        {
            Vector3 directionToPlayer = (_player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer < viewAngle / 2f)
            {
                Gizmos.color = Color.red;
            }
        }

        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 leftDirection = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 rightDirection = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;

        Gizmos.DrawRay(transform.position, leftDirection * viewRadius);
        Gizmos.DrawRay(transform.position, rightDirection * viewRadius);

        Gizmos.DrawLine(transform.position + leftDirection * viewRadius, transform.position + rightDirection * viewRadius);
    }
}