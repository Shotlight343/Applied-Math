using UnityEngine;

// UI prefab (Image + RectTransform). Flies from its spawn position to a
// target RectTransform using anchoredPosition lerp only - no physics.
[RequireComponent(typeof(RectTransform))]
public class Coin : MonoBehaviour
{
    [SerializeField] private float flightDuration = 0.6f;

    private RectTransform rect;
    private RectTransform target;
    private Vector2 startPos;
    private int value;
    private float t;
    private bool initialized;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Init(RectTransform targetRect, int coinValue)
    {
        target = targetRect;
        value = coinValue;
        startPos = rect.anchoredPosition;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;
        if (target == null) { Destroy(gameObject); return; }

        t += Time.deltaTime / flightDuration;
        float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 2f); // ease-out
        rect.anchoredPosition = Vector2.Lerp(startPos, target.anchoredPosition, eased);

        if (t >= 1f)
        {
            if (BankUI.Instance != null) BankUI.Instance.AddCoins(value);
            Destroy(gameObject);
        }
    }
}
