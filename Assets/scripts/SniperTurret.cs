using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SniperTurret : MonoBehaviour
{

    [SerializeField] private string creatureTag = "Creature";
    [SerializeField] private float range = 8f;

  
    [SerializeField] private Color outlineColor = Color.red;
    [SerializeField] private float outlineWidth = 0.05f;


    [SerializeField] private float sightAngleTolerance = 2f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float reloadTime = 2f;
    [SerializeField] private LayerMask obstacleMask;

  
    [SerializeField] private float turnSpeed = 5f; 

    private LineRenderer lineRenderer;
    private bool wasDetected;
    private float reloadTimer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = outlineWidth;
        lineRenderer.endWidth = outlineWidth;
        lineRenderer.startColor = outlineColor;
        lineRenderer.endColor = outlineColor;
    }

    private void Start()
    {
        DrawRangeShape();
    }

    private void Update()
    {
        Transform target = FindNearestCreature();

        if (target != null) AimAt(target);

        bool detected = target != null && IsDetected(target);

        if (reloadTimer > 0f) reloadTimer -= Time.deltaTime;

        bool justEntered = detected && !wasDetected;
        bool canFireAgain = detected && reloadTimer <= 0f;

        if ((justEntered || canFireAgain) && target != null)
        {
            Fire(target);
            reloadTimer = reloadTime;
        }

        wasDetected = detected;
    }

    private void AimAt(Transform target)
    {
        Vector3 dir = target.position - transform.position;
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, angle, 0), Time.deltaTime * turnSpeed);
    }

    private bool IsDetected(Transform target)
    {
        if (Vector3.Distance(transform.position, target.position) > range) return false;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        Vector3 forward = transform.forward;
        forward.y = 0f;
        if (Vector3.Angle(forward, dir) > sightAngleTolerance) return false;

        Vector3 origin = transform.position;
        Vector3 rayDir = target.position - origin;
        if (Physics.Raycast(origin, rayDir.normalized, out RaycastHit hit, range, obstacleMask))
        {
            return hit.collider.CompareTag(creatureTag);
        }
        return true;
    }

    private Transform FindNearestCreature()
    {
        GameObject[] creatures = GameObject.FindGameObjectsWithTag(creatureTag);
        Transform nearest = null;
        float nearestDist = Mathf.Infinity;
        foreach (GameObject c in creatures)
        {
            float d = Vector3.Distance(transform.position, c.transform.position);
            if (d < nearestDist) { nearestDist = d; nearest = c.transform; }
        }
        return nearest;
    }

    private void Fire(Transform target)
    {
        if (projectilePrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 direction = (target.position - spawnPos).normalized;

        GameObject bullet = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(direction));
        Projectile proj = bullet.GetComponent<Projectile>();
        if (proj != null) proj.Launch(direction);
    }

    private void DrawRangeShape()
    {
        lineRenderer.loop = false;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.forward * range);
    }
}