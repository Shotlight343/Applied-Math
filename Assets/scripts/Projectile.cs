using UnityEngine;


public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float hitRadius = 0.2f;
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask obstacleMask; 
    [SerializeField] private string creatureTag = "Creature";

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
            if (col.CompareTag(creatureTag))
            {
                Creature creature = col.GetComponent<Creature>();
                if (creature != null) creature.TakeDamage(damage);
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