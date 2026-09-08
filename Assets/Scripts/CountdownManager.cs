using System.Collections;
using TMPro;
using UnityEngine;

public class CountdownManager : MonoBehaviour
{
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private CarControl carControl;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private TimeManager timeManager;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.8f;
    [SerializeField] private float startScale = 2.5f;

    private RigidbodyConstraints originalConstraints;

    private void Start()
    {
        originalConstraints = rb.constraints;

        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        carControl.SetControlsEnabled(false);
        timeManager.SetTimerRunning(false);

        rb.constraints = RigidbodyConstraints.FreezeAll;

        countdownPanel.SetActive(true);
        countdownText.gameObject.SetActive(true);

        yield return ShowText("3");
        yield return ShowText("2");
        yield return ShowText("1");
        yield return ShowText("GO!");

        countdownPanel.SetActive(false);

        rb.constraints = originalConstraints;

        carControl.SetControlsEnabled(true);
        timeManager.SetTimerRunning(true);
    }

    private IEnumerator ShowText(string text)
    {
        countdownText.text = text;

        Color color = countdownText.color;
        color.a = 1f;
        countdownText.color = color;

        countdownText.transform.localScale = Vector3.one * startScale;

        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animationDuration;

            countdownText.transform.localScale = Vector3.Lerp(Vector3.one * startScale, Vector3.one,Mathf.SmoothStep(0f, 1f, t));

            color.a = 1f - Mathf.Clamp01((t - 0.65f) / 0.35f);
            countdownText.color = color;

            yield return null;
        }
    }
}