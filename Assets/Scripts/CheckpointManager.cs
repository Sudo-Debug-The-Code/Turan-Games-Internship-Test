using TMPro;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [SerializeField] private Checkpoint[] checkpoints;
    [SerializeField] private int lapsToWin = 3;

    [Header("UI")]
    [SerializeField] private TMP_Text lapText;
    [SerializeField] private TMP_Text checkpointText;

    private int currentCheckpoint = 0;
    private int checkpointsPassed = 0;
    private int currentLap = 0;
    private bool completedLap = false;

    private void Start()
    {
        for (int i = 0; i < checkpoints.Length; i++)
        {
            checkpoints[i].Setup(this, i);
        }

        UpdateUI();
    }

    public void ReachCheckpoint(int checkpointIndex)
    {
        if (GameManager.Instance.GameEnded()) return;

        if (checkpointIndex != currentCheckpoint) return;

        Debug.Log("Checkpoint: " + checkpointIndex);

        if (completedLap && checkpointIndex == 0)
        {
            currentLap++;

            Debug.Log("Lap: " + currentLap + "/" + lapsToWin);

            if (currentLap >= lapsToWin)
            {
                GameManager.Instance.Win();
                return;
            }

            completedLap = false;
            checkpointsPassed = 0;
        }

        currentCheckpoint++;
        checkpointsPassed++;

        if (currentCheckpoint >= checkpoints.Length)
        {
            completedLap = true;
            currentCheckpoint = 0;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        lapText.text = "Lap: " + (currentLap + 1) + "/" + lapsToWin;
        checkpointText.text = "Checkpoint: " + checkpointsPassed + "/" + checkpoints.Length;
    }
}