using UnityEngine;
using System.Collections.Generic;


public class Creature : MonoBehaviour
{
    
    public static int AliveCount { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        AliveCount = 0; 
    }

    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private float coinScatter = 40f; 

    private int currentHP;
    private int coinMin;
    private int coinMax;
    private bool isDead;
    private bool counted;

    private List<Vector3> points;
    private float moveSpeed;
    private int segmentIndex;
    private float segmentT;
    private float segmentLength;

    public void Initialize(List<Vector3> waypoints, float speed, int hp, int dropMin, int dropMax, Color tint)
    {
        points = waypoints;
        moveSpeed = Mathf.Max(0.01f, speed);
        currentHP = hp;
        coinMin = dropMin;
        coinMax = Mathf.Max(dropMin, dropMax);
        segmentIndex = 0;
        segmentT = 0f;
        transform.position = points[0];
        UpdateSegmentLength();

        
        if (tint.a > 0f)
        {
            Renderer r = GetComponentInChildren<Renderer>();
            if (r != null) r.material.color = tint;
        }

        if (!counted)
        {
            counted = true;
            AliveCount++;
        }
    }

    private void OnDestroy()
    {
        if (counted) AliveCount--;
    }

    private void Update()
    {
        if (points == null || isDead) return;

        segmentT += (moveSpeed * Time.deltaTime) / segmentLength;

        
        while (segmentT >= 1f)
        {
            segmentIndex++;
            if (segmentIndex >= points.Count - 1)
            {
                transform.position = points[points.Count - 1];
                ReachTarget();
                return;
            }
            float overshoot = (segmentT - 1f) * segmentLength;
            UpdateSegmentLength();
            segmentT = overshoot / segmentLength;
        }

        transform.position = Vector3.Lerp(points[segmentIndex], points[segmentIndex + 1], segmentT);
    }

    private void UpdateSegmentLength()
    {
        segmentLength = Mathf.Max(0.0001f, Vector3.Distance(points[segmentIndex], points[segmentIndex + 1]));
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;     

        currentHP -= amount;
        if (currentHP <= 0) Die();
    }

    private void Die()
    {
        isDead = true;
        SpawnCoins();
        Destroy(gameObject);
    }

    private void ReachTarget()
    {
        isDead = true;
        if (GameManager.Instance != null) GameManager.Instance.DamagePlayer(1);
        Destroy(gameObject);
    }

    private void SpawnCoins()
    {
        if (coinPrefab == null || GameManager.Instance == null) return;

        RectTransform canvasRect = GameManager.Instance.UICanvasRect;
        if (canvasRect == null || Camera.main == null) return;

        Vector2 screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 origin);

        // Each kill drops a random number of coins from this wave's range, each worth 1
        int count = Random.Range(coinMin, coinMax + 1);
        for (int i = 0; i < count; i++)
        {
            GameObject coinObj = Instantiate(coinPrefab, canvasRect);
            RectTransform coinRect = coinObj.GetComponent<RectTransform>();
            if (coinRect != null) coinRect.anchoredPosition = origin + Random.insideUnitCircle * coinScatter;

            Coin coin = coinObj.GetComponent<Coin>();
            if (coin != null) coin.Init(GameManager.Instance.BankTargetRect, 1);
        }
    }
}