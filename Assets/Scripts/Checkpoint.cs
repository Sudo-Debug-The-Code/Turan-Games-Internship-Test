using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private CheckpointManager checkpointManager;
    private int checkpointIndex;

    public void Setup(CheckpointManager manager, int index)
    {
        checkpointManager = manager;
        checkpointIndex = index;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;

        checkpointManager.ReachCheckpoint(checkpointIndex);
    }
}