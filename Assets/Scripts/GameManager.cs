using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject _gameOverPanel;
    [SerializeField] Transform _playerStartPoint;
    [SerializeField] int _totalRetries = 3;

    private int currentRetries;
    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        currentRetries = _totalRetries;
        _gameOverPanel.SetActive(false);
    }

    public void OnPlayerCaught()
    {
        _gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RespawnPlayer()
    {
        player.transform.position = _playerStartPoint.position;
        _gameOverPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void RetryLevel()
    {
        currentRetries--;

        if (currentRetries >= 0)
        {
            RespawnPlayer();
            Debug.Log("Tentativi rimanenti: " + currentRetries);
        }
        else
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1f;
    }
}