using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private float timeLimit = 180f;
    [SerializeField] private TMP_Text timerText;

    private float currentTime;
    private float elapsedTime;
    private bool timerRunning = false;

    private void Start()
    {
        currentTime = timeLimit;
        elapsedTime = 0;

        UpdateTimerText();
    }

    private void Update()
    {
        if (GameManager.Instance.GameEnded()) return;
        if (!timerRunning) return;

        currentTime -= Time.deltaTime;
        elapsedTime += Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;

            UpdateTimerText();

            GameManager.Instance.Lose();

            return;
        }

        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }

    public void SetTimerRunning(bool running)
    {
        timerRunning = running;
    }
}