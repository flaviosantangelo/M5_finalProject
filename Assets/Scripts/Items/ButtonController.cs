using UnityEngine;

public class ButtonController : MonoBehaviour
{
    
    [SerializeField] private DoorOrBlockController _targetDoor;
    [SerializeField] private GameObject _canvasToDisplay;
    [SerializeField] private bool _activatesOnPress = true;
    private bool _isPlayerInTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInTrigger = true;
        }
        if (_canvasToDisplay != null)
        {
            _canvasToDisplay.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInTrigger = false;
        }
        if (_canvasToDisplay != null)
        {
            _canvasToDisplay.SetActive(false);
        }
    }

    void Update()
    {
        if (_isPlayerInTrigger && Input.GetKeyDown(KeyCode.E))
        {
            if (_activatesOnPress)
            {
                _targetDoor.Open();
            }
            else
            {
                _targetDoor.Close();
            }
        }
    }
}