using UnityEngine;

public class Victory : MonoBehaviour
{
    [SerializeField] private GameObject _victoryCanvas; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _victoryCanvas.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}