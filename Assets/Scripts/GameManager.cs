using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("End Game UI")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Win UI")]
    [SerializeField] private TMP_Text currentTimeText;
    [SerializeField] private TMP_Text bestTimeText;
    [SerializeField] private TMP_Text bestTimeLabelText;

    public static GameManager Instance;

    [SerializeField] private TimeManager timeManager;
    [SerializeField] private CarControl car;

    private bool gameEnded = false;

    public bool GameEnded()
    {
        return gameEnded;
    }
    private void Start()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Restart();
        }
    }

    public void Win()
    {
        if (gameEnded) return;

        gameEnded = true;

        car.SetControlsEnabled(false);
        timeManager.SetTimerRunning(false);

        float currentTime = timeManager.GetElapsedTime();

        bool hasBestTime = PlayerPrefs.HasKey("BestTime");
        float bestTime = PlayerPrefs.GetFloat("BestTime", currentTime);

        bool newBest = !hasBestTime || currentTime < bestTime;

        if (newBest)
        {
            bestTime = currentTime;

            PlayerPrefs.SetFloat("BestTime", bestTime);
            PlayerPrefs.Save();

            bestTimeLabelText.text = "New Best Time";
        }
        else
        {
            bestTimeLabelText.text = "Best Time";
        }

        currentTimeText.text = FormatTime(currentTime);
        bestTimeText.text = FormatTime(bestTime);

        winPanel.SetActive(true);
    }

    public void Lose()
    {
        if (gameEnded) return;

        gameEnded = true;

        car.SetControlsEnabled(false);
        timeManager.SetTimerRunning(false);

        losePanel.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        int milliseconds = Mathf.FloorToInt((time * 1000) % 1000);

        return minutes.ToString("00") + ":" + seconds.ToString("00") + "." + milliseconds.ToString("000");
    }

    public float GetBestTime()
    {
        if (!PlayerPrefs.HasKey("BestTime")) return -1;

        return PlayerPrefs.GetFloat("BestTime");
    }
}