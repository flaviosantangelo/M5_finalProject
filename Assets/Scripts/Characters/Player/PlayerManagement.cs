using UnityEngine;

public class PlayerManagement : MonoBehaviour
{
    private PlayerController _playerController;
    public PlayerController GetPlayerController() => _playerController;
    private Camera _camera;
    private Quaternion _deltaRotationMove;
    private float _lenght;
    private Vector3 _dir;
    public Vector3 GetMove() => _dir;

    private bool _hasRay;
    public bool GetHasRay() => _hasRay;
    private Ray _ray;
    public Ray GetRay() => _ray;

    void Awake()
    {
        _playerController = GetComponentInChildren<PlayerController>();
    }

    void Start()
    {
        _camera = Camera.main;
        _deltaRotationMove = Quaternion.Euler(0f, _camera.transform.eulerAngles.y, 0f);
    }

    void Update()
    {
        InputRead();
    }

    private void InputRead()
    {
        _dir = _deltaRotationMove * new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        if (_dir == Vector3.zero)
        {
            if (Input.GetMouseButton(0))
            {
                _ray = _camera.ScreenPointToRay(Input.mousePosition);
                _hasRay = true;
            }
            else
            {
                _hasRay = false;
            }
        }
        else
        {
            _lenght = _dir.sqrMagnitude;
            if (_lenght > 1f)
            {
                _lenght = Mathf.Sqrt(_lenght);
                _dir.x /= _lenght;
                _dir.z /= _lenght;
            }
        }
    }
}
