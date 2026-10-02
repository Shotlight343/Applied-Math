using UnityEngine;
using TMPro;
using System.Collections;

// Swap Text for TMP_Text if your project uses TextMeshPro.
public class BankUI : MonoBehaviour
{
    public static BankUI Instance { get; private set; }

    [SerializeField] private TMP_Text bankText;
    [SerializeField] private RectTransform iconTransform; // punches on each deposit
    [SerializeField] private float countDuration = 0.5f;
    [SerializeField] private float punchScale = 1.2f;
    [SerializeField] private float punchDuration = 0.15f;

    private int displayedTotal;
    private int targetTotal;
    private Coroutine countRoutine;
    private Coroutine punchRoutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateText(displayedTotal);
    }

    public void AddCoins(int amount)
    {
        targetTotal += amount;

        if (countRoutine != null) StopCoroutine(countRoutine);
        countRoutine = StartCoroutine(CountUpRoutine());

        if (iconTransform != null)
        {
            if (punchRoutine != null) StopCoroutine(punchRoutine);
            punchRoutine = StartCoroutine(PunchRoutine());
        }
    }

    private IEnumerator CountUpRoutine()
    {
        int startValue = displayedTotal;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / countDuration;
            float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 2f); // ease-out
            displayedTotal = Mathf.RoundToInt(Mathf.Lerp(startValue, targetTotal, eased));
            UpdateText(displayedTotal);
            yield return null;
        }

        displayedTotal = targetTotal;
        UpdateText(displayedTotal);
    }

    private IEnumerator PunchRoutine()
    {
        Vector3 baseScale = Vector3.one;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / punchDuration;
            float scale = Mathf.Lerp(punchScale, 1f, t); // overshoots then settles to 1
            iconTransform.localScale = baseScale * scale;
            yield return null;
        }

        iconTransform.localScale = baseScale;
    }

    private void UpdateText(int value)
    {
        if (bankText != null) bankText.text = value.ToString();
    }
}
