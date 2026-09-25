using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class FlameTurret : MonoBehaviour
{

    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float range = 8f;

    
    [SerializeField] private Color outlineColor = Color.red;
    [SerializeField] private float outlineWidth = 0.05f;
    [SerializeField] private int arcSegments = 32;

    
    [SerializeField] private float coneAngle = 40f; 
    [SerializeField] private GameObject flameVisual; 
    [SerializeField] private float damageTickRate = 0.25f; 

    private Transform player;
    private LineRenderer lineRenderer;
    private float tickTimer;

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

        if (flameVisual != null) flameVisual.SetActive(detected);

        if (!detected)
        {
            tickTimer = 0f;
            return;
        }

        tickTimer -= Time.deltaTime;
        if (tickTimer <= 0f)
        {
            tickTimer = damageTickRate;
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null) health.Hit();
        }
    }

    private bool IsPlayerDetected()
    {
        if (DistanceToPlayer() > range) return false;
        return AngleToPlayer() <= coneAngle / 2f;
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

    private void DrawRangeShape()
    {
        lineRenderer.loop = false;
        float halfAngle = coneAngle / 2f;

        Vector3[] points = new Vector3[arcSegments + 3];
        points[0] = Vector3.zero; // turret origin
        for (int i = 0; i <= arcSegments; i++)
        {
            float t = (float)i / arcSegments;
            float currentAngle = -halfAngle + coneAngle * t;
            points[i + 1] = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * range;
        }
        points[points.Length - 1] = Vector3.zero; // close the pie-slice back to the origin

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }
}