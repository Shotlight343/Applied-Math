using UnityEngine;
using System.Collections.Generic;

public class TurretShop : MonoBehaviour
{
   
    [SerializeField] private List<GameObject> turretPrefabs = new List<GameObject>();

  
    [SerializeField] private float placementY = 0.5f;
    [SerializeField] private float minTurretSpacing = 2.5f;
    [SerializeField] private Collider mapBounds;

 
    [SerializeField] private Color validColor = new Color(0.2f, 1f, 0.3f, 1f);
    [SerializeField] private Color invalidColor = new Color(1f, 0.2f, 0.2f, 1f);

    private class Entry
    {
        public GameObject prefab;
        public TurretUpgrade info;
    }

    private const float ButtonWidth = 200f;
    private const float ButtonHeight = 56f;
    private const float HeaderHeight = 30f;
    private const float Padding = 10f;

    public static bool IsPlacing { get; private set; }

    private readonly List<Entry> entries = new List<Entry>();
    private readonly List<LineRenderer> ghostLines = new List<LineRenderer>();

    private GameObject selectedPrefab;
    private TurretUpgrade selectedInfo;
    private GameObject ghost;

    private GUIStyle buttonStyle;
    private GUIStyle headerStyle;
    private GUIStyle hintStyle;

    private void Awake()
    {
        foreach (GameObject prefab in turretPrefabs)
        {
            if (prefab == null) continue;

            TurretUpgrade info = prefab.GetComponentInChildren<TurretUpgrade>(true);
            if (info == null)
            {
                Debug.LogWarning($"TurretShop: {prefab.name} has no TurretUpgrade component, so it was skipped.");
                continue;
            }

            entries.Add(new Entry { prefab = prefab, info = info });
        }
    }

    private void OnDisable()
    {
        CancelPlacement();
    }

    private void OnGUI()
    {
        if (PauseMenu.IsPaused) return;

        GameManager gm = GameManager.Instance;
        if (gm != null && (gm.IsGameOver || gm.IsVictory))
        {
            CancelPlacement();
            return;
        }

        if (buttonStyle == null) CreateStyles();

        DrawPanel();
        if (ghost != null) HandlePlacement();
    }

    private Rect GetPanelRect()
    {
        int count = Mathf.Max(1, entries.Count);
        float width = Padding + count * (ButtonWidth + Padding);
        float height = HeaderHeight + ButtonHeight + 14f;
        return new Rect((Screen.width - width) / 2f, Screen.height - height - 16f, width, height);
    }

    private void DrawPanel()
    {
        Rect panel = GetPanelRect();
        GUI.Box(panel, GUIContent.none);

        string header = ghost != null
            ? $"Placing {selectedInfo.TurretName} - click the map  (right-click / Esc to cancel)"
            : "Turret Shop - pick a turret, then click the map to place it";
        GUI.Label(new Rect(panel.x, panel.y + 4f, panel.width, HeaderHeight), header, headerStyle);

        for (int i = 0; i < entries.Count; i++)
        {
            Entry entry = entries[i];
            Rect rect = new Rect(panel.x + Padding + i * (ButtonWidth + Padding), panel.y + HeaderHeight + 4f, ButtonWidth, ButtonHeight);

            bool isSelected = ghost != null && selectedPrefab == entry.prefab;
            bool canAfford = BankUI.Instance != null && BankUI.Instance.Balance >= entry.info.BuildCost;

            GUI.color = isSelected ? new Color(0.6f, 1f, 0.6f, 1f) : Color.white;
            GUI.enabled = canAfford || isSelected;

            if (GUI.Button(rect, $"{entry.info.TurretName}\n{entry.info.BuildCost} coins", buttonStyle))
            {
                SfxManager.Click();
                if (isSelected) CancelPlacement();
                else StartPlacement(entry);
            }

            GUI.enabled = true;
            GUI.color = Color.white;
        }
    }

    private void StartPlacement(Entry entry)
    {
        CancelPlacement();

        selectedPrefab = entry.prefab;
        selectedInfo = entry.info;

        ghost = Instantiate(entry.prefab);
        foreach (MonoBehaviour mb in ghost.GetComponentsInChildren<MonoBehaviour>(true)) mb.enabled = false;
        foreach (Collider col in ghost.GetComponentsInChildren<Collider>(true)) col.enabled = false;
        ghost.GetComponentsInChildren(true, ghostLines);
        ghost.SetActive(false);
        IsPlacing = true;
    }

    private void CancelPlacement()
    {
        if (ghost != null) Destroy(ghost);

        ghost = null;
        IsPlacing = false;
        selectedPrefab = null;
        selectedInfo = null;
        ghostLines.Clear();
    }

    private void HandlePlacement()
    {
        Event e = Event.current;
        Vector2 mouse = e.mousePosition;

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            CancelPlacement();
            e.Use();
            return;
        }

        if (e.type == EventType.MouseDown && e.button == 1)
        {
            CancelPlacement();
            e.Use();
            return;
        }

        bool overUI = IsOverUI(mouse);
        Vector3 point = default;
        bool hasPoint = !overUI && TryGetGroundPoint(mouse, out point);
        string problem = hasPoint ? GetPlacementProblem(point) : null;

        ghost.SetActive(hasPoint);
        if (hasPoint)
        {
            ghost.transform.position = point;

            Color color = problem == null ? validColor : invalidColor;
            foreach (LineRenderer line in ghostLines)
            {
                line.startColor = color;
                line.endColor = color;
            }

            string hint = problem ?? $"Click to place ({selectedInfo.BuildCost} coins)";
            GUI.Box(new Rect(mouse.x + 20f, mouse.y + 20f, 300f, 30f), hint, hintStyle);
        }

        if (e.type == EventType.MouseDown && e.button == 0 && hasPoint && problem == null)
        {
            PlaceTurret(point);
            e.Use();
        }
    }

    private void PlaceTurret(Vector3 point)
    {

        if (BankUI.Instance == null || !BankUI.Instance.TrySpend(selectedInfo.BuildCost)) return;

        GameObject turret = Instantiate(selectedPrefab, point, selectedPrefab.transform.rotation);

        TurretUpgrade upgrade = turret.GetComponentInChildren<TurretUpgrade>(true);
        if (upgrade != null) upgrade.SetLevel(1);

        SfxManager.Buy();
        SfxManager.Build();

        CancelPlacement();
    }

    private string GetPlacementProblem(Vector3 point)
    {
        if (BankUI.Instance == null || BankUI.Instance.Balance < selectedInfo.BuildCost) return "Not enough coins";

        if (mapBounds != null)
        {
            Bounds b = mapBounds.bounds;
            if (point.x < b.min.x || point.x > b.max.x || point.z < b.min.z || point.z > b.max.z) return "Outside the map";
        }

        foreach (TurretUpgrade other in TurretUpgrade.All)
        {
            Vector3 offset = other.transform.position - point;
            offset.y = 0f;
            if (offset.magnitude < minTurretSpacing) return "Too close to another turret";
        }

        return null;
    }

    private bool IsOverUI(Vector2 guiPoint)
    {
        if (GetPanelRect().Contains(guiPoint)) return true;
        if (PauseMenu.SpeedButtonContains(guiPoint)) return true;

        foreach (TurretUpgrade turret in TurretUpgrade.All)
        {
            if (turret.ButtonContains(guiPoint)) return true;
        }
        return false;
    }

    private bool TryGetGroundPoint(Vector2 guiPoint, out Vector3 point)
    {
        point = default;

        Camera cam = Camera.main;
        if (cam == null) return false;

        Ray ray = cam.ScreenPointToRay(new Vector3(guiPoint.x, Screen.height - guiPoint.y, 0f));
        Plane ground = new Plane(Vector3.up, new Vector3(0f, placementY, 0f));
        if (!ground.Raycast(ray, out float distance)) return false;

        point = ray.GetPoint(distance);
        return true;
    }

    private void CreateStyles()
    {
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 16, alignment = TextAnchor.MiddleCenter };

        headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        headerStyle.normal.textColor = Color.white;

        hintStyle = new GUIStyle(GUI.skin.box) { fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        hintStyle.normal.textColor = Color.white;
    }
}