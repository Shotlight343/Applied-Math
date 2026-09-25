using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class ShotgunTurret : MonoBehaviour
{

    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float range = 8f;


    [SerializeField] private Color outlineColor = Color.red;
    [SerializeField] private float outlineWidth = 0.05f;
    [SerializeField] private int arcSegments = 32;


    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int pelletCount = 6;
    [SerializeField] private float spreadAngle = 30f; // total spread the pellets fan across
    [SerializeField] private float fireCooldown = 1.5f;

    private Transform player;
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

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null) player = playerObj.transform;
    }

    private void Start()
    {
        DrawRangeShape();
    }

    private void Update()
    {
        if (player == null) return;

        bool detected = DistanceToPlayer() <= range; // full radius, no angle restriction

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        if (detected && cooldownTimer <= 0f)
        {
            FireBlast();
            cooldownTimer = fireCooldown;
        }
    }

    private float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, player.position);
    }

    private void FireBlast()
    {
        if (projectilePrefab == null || player == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 baseDir = (player.position - spawnPos).normalized;

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