using UnityEngine;


/// </summary>
public class PowerUp : MonoBehaviour
{
    [SerializeField] private float pickupRadius = 1f;

    [SerializeField] private RocketSpawner spawner;

    private Transform player;

    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning($"{name}: No GameObject tagged 'Player' found in scene.");

        if (spawner == null)
            spawner = FindObjectOfType<RocketSpawner>();
    }

    private void Update()
    {
        if (player == null) return;

        // Manual distance check -- this replaces OnTriggerEnter.
        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= pickupRadius)
        {
            Pickup();
        }
    }

    private void Pickup()
    {
        if (spawner != null)
            spawner.IncreaseRocketCount(); // no-op once maxRocketCount is reached

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}