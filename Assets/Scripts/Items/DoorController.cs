using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DoorOrBlockController : MonoBehaviour
{
    private Collider doorCollider;

    [SerializeField] private float _rotationDuration = 1.0f;
    [SerializeField] private float _openAngle = 90f;
    [SerializeField] private bool _isSlidingBlock = false;
    private bool _isRotating = false;
    private Quaternion _initialRotation;
    private Quaternion _openRotation;
    private NavMeshObstacle _obstacle;

    private Vector3 _originalPosition;
    public Vector3 _targetPosition; 

    void Start()
    {
        _obstacle = GetComponent<NavMeshObstacle>();
        doorCollider = GetComponent<Collider>();

        if (_isSlidingBlock)
        {
            _originalPosition = transform.position;
        }
        _initialRotation = transform.localRotation;
        _openRotation = _initialRotation * Quaternion.Euler(0, _openAngle, 0);
    }

    public void Open()
    {
        if (_isRotating) return;

        _obstacle.carving = false;
        _obstacle.enabled = false;
        doorCollider.enabled = false;

        StartCoroutine(RotateDoor(_openRotation));


        
        if (_isSlidingBlock)
        {
            transform.position = _targetPosition;
            _obstacle.enabled = false; 
        }
        else
        {
            _obstacle.enabled = false;
        }

        _obstacle.carving = false;
    }

    public void Close()
    {

        if (_isSlidingBlock)
        {
            
            transform.position = _originalPosition;
            _obstacle.enabled = true; 
        }
        else
        {
            _obstacle.enabled = true;
        }

        _obstacle.carving = true;
    }

    private IEnumerator RotateDoor(Quaternion targetRotation)
    {
        _isRotating = true;
        Quaternion startRotation = transform.localRotation;
        float timeElapsed = 0;

        while (timeElapsed < _rotationDuration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / _rotationDuration;
            transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        transform.localRotation = targetRotation; 
        _isRotating = false;

        
        if (targetRotation == _initialRotation)
        {
            _obstacle.enabled = true;
            _obstacle.carving = true;
            doorCollider.enabled = true;
        }
    }
}