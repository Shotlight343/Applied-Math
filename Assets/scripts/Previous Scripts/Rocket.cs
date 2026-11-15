using UnityEngine;


public class Rocket : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float lifetime;
    private float maxDistance;

    private float elapsedTime = 0f;
    private Vector3 spawnPosition;
    private bool initialized = false;


    public void Initialize(Vector3 dir, float spd, float life, float maxDist)
    {
        direction = dir.normalized;
        speed = spd;
        lifetime = life;
        maxDistance = maxDist;
        spawnPosition = transform.position;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

        transform.position += direction * speed * Time.deltaTime;
        elapsedTime += Time.deltaTime;

        float travelled = Vector3.Distance(transform.position, spawnPosition);

        if (elapsedTime >= lifetime || travelled >= maxDistance)
        {
            Destroy(gameObject);
        }
    }
}