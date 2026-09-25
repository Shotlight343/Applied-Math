using UnityEngine; 
using System.Collections;


[RequireComponent(typeof(LineRenderer))]
public class SniperTurret : MonoBehaviour
{
    
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float range = 8f;

  
    [SerializeField] private Color outlineColor = Color.red;
    [SerializeField] private float outlineWidth = 0.05f;

   
    [SerializeField] private float sightAngleTolerance = 2f; 
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint; 
    [SerializeField] private float reloadTime = 2f; 
    [SerializeField] private LayerMask obstacleMask;

    private Transform player;
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

        bool detected = IsPlayerDetected();

        if (reloadTimer > 0f) reloadTimer -= Time.deltaTime;

        
        bool justEntered = detected && !wasDetected;
        bool canFireAgain = detected && reloadTimer <= 0f;

        if (justEntered || canFireAgain)
        {
            Fire();
            reloadTimer = reloadTime;
        }

        wasDetected = detected;
    }

    private bool IsPlayerDetected()
    {
        if (DistanceToPlayer() > range) return false;
        if (AngleToPlayer() > sightAngleTolerance) return false;

        Vector3 origin = transform.position;
        Vector3 dir = player.position - origin;
        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, range, obstacleMask))
        {
            return hit.collider.CompareTag(playerTag);
        }
        return true; 
    }

    private float AngleToPlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        Vector3 forward = transform.forward;
        forward.y = 0f;
        return Vector3.Angle(forward, dir);
    }

    private float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, player.position);
    }

    private void Fire()
    {
        if (projectilePrefab == null || player == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 direction = (player.position - spawnPos).normalized;

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