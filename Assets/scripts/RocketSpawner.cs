using UnityEngine;


public class RocketSpawner : MonoBehaviour
{
  
    [Tooltip("Seconds between barrages.")]
    [SerializeField] private float fireInterval = 3f;

  
    [SerializeField] private GameObject rocketPrefab;
    
    [SerializeField] private int rocketCount = 0;
    [SerializeField] private int maxRocketCount = 8;
    [SerializeField] private float rocketSpeed = 8f;
    [SerializeField] private float rocketLifetime = 5f;
    [SerializeField] private float rocketMaxDistance = 30f;

    private Transform player;
    private float timer = 0f;

    public int RocketCount => rocketCount;
    public int MaxRocketCount => maxRocketCount;

    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning($"{name}: No GameObject tagged 'Player' found in scene.");
    }

    private void Update()
    {
    
        if (rocketCount <= 0) return;

        timer += Time.deltaTime;
        if (timer >= fireInterval)
        {
            timer = 0f;
            FireBarrage();
        }
    }

    private void FireBarrage()
    {
        if (rocketPrefab == null || player == null) return;

        Vector3 origin = player.position;
        float spacing = 360f / rocketCount;
        float firstOffset = spacing / 2f; // half-spacing offset (matches the 4-rocket / 45deg example)

        for (int i = 0; i < rocketCount; i++)
        {
            float angleDeg = firstOffset + i * spacing;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            // X/Z plane direction -- swap to (cos, sin, 0) for a 2D (X/Y) setup.
            Vector3 direction = new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad));

            GameObject rocketObj = Instantiate(rocketPrefab, origin, Quaternion.identity);
            Rocket rocket = rocketObj.GetComponent<Rocket>();
            if (rocket != null)
                rocket.Initialize(direction, rocketSpeed, rocketLifetime, rocketMaxDistance);
        }
    }

    /// <summary>Called by PowerUp.cs on pickup. Increases rocketCount, capped at maxRocketCount.</summary>
    public void IncreaseRocketCount()
    {
        if (rocketCount < maxRocketCount)
            rocketCount++;
        // Pickup beyond cap: no effect -- rocketCount simply stays at maxRocketCount.
    }
}