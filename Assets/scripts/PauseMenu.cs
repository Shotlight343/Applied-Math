using UnityEngine;
using UnityEngine.SceneManagement;


public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        IsPaused = false; 
    }

    private const float PanelWidth = 340f;
    private const float ButtonWidth = 260f;
    private const float ButtonHeight = 54f;
    private const float ButtonGap = 14f;
    private const float TitleHeight = 70f;

    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;
    private GUIStyle hintStyle;

    private void OnGUI()
    {
        GUI.depth = -100; 

        if (buttonStyle == null) CreateStyles();

        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            if (IsPaused)
            {
                Resume();
                e.Use();
            }
            else if (!TurretShop.IsPlacing) 
            {
                Pause();
                e.Use();
            }
        }

        if (IsPaused) DrawMenu();
    }

    private void OnDestroy()
    {
        if (IsPaused) Resume(); 
    }

    private void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    private void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private void Restart()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.buildIndex < 0)
        {
            Debug.LogError("Restart: this scene isn't in the build. Add it under File > Build Profiles > Scene List, then try again.");
            return;
        }

        Resume(); 
        SceneManager.LoadScene(scene.buildIndex);
    }

    private void Quit()
    {
        Resume();
        SceneManager.LoadScene("MainMenu");
        Debug.Log("Quit pressed - nothing happens until a main menu exists.");
    }

    private void DrawMenu()
    {
        // Dim the game behind the menu
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float panelHeight = TitleHeight + 3f * ButtonHeight + 2f * ButtonGap + 50f;
        Rect panel = new Rect((Screen.width - PanelWidth) / 2f, (Screen.height - panelHeight) / 2f, PanelWidth, panelHeight);
        GUI.Box(panel, GUIContent.none);

        GUI.Label(new Rect(panel.x, panel.y + 10f, panel.width, TitleHeight - 10f), "PAUSED", titleStyle);

        float x = panel.x + (PanelWidth - ButtonWidth) / 2f;
        float y = panel.y + TitleHeight;

        if (GUI.Button(new Rect(x, y, ButtonWidth, ButtonHeight), "Resume", buttonStyle)) Resume();
        y += ButtonHeight + ButtonGap;

        if (GUI.Button(new Rect(x, y, ButtonWidth, ButtonHeight), "Restart", buttonStyle)) Restart();
        y += ButtonHeight + ButtonGap;

        if (GUI.Button(new Rect(x, y, ButtonWidth, ButtonHeight), "Quit", buttonStyle)) Quit();

        GUI.Label(new Rect(panel.x, panel.yMax - 34f, panel.width, 24f), "Esc to resume", hintStyle);
    }

    private void CreateStyles()
    {
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 36, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        titleStyle.normal.textColor = Color.white;

        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 22, alignment = TextAnchor.MiddleCenter };

        hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
        hintStyle.normal.textColor = new Color(1f, 1f, 1f, 0.7f);
    }
}
