using UnityEngine;

// No Rigidbody, no Collider required on the projectile OR the creature:
// moves via transform and checks for hits by measuring distance from
// each tagged creature to the path it swept this frame.
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float hitRadius = 0.2f;
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask obstacleMask; 
    [SerializeField] private string creatureTag = "Creature";

    private Vector3 direction;
    private Vector3 previousPosition;

    private void Start()
    {
        previousPosition = transform.position;
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector3 dir)
    {
        direction = dir.normalized;
    }

    private void Update()
    {
        previousPosition = transform.position;
        transform.position += direction * speed * Time.deltaTime;
        CheckForHit();
    }

    private void CheckForHit()
    {
        Vector3 movement = transform.position - previousPosition;
        float distance = movement.magnitude;

        
        GameObject[] creatures = GameObject.FindGameObjectsWithTag(creatureTag);
        foreach (GameObject c in creatures)
        {
            float d = DistancePointToSegment(c.transform.position, previousPosition, transform.position);
            if (d <= hitRadius)
            {
                Creature creature = c.GetComponent<Creature>();
                if (creature != null) creature.TakeDamage(damage);
                Destroy(gameObject);
                Debug.Log($"Projectile hit {c.name} for {damage} damage.");
                return;
            }
        }

        
        if (distance > 0f && Physics.SphereCast(previousPosition, hitRadius, movement.normalized, out RaycastHit hit, distance, obstacleMask))
        {
            Destroy(gameObject);
        }
    }

    private float DistancePointToSegment(Vector3 point, Vector3 segStart, Vector3 segEnd)
    {
        Vector3 segment = segEnd - segStart;
        float lenSq = segment.sqrMagnitude;
        if (lenSq < 0.0001f) return Vector3.Distance(point, segStart);

        float t = Mathf.Clamp01(Vector3.Dot(point - segStart, segment) / lenSq);
        Vector3 projection = segStart + t * segment;
        return Vector3.Distance(point, projection);
    }
}