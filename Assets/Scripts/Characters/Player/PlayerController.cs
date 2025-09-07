using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private bool _mainSwitch = true;
    private PlayerManagement _playerManagement;
    private NavMeshAgent _agent;
    private RaycastHit _hit;
    private IEnumerator _dest;
    private Quaternion _targetRotation;
    [SerializeField] private float _step = 1f;
    private float _lastTimeStep;
    public event Action onStep;

    void Awake()
    {
        _playerManagement = GetComponentInParent<PlayerManagement>();
        _agent = GetComponent<NavMeshAgent>();
        _agent.SetDestination(transform.position);
        _dest = GoToDestination();
    }

    void OnEnable()
    {
        _lastTimeStep = Time.time;
        StartCoroutine(_dest);
    }

    void OnDisable()
    {
        StopCoroutine(_dest);
        _agent.isStopped = true;
    }

    private IEnumerator GoToDestination()
    {
        while (_mainSwitch)
        {
            yield return null;

            if (_playerManagement.GetMove() == Vector3.zero)
            {
                if (_playerManagement.GetHasRay())
                {
                    if (Physics.Raycast(_playerManagement.GetRay(), out _hit))
                    {
                        _agent.isStopped = false;
                        _agent.SetDestination(_hit.point);
                    }
                }
            }
            
            else
            {
                _agent.isStopped = true;

                _targetRotation = Quaternion.LookRotation(_playerManagement.GetMove(), Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, _agent.angularSpeed * Time.deltaTime);

                _agent.Move(_playerManagement.GetMove() * (_agent.speed * Time.deltaTime));
            }
            
            if (_agent.remainingDistance > 0.2f && _lastTimeStep + _step < Time.time)
            {
                _lastTimeStep = Time.time;
                onStep?.Invoke();
            }
        }
    }
}
