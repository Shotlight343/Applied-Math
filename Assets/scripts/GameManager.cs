using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player HP")]
    [SerializeField] private int maxHP = 20;
    [SerializeField] private HPBarUI hpBarUI;

    [Header("Economy")]
    [SerializeField] private BankUI bankUI;

    [Header("UI References (for coin flight)")]
    [SerializeField] private RectTransform uiCanvasRect;
    [SerializeField] private RectTransform bankTargetRect; // anchor the coins fly toward

    public RectTransform UICanvasRect => uiCanvasRect;
    public RectTransform BankTargetRect => bankTargetRect;

    private int currentHP;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        currentHP = maxHP;
    }

    private void Start()
    {
        if (hpBarUI != null) hpBarUI.SetHP(currentHP, maxHP, true); // snap both bars on init
    }

    public void DamagePlayer(int amount)
    {
        currentHP = Mathf.Max(0, currentHP - amount);
        if (hpBarUI != null) hpBarUI.SetHP(currentHP, maxHP);
        if (currentHP <= 0) HandleGameOver();
    }

    private void HandleGameOver()
    {
        Debug.Log("Game Over - player HP reached 0.");
        // TODO: stop spawners, show game-over UI, etc.
    }
}
