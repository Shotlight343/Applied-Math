using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemyTypeData
{
    public string name = "Cubey";
    public GameObject prefab;                        
    public int hp = 4;
    public float speed = 3f;                          
    public float scale = 1f;
    public Color tint = new Color(0f, 0f, 0f, 0f);    
}


public class WaveManager : MonoBehaviour
{
    [Header("Spawner")]
    [SerializeField] private CreatureSpawner spawner;

    [Header("Enemy types")]
    [SerializeField] private EnemyTypeData basic = new EnemyTypeData { name = "Basic Cubey", hp = 4, speed = 3f };
    [SerializeField] private EnemyTypeData elite = new EnemyTypeData { name = "Elite Cubey", hp = 8, speed = 1.5f, scale = 1.5f, tint = new Color(0.45f, 0.1f, 0.65f, 1f) };
    [SerializeField] private EnemyTypeData speedy = new EnemyTypeData { name = "Speedy Cubey", hp = 2, speed = 6f, scale = 0.7f, tint = new Color(1f, 0.85f, 0.1f, 1f) };

    [Header("Timing")]
    [SerializeField] private float firstWaveDelay = 5f;
    [SerializeField] private float timeBetweenWaves = 8f;
    [SerializeField] private float spawnInterval = 0.8f;


    private static readonly int[][] WaveCounts =
    {
        new[] { 5, 0, 0 },
        new[] { 10, 0, 0 },
        new[] { 10, 4, 0 },
        new[] { 12, 5, 0 },
        new[] { 14, 5, 2 },
        new[] { 16, 7, 3 },
        new[] { 14, 6, 6 },
        new[] { 8, 11, 3 },
        new[] { 15, 9, 2 },
        new[] { 20, 10, 10 },
    };

    private int currentWave;  
    private float countdown;   
    private GUIStyle labelStyle;

    private bool GameOver => GameManager.Instance != null && GameManager.Instance.IsGameOver;

    private void Start()
    {
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        yield return StartCoroutine(Countdown(firstWaveDelay));

        for (int w = 0; w < WaveCounts.Length; w++)
        {
            if (GameOver) yield break;

            currentWave = w + 1;
            List<EnemyTypeData> queue = BuildQueue(WaveCounts[w]);

           
            int coinMin = currentWave;
            int coinMax = currentWave * 2;

            for (int i = 0; i < queue.Count; i++)
            {
                if (GameOver) yield break;
                Spawn(queue[i], coinMin, coinMax);
                yield return new WaitForSeconds(spawnInterval);
            }

         
            while (Creature.AliveCount > 0)
            {
                if (GameOver) yield break;
                yield return null;
            }

            SfxManager.WaveWin(); 

            if (w < WaveCounts.Length - 1) yield return StartCoroutine(Countdown(timeBetweenWaves));
        }

        if (!GameOver && GameManager.Instance != null) GameManager.Instance.Victory();
    }

    private IEnumerator Countdown(float seconds)
    {
        countdown = seconds;
        while (countdown > 0f)
        {
            if (GameOver) yield break;
            countdown -= Time.deltaTime;
            yield return null;
        }
        countdown = 0f;
    }

    private List<EnemyTypeData> BuildQueue(int[] counts)
    {
        List<EnemyTypeData> queue = new List<EnemyTypeData>();
        for (int i = 0; i < counts[0]; i++) queue.Add(basic);
        for (int i = 0; i < counts[1]; i++) queue.Add(elite);
        for (int i = 0; i < counts[2]; i++) queue.Add(speedy);

    
        for (int i = queue.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            EnemyTypeData temp = queue[i];
            queue[i] = queue[j];
            queue[j] = temp;
        }
        return queue;
    }

    private void Spawn(EnemyTypeData type, int coinMin, int coinMax)
    {
        if (spawner == null)
        {
            Debug.LogWarning("WaveManager: drag your CreatureSpawner into the Spawner field.");
            return;
        }

        spawner.Spawn(type, coinMin, coinMax);
    }

   
    private void OnGUI()
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold };
            labelStyle.normal.textColor = Color.white;
        }

        GameManager gm = GameManager.Instance;
        string text;
        if (gm != null && gm.IsGameOver) text = "GAME OVER";
        else if (gm != null && gm.IsVictory) text = "VICTORY!";
        else if (currentWave == 0) text = $"First wave in {Mathf.CeilToInt(countdown)}s";
        else if (countdown > 0f) text = $"Wave {currentWave} cleared - next wave in {Mathf.CeilToInt(countdown)}s";
        else text = $"Wave {currentWave} / {WaveCounts.Length}   Enemies alive: {Creature.AliveCount}";

        GUI.Label(new Rect(20f, 20f, 900f, 44f), text, labelStyle);
    }
}