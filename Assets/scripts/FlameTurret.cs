using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FlameTurret : MonoBehaviour
{
    
    [SerializeField] private string creatureTag = "Creature";
    [SerializeField] private float range = 8f;

    [SerializeField] private Color outlineColor = Color.red;
    [SerializeField] private float outlineWidth = 0.05f;
    [SerializeField] private int arcSegments = 32;

    
    [SerializeField] private float coneAngle = 40f;
    [SerializeField] private GameObject flameVisual;
    [SerializeField] private float damageTickRate = 0.25f;
    [SerializeField] private int damagePerTick = 1;

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
    }

    private void Start()
    {
        DrawRangeShape();
    }

    private void Update()
    {
        Transform target = FindNearestCreature();
        bool detected = target != null && IsDetected(target);

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
            Creature creature = target.GetComponent<Creature>();
            if (creature != null) creature.TakeDamage(damagePerTick);
        }
    }

    private bool IsDetected(Transform target)
    {
        if (Vector3.Distance(transform.position, target.position) > range) return false;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        Vector3 forward = transform.forward;
        forward.y = 0f;
        return Vector3.Angle(forward, dir) <= coneAngle / 2f;
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

    private void DrawRangeShape()
    {
        lineRenderer.loop = false;
        float halfAngle = coneAngle / 2f;

        Vector3[] points = new Vector3[arcSegments + 3];
        points[0] = Vector3.zero;
        for (int i = 0; i <= arcSegments; i++)
        {
            float t = (float)i / arcSegments;
            float currentAngle = -halfAngle + coneAngle * t;
            points[i + 1] = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * range;
        }
        points[points.Length - 1] = Vector3.zero;

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }
}