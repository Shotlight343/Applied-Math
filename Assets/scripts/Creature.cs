using UnityEngine;
using System.Collections.Generic;

// Tag this prefab "Creature". Moves purely via transform using Bezier lerp
// (no physics, no NavMesh). Spawner calls Initialize() with 3 points
// (quadratic) or 4 points (cubic).
public class Creature : MonoBehaviour
{
    [SerializeField] private int maxHP = 5;
    [SerializeField] private GameObject coinPrefab; // UI prefab with a Coin component

    private int currentHP;
    private List<Vector3> points;
    private float travelTime;
    private float t;

    public void Initialize(List<Vector3> controlPoints, float travelDuration)
    {
        points = controlPoints;
        travelTime = Mathf.Max(0.01f, travelDuration);
        currentHP = maxHP;
        t = 0f;
        transform.position = points[0];
    }

    private void Update()
    {
        if (points == null) return;

        t += Time.deltaTime / travelTime;
        float clamped = Mathf.Clamp01(t);

        transform.position = points.Count == 3
            ? BezierUtility.QuadraticLerp(points[0], points[1], points[2], clamped)
            : BezierUtility.CubicLerp(points[0], points[1], points[2], points[3], clamped);

        if (t >= 1f) ReachTarget();
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0) Die();
    }

    private void Die()
    {
        SpawnCoin();
        Destroy(gameObject);
    }

    private void ReachTarget()
    {
        if (GameManager.Instance != null) GameManager.Instance.DamagePlayer(1);
        Destroy(gameObject);
    }

    private void SpawnCoin()
    {
        if (coinPrefab == null || GameManager.Instance == null) return;

        RectTransform canvasRect = GameManager.Instance.UICanvasRect;
        if (canvasRect == null) return;

        Vector2 screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 localPoint);

        GameObject coinObj = Instantiate(coinPrefab, canvasRect);
        RectTransform coinRect = coinObj.GetComponent<RectTransform>();
        if (coinRect != null) coinRect.anchoredPosition = localPoint;

        Coin coin = coinObj.GetComponent<Coin>();
        if (coin != null) coin.Init(GameManager.Instance.BankTargetRect, 1);
    }
}
