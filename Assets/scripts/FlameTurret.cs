using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FlameTurret : MonoBehaviour, ILevelable
{
  
    [SerializeField] private string creatureTag = "Creature";
    [SerializeField] private float range = 8f;

   
    [SerializeField] private Color outlineColor = Color.red;
    [SerializeField] private float outlineWidth = 0.05f;
    [SerializeField] private int arcSegments = 32;

    [SerializeField] private float coneAngle = 40f;
    [SerializeField] private GameObject flameVisual;


    [SerializeField] private float turnSpeed = 5f; 

  
    [SerializeField] private int[] damagePerTickByLevel = { 1, 1, 2 };
    [SerializeField] private float[] tickRateByLevel = { 0.5f, 0.4f, 0.25f }; 

    private LineRenderer lineRenderer;
    private float tickTimer;
    private int level = 1;

    private int CurrentDamage => damagePerTickByLevel[Mathf.Clamp(level - 1, 0, damagePerTickByLevel.Length - 1)];
    private float CurrentTickRate => tickRateByLevel[Mathf.Clamp(level - 1, 0, tickRateByLevel.Length - 1)];

    public void ApplyLevel(int newLevel)
    {
        level = newLevel;
    }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = outlineWidth;
        lineRenderer.endWidth = outlineWidth;
        lineRenderer.startColor = outlineColor;
        lineRenderer.endColor = outlineColor;

        DrawRangeShape();
    }

    private void Update()
    {
        GameObject[] creatures = GameObject.FindGameObjectsWithTag(creatureTag);

       
        Transform nearest = null;
        float nearestDist = Mathf.Infinity;
        foreach (GameObject c in creatures)
        {
            float d = Vector3.Distance(transform.position, c.transform.position);
            if (d < nearestDist) { nearestDist = d; nearest = c.transform; }
        }
        if (nearest != null && nearestDist <= range) AimAt(nearest);

        tickTimer -= Time.deltaTime;
        bool tickDue = tickTimer <= 0f;
        bool anyInCone = false;

        foreach (GameObject c in creatures)
        {
            if (!IsDetected(c.transform)) continue;

            anyInCone = true;
            if (tickDue)
            {
                Creature creature = c.GetComponent<Creature>();
                if (creature != null) creature.TakeDamage(CurrentDamage);
            }
        }

        if (flameVisual != null) flameVisual.SetActive(anyInCone);

        
        if (tickDue) tickTimer = anyInCone ? CurrentTickRate : 0f;
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
        return Vector3.Angle(forward, dir) <= coneAngle / 2f;
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