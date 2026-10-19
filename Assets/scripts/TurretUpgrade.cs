using UnityEngine;
using System.Collections.Generic;


public class TurretUpgrade : MonoBehaviour
{
    public static readonly List<TurretUpgrade> All = new List<TurretUpgrade>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        All.Clear(); 
    }

    private const float ButtonWidth = 240f;
    private const float ButtonHeight = 32f;
    private const float SellButtonHeight = 28f;
    private const float SellConfirmSeconds = 3f;

    [SerializeField] private string turretName = "Turret";
    [SerializeField] private int startLevel = 0;
    [SerializeField] private int[] levelCosts = { 10, 50, 100 };
    [SerializeField, Range(0, 100)] private int sellRefundPercent = 50; 
    [SerializeField] private Vector3 labelOffset = new Vector3(0f, 3f, 0f); 

    public string TurretName => turretName;
    public int BuildCost => levelCosts.Length > 0 ? levelCosts[0] : 0; 
    public int Level { get; private set; }
    public int MaxLevel => levelCosts.Length;

  
    public int SellValue => Mathf.RoundToInt(invested * sellRefundPercent / 100f);

    private MonoBehaviour[] turretScripts;
    private LineRenderer outline;
    private GUIStyle buttonStyle;
    private GUIStyle labelStyle;

    private int invested;          
    private float sellArmedUntil;  

    private void Awake()
    {
        List<MonoBehaviour> found = new List<MonoBehaviour>();
        foreach (MonoBehaviour mb in GetComponents<MonoBehaviour>())
        {
            if (mb is ILevelable) found.Add(mb);
        }
        turretScripts = found.ToArray();
        outline = GetComponent<LineRenderer>();
        Level = Mathf.Clamp(startLevel, 0, MaxLevel); 
    }

    private void OnEnable()
    {
        All.Add(this);
    }

    private void OnDisable()
    {
        All.Remove(this);
    }

    private void Start()
    {
        Refresh();
    }

   
    public void SetLevel(int level)
    {
        Level = Mathf.Clamp(level, 0, MaxLevel);

        invested = 0;
        for (int i = 0; i < Level; i++) invested += levelCosts[i];

        Refresh();
    }

   
    private void Refresh()
    {
        bool built = Level > 0;

        foreach (MonoBehaviour mb in turretScripts)
        {
            if (built) ((ILevelable)mb).ApplyLevel(Level);
            mb.enabled = built;
        }

        if (outline != null) outline.enabled = built;
    }

    private void TryUpgrade()
    {
        if (Level >= MaxLevel || BankUI.Instance == null) return;

        int cost = levelCosts[Level];
        if (!BankUI.Instance.TrySpend(cost)) return;

        invested += cost;
        Level++;
        Refresh();
    }

    private void Sell()
    {
        int refund = SellValue;
        if (refund > 0 && BankUI.Instance != null) BankUI.Instance.AddCoins(refund);
        SfxManager.Sell();

        Destroy(gameObject);
    }


    public bool TryGetButtonRect(out Rect rect)
    {
        rect = default;

        Camera cam = Camera.main;
        if (cam == null) return false;

        Vector3 screen = cam.WorldToScreenPoint(transform.position + labelOffset);
        if (screen.z < 0f) return false;

        rect = new Rect(screen.x - ButtonWidth / 2f, Screen.height - screen.y - ButtonHeight, ButtonWidth, ButtonHeight);
        return true;
    }


    private static Rect GetSellRect(Rect upgradeRect)
    {
        return new Rect(upgradeRect.x, upgradeRect.yMax + 2f, ButtonWidth, SellButtonHeight);
    }

 
    public bool ButtonContains(Vector2 guiPoint)
    {
        if (!TryGetButtonRect(out Rect upgradeRect)) return false;
        if (upgradeRect.Contains(guiPoint)) return true;

        return Level > 0 && GetSellRect(upgradeRect).Contains(guiPoint);
    }

    private void OnGUI()
    {
        if (PauseMenu.IsPaused) return;
        if (GameManager.Instance != null && (GameManager.Instance.IsGameOver || GameManager.Instance.IsVictory)) return;
        if (!TryGetButtonRect(out Rect rect)) return;

        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 16 };
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
            labelStyle.normal.textColor = Color.white;
        }

        DrawUpgrade(rect);
        if (Level > 0) DrawSell(GetSellRect(rect));
    }

    private void DrawUpgrade(Rect rect)
    {
        if (Level >= MaxLevel)
        {
            GUI.Label(rect, $"{turretName} Lv{Level} (MAX)", labelStyle);
            return;
        }

        int cost = levelCosts[Level];
        string text = Level == 0
            ? $"Build {turretName} ({cost} coins)"
            : $"{turretName} Lv{Level} - Upgrade ({cost} coins)";

  
        GUI.enabled = BankUI.Instance != null && BankUI.Instance.Balance >= cost;
        if (GUI.Button(rect, text, buttonStyle)) TryUpgrade();
        GUI.enabled = true;
    }

    private void DrawSell(Rect rect)
    {
        bool armed = Time.unscaledTime < sellArmedUntil;
        string text = armed ? $"Click again to sell (+{SellValue})" : $"Sell (+{SellValue} coins)";

        GUI.color = armed ? new Color(1f, 0.55f, 0.55f, 1f) : Color.white;
        if (GUI.Button(rect, text, buttonStyle))
        {
            if (armed) Sell();
            else sellArmedUntil = Time.unscaledTime + SellConfirmSeconds;
        }
        GUI.color = Color.white;
    }
}