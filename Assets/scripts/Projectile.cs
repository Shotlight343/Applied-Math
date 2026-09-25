using UnityEngine;


public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float hitRadius = 0.2f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private string playerTag = "Player";

    private Vector3 direction;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector3 dir)
    {
        direction = dir.normalized;
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        CheckForHit();
    }

    private void CheckForHit()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius);
        foreach (Collider col in hits)
        {
            if (col.CompareTag(playerTag))
            {
                PlayerHealth health = col.GetComponent<PlayerHealth>();
                if (health != null) health.Hit();
                Destroy(gameObject);
                return;
            }

            if (((1 << col.gameObject.layer) & obstacleMask) != 0)
            {
               
                Destroy(gameObject);
                return;
            }
        }
    }
}