using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrollingEnemy : MonoBehaviour
{
    private GameManager _gameManager;
    public enum EnemyState { Patrolling, Chasing, Searching }
    [SerializeField] private EnemyState _currentState;
    private NavMeshAgent _agent;
    private Transform _player;
    private Coroutine _currentBehaviorRoutine;
    [SerializeField] private float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 90f;
    [SerializeField] private LayerMask _playerMask;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private Transform[] _patrolPoints;
    private int currentPatrolIndex = 0;
    private Vector3 _lastKnownPlayerPosition;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            _player = playerObject.transform;
        }
        _gameManager = FindObjectOfType<GameManager>();
        SetState(EnemyState.Patrolling);
    }

    private void SetState(EnemyState newState)
    {
        if (_currentBehaviorRoutine != null)
        {
            StopCoroutine(_currentBehaviorRoutine);
        }

        _currentState = newState;

        switch (_currentState)
        {
            case EnemyState.Patrolling:
                _currentBehaviorRoutine = StartCoroutine(PatrolRoutine());
                break;
            case EnemyState.Chasing:
                _currentBehaviorRoutine = StartCoroutine(ChaseRoutine());
                break;
            case EnemyState.Searching:
                _currentBehaviorRoutine = StartCoroutine(SearchRoutine());
                break;
        }
    }

    private IEnumerator PatrolRoutine()
    {
        if (_patrolPoints.Length == 0) yield break;

        while (_currentState == EnemyState.Patrolling)
        {
            if (CanSeePlayer())
            {
                SetState(EnemyState.Chasing);
                yield break;
            }

            _agent.SetDestination(_patrolPoints[currentPatrolIndex].position);
            while (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance)
            {
                yield return null;
            }

            currentPatrolIndex = (currentPatrolIndex + 1) % _patrolPoints.Length;
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator ChaseRoutine()
    {
        
        while (_currentState == EnemyState.Chasing)
        {
            if (_player == null) { SetState(EnemyState.Patrolling); yield break; }

            if (CanSeePlayer())
            {
                _agent.SetDestination(_player.position);
                _lastKnownPlayerPosition = _player.position;
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
        _agent.SetDestination(_lastKnownPlayerPosition);

        while (_agent.pathPending || _agent.remainingDistance >_agent.stoppingDistance)
        {
            if (CanSeePlayer())
            {
                SetState(EnemyState.Chasing);
                yield break;
            }
            yield return null;
        }

        yield return new WaitForSeconds(2f);
        SetState(EnemyState.Patrolling);
    }

    private bool CanSeePlayer()
    {
        if (_player == null) return false;
        Vector3 directionToPlayer = (_player.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, directionToPlayer) < viewAngle / 2f)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, _player.position);
            if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, _obstacleMask))
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
                if (_currentBehaviorRoutine != null)
                {
                    StopCoroutine(_currentBehaviorRoutine);
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