using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ShotgunTurret : MonoBehaviour
{
   
    [SerializeField] private string creatureTag = "Creature";
    [SerializeField] private float range = 8f;

    
    [SerializeField] private Color outlineColor = Color.red;
    [SerializeField] private float outlineWidth = 0.05f;
    [SerializeField] private int arcSegments = 32;

   
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int pelletCount = 6;
    [SerializeField] private float spreadAngle = 30f;
    [SerializeField] private float fireCooldown = 1.5f;

    private LineRenderer lineRenderer;
    private float cooldownTimer;

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
        bool detected = target != null && Vector3.Distance(transform.position, target.position) <= range;

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        if (detected && cooldownTimer <= 0f)
        {
            FireBlast(target);
            cooldownTimer = fireCooldown;
        }
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

    private void FireBlast(Transform target)
    {
        if (projectilePrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 baseDir = (target.position - spawnPos).normalized;

        float halfSpread = spreadAngle / 2f;
        for (int i = 0; i < pelletCount; i++)
        {
            float t = pelletCount == 1 ? 0f : (float)i / (pelletCount - 1);
            float angle = -halfSpread + spreadAngle * t;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * baseDir;

            GameObject pellet = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir));
            Projectile proj = pellet.GetComponent<Projectile>();
            if (proj != null) proj.Launch(dir);
        }
    }

    private void DrawRangeShape()
    {
        lineRenderer.loop = true;
        lineRenderer.positionCount = arcSegments;
        for (int i = 0; i < arcSegments; i++)
        {
            float angle = 360f * i / arcSegments;
            Vector3 point = Quaternion.Euler(0, angle, 0) * Vector3.forward * range;
            lineRenderer.SetPosition(i, point);
        }
    }
}