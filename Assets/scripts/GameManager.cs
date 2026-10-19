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
    [SerializeField] private RectTransform bankTargetRect; 

    public RectTransform UICanvasRect => uiCanvasRect;
    public RectTransform BankTargetRect => bankTargetRect;

    public bool IsGameOver { get; private set; }
    public bool IsVictory { get; private set; }

    private int currentHP;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        currentHP = maxHP;
    }

    private void Start()
    {
        if (hpBarUI != null) hpBarUI.SetHP(currentHP, maxHP, true); 
    }

    public void DamagePlayer(int amount)
    {
        if (IsGameOver || IsVictory) return;

        currentHP = Mathf.Max(0, currentHP - amount);
        if (hpBarUI != null) hpBarUI.SetHP(currentHP, maxHP);
        if (currentHP <= 0) HandleGameOver();
    }

  
    public void Victory()
    {
        if (IsGameOver) return;
        IsVictory = true;
        Debug.Log("Victory - all waves cleared.");
    }

    private void HandleGameOver()
    {
        IsGameOver = true;
        SfxManager.Lose();
        Debug.Log("Game Over - player HP reached 0.");
    }
}