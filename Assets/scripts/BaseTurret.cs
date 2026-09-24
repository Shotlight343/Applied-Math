using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public abstract class BaseTurret : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] protected string playerTag = "Player";
    [SerializeField] protected float range = 8f;

    [Header("Range Outline")]
    [SerializeField] protected Color outlineColor = Color.red;
    [SerializeField] protected float outlineWidth = 0.05f;
    [SerializeField] protected int arcSegments = 32; // resolution for cone/circle shapes

    protected Transform player;
    protected LineRenderer lineRenderer;
    protected bool playerInRange;

    protected virtual void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false; // points are relative to the turret, so the outline follows its position/rotation
        lineRenderer.startWidth = outlineWidth;
        lineRenderer.endWidth = outlineWidth;
        lineRenderer.startColor = outlineColor;
        lineRenderer.endColor = outlineColor;

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null) player = playerObj.transform;
    }

    protected virtual void Start()
    {
        DrawRangeShape();
    }

    protected virtual void Update()
    {
        if (player == null) return;

        playerInRange = IsPlayerDetected();
        OnDetectionUpdate(playerInRange);
    }

    /// Each turret defines its own detection rule (cone angle, line of sight, radius).
    protected abstract bool IsPlayerDetected();

    /// Each turret defines what happens while the player is / isn't detected (firing logic).
    protected abstract void OnDetectionUpdate(bool detected);

    /// Each turret draws its own outline shape (cone, line, circle) using lineRenderer.
    protected abstract void DrawRangeShape();

    /// Angle (degrees) between the turret's forward direction and the player, flattened to the X/Z plane.
    protected float AngleToPlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        Vector3 forward = transform.forward;
        forward.y = 0f;
        return Vector3.Angle(forward, dir);
    }

    protected float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, player.position);
    }
}