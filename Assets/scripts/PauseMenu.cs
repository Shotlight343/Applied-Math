using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }
    public static float GameSpeed { get; private set; } = 1f;

    private static Rect SpeedButtonRect => new Rect(Screen.width / 2f - 65f, 20f, 130f, 44f);

    public static bool SpeedButtonContains(Vector2 guiPoint)
    {
        return SpeedButtonRect.Contains(guiPoint);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        IsPaused = false;
        GameSpeed = 1f;
    }

  

    private const float PanelWidth = 340f;
    private const float ButtonWidth = 260f;
    private const float ButtonHeight = 54f;
    private const float ButtonGap = 14f;
    private const float TitleHeight = 70f;

    private bool showOptions;
    private bool sliderChanged;
    private bool endFrozen;
    private float endTime = -1f;
    private const float EndScreenDelay = 1.2f;

    private bool GameWon => GameManager.Instance != null && GameManager.Instance.IsVictory;
    private bool GameLost => GameManager.Instance != null && GameManager.Instance.IsGameOver;
    private bool GameEnded => GameWon || GameLost;

    private GUIStyle titleStyle;
    private GUIStyle endTitleStyle;
    private GUIStyle speedStyle;
    private GUIStyle buttonStyle;
    private GUIStyle hintStyle;
    private GUIStyle hintStyleLeft;

    private void OnGUI()
    {
        GUI.depth = -100;

        if (buttonStyle == null) CreateStyles();

        if (GameEnded)
        {
            if (endTime < 0f)
            {
                endTime = Time.unscaledTime;
                IsPaused = false;
                showOptions = false;
            }

            if (Time.unscaledTime - endTime < EndScreenDelay) return;

            if (!endFrozen)
            {
                endFrozen = true;
                Time.timeScale = 0f;
            }
            DrawEndScreen();
            return;
        }

        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            if (IsPaused)
            {
                if (showOptions) showOptions = false;
                else Resume();
                e.Use();
            }
            else if (!TurretShop.IsPlacing)
            {
                Pause();
                e.Use();
            }
        }

        if (IsPaused) DrawMenu();
        else DrawSpeedButton();
    }
private void DrawSpeedButton()
{
    string label = $"Speed: {GameSpeed:0}x";
    if (!GUI.Button(SpeedButtonRect, label, speedStyle)) return;

    SfxManager.Click();
    GameSpeed = GameSpeed >= 3f ? 1f : GameSpeed + 1f;
    Time.timeScale = GameSpeed;
}

    private void OnDestroy()
    {
        IsPaused = false;
        GameSpeed = 1f;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private void Pause()
    {
        IsPaused = true;
        showOptions = false;
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    private void Resume()
    {
        IsPaused = false;
        Time.timeScale = GameSpeed;
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

        GameSpeed = 1f;
        Resume();
        SceneManager.LoadScene(scene.buildIndex);
    }

    private void Quit()
    {
      
        GameSpeed = 1f;
        Resume();
        SceneManager.LoadScene("MainMenu");
    }

    private void DrawMenu()
    {

        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (showOptions)
        {
            DrawOptions();
            return;
        }

        float panelHeight = TitleHeight + 4f * ButtonHeight + 3f * ButtonGap + 50f;
        Rect panel = new Rect((Screen.width - PanelWidth) / 2f, (Screen.height - panelHeight) / 2f, PanelWidth, panelHeight);
        GUI.Box(panel, GUIContent.none);

        GUI.Label(new Rect(panel.x, panel.y + 10f, panel.width, TitleHeight - 10f), "PAUSED", titleStyle);

        float x = panel.x + (PanelWidth - ButtonWidth) / 2f;
        float y = panel.y + TitleHeight;

        if (MenuButton(new Rect(x, y, ButtonWidth, ButtonHeight), "Resume")) Resume();
        y += ButtonHeight + ButtonGap;

        if (MenuButton(new Rect(x, y, ButtonWidth, ButtonHeight), "Options")) showOptions = true;
        y += ButtonHeight + ButtonGap;

        if (MenuButton(new Rect(x, y, ButtonWidth, ButtonHeight), "Restart")) Restart();
        y += ButtonHeight + ButtonGap;

        if (MenuButton(new Rect(x, y, ButtonWidth, ButtonHeight), "Quit")) Quit();

        GUI.Label(new Rect(panel.x, panel.yMax - 34f, panel.width, 24f), "Esc to resume", hintStyle);
    }

    private void DrawEndScreen()
    {
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float panelWidth = 420f;
        float titleHeight = 110f;
        float panelHeight = titleHeight + 2f * ButtonHeight + ButtonGap + 40f;
        Rect panel = new Rect((Screen.width - panelWidth) / 2f, (Screen.height - panelHeight) / 2f, panelWidth, panelHeight);
        GUI.Box(panel, GUIContent.none);

        endTitleStyle.normal.textColor = GameWon ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.4f, 0.4f);
        GUI.Label(new Rect(panel.x, panel.y + 15f, panel.width, titleHeight - 15f), GameWon ? "YOU WIN" : "YOU LOSE", endTitleStyle);

        float x = panel.x + (panelWidth - ButtonWidth) / 2f;
        float y = panel.y + titleHeight;

        if (MenuButton(new Rect(x, y, ButtonWidth, ButtonHeight), "Restart")) Restart();
        y += ButtonHeight + ButtonGap;

        if (MenuButton(new Rect(x, y, ButtonWidth, ButtonHeight), "Quit")) Quit();
    }

    private void DrawOptions()
    {

        bool mouseReleased = Event.current.type == EventType.MouseUp;

        float panelHeight = TitleHeight + 2f * 74f + ButtonHeight + 50f;
        Rect panel = new Rect((Screen.width - PanelWidth) / 2f, (Screen.height - panelHeight) / 2f, PanelWidth, panelHeight);
        GUI.Box(panel, GUIContent.none);

        GUI.Label(new Rect(panel.x, panel.y + 10f, panel.width, TitleHeight - 10f), "OPTIONS", titleStyle);

        float x = panel.x + (PanelWidth - ButtonWidth) / 2f;
        float y = panel.y + TitleHeight;

        SfxManager.MusicVolume = VolumeSlider(new Rect(x, y, ButtonWidth, 60f), "Music", SfxManager.MusicVolume);
        y += 74f;
        SfxManager.SfxVolume = VolumeSlider(new Rect(x, y, ButtonWidth, 60f), "Sound effects", SfxManager.SfxVolume);
        y += 74f;

        if (MenuButton(new Rect(x, y, ButtonWidth, ButtonHeight), "Back")) showOptions = false;

        if (mouseReleased && sliderChanged)
        {
            sliderChanged = false;
            SfxManager.Click();
        }
    }

    private float VolumeSlider(Rect rect, string label, float value)
    {
        GUI.Label(new Rect(rect.x, rect.y, rect.width, 28f), $"{label}: {Mathf.RoundToInt(value * 100f)}%", hintStyleLeft);

        float newValue = GUI.HorizontalSlider(new Rect(rect.x, rect.y + 36f, rect.width, 20f), value, 0f, 1f);
        if (!Mathf.Approximately(newValue, value)) sliderChanged = true;
        return newValue;
    }

    private bool MenuButton(Rect rect, string label)
    {
        if (!GUI.Button(rect, label, buttonStyle)) return false;

        SfxManager.Click();
        return true;
    }

    private void CreateStyles()
    {
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 36, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        titleStyle.normal.textColor = Color.white;

        endTitleStyle = new GUIStyle(GUI.skin.label) { fontSize = 60, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };

        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 22, alignment = TextAnchor.MiddleCenter };

        speedStyle = new GUIStyle(GUI.skin.button) { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };

        hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
        hintStyle.normal.textColor = new Color(1f, 1f, 1f, 0.7f);

        hintStyleLeft = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleLeft };
        hintStyleLeft.normal.textColor = Color.white;
    }
}