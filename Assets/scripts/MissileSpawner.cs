using UnityEngine;

public class MissileSpawner : MonoBehaviour
{
  
    public PlayerController player;
    public HomingMissile missilePrefab;   
    public Camera cam;                    

  
    public float spawnInterval = 3f;      
    public int startingMissiles = 1;      
    public int missilesAddedPerStep = 1;  
    public float difficultyStep = 10f;

   
    
    public float offscreenMargin = 0.15f;

    private float survivalTime;
    private float spawnTimer;

    private void Start()
    {
        if (cam == null) cam = Camera.main;
        spawnTimer = spawnInterval;
    }

    private void Update()
    {
        if (player == null || cam == null) return;

        survivalTime += Time.deltaTime;
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            spawnTimer = spawnInterval;
            SpawnWave();
        }
    }

    private int CurrentMissileCount()
    {
        int steps = Mathf.FloorToInt(survivalTime / difficultyStep);
        return startingMissiles + steps * missilesAddedPerStep;
    }

    private void SpawnWave()
    {
        int count = CurrentMissileCount();
        for (int i = 0; i < count; i++)
            SpawnOne();
    }

    private void SpawnOne()
    {
        Vector3 pos = GetOffscreenPosition();

        HomingMissile missile;
        if (missilePrefab != null)
        {
            missile = Instantiate(missilePrefab, pos, Quaternion.identity);
        }
        else
        {
            
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.transform.localScale = new Vector3(0.4f, 0.4f, 0.8f);
            Destroy(go.GetComponent<Collider>()); 
            go.transform.position = pos;
            missile = go.AddComponent<HomingMissile>();
        }

        missile.Init(player);
    }

    
    private Vector3 GetOffscreenPosition()
    {
        float angle = Random.Range(0f, 360f);
        Vector3 dir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.up;

       
        float radius = (0.5f + offscreenMargin) / 0.7071f;
        Vector2 viewport = new Vector2(0.5f, 0.5f) + new Vector2(dir.x, dir.y) * radius;

        
        float depth = Vector3.Dot(player.transform.position - cam.transform.position, cam.transform.forward);
        depth = Mathf.Max(depth, cam.nearClipPlane + 1f);

        return cam.ViewportToWorldPoint(new Vector3(viewport.x, viewport.y, depth));
    }
}
