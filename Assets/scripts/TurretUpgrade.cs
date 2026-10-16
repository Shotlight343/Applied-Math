using UnityEngine;
using System.Collections.Generic;


public class TurretUpgrade : MonoBehaviour
{
    [SerializeField] private string turretName = "Turret";
    [SerializeField] private int startLevel = 0;
    [SerializeField] private int[] levelCosts = { 10, 50, 100 };
    [SerializeField] private Vector3 labelOffset = new Vector3(0f, 3f, 0f); 

    public int Level { get; private set; }
    public int MaxLevel => levelCosts.Length;

    private MonoBehaviour[] turretScripts;
    private LineRenderer outline;
    private GUIStyle buttonStyle;
    private GUIStyle labelStyle;

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

    private void Start()
    {
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
        if (!BankUI.Instance.TrySpend(levelCosts[Level])) return;

        Level++;
        Refresh();
    }

    private void OnGUI()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        if (GameManager.Instance != null && (GameManager.Instance.IsGameOver || GameManager.Instance.IsVictory)) return;

        Vector3 screen = cam.WorldToScreenPoint(transform.position + labelOffset);
        if (screen.z < 0f) return; // behind the camera

        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 16 };
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
            labelStyle.normal.textColor = Color.white;
        }

        const float width = 240f;
        const float height = 32f;
        Rect rect = new Rect(screen.x - width / 2f, Screen.height - screen.y - height, width, height);

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
}
