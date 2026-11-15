using UnityEngine;
using UnityEngine.SceneManagement;


/// </summary>
public class NoNoZone : MonoBehaviour
{
    [SerializeField] private float warningRadius = 3f;

    [SerializeField] private float dangerRadius = 1.2f;

    [SerializeField] private float dangerTimeLimit = 2f;

    [SerializeField] private float shakeMagnitude = 0.08f;
    [SerializeField] private Color warningColor = Color.red;

    private Transform player;
    private Renderer zoneRenderer;
    private Color originalColor;
    private Vector3 originalPosition;
    private float dangerTimer = 0f;

    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    

        zoneRenderer = GetComponent<Renderer>();
        if (zoneRenderer != null)
            originalColor = zoneRenderer.material.color;

        originalPosition = transform.position;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, originalPosition);

        bool inWarningZone = distance <= warningRadius;
        bool inDangerZone = distance <= dangerRadius;

        if (inDangerZone)
        {
            dangerTimer += Time.deltaTime;
            if (dangerTimer >= dangerTimeLimit)
            {
                RestartScene();
                return;
            }
        }
        else
        {
            dangerTimer = 0f;
        }

        if (inWarningZone)
        {
            ApplyWarningVisuals();
        }
        else
        {
            ResetVisuals();
        }
    }

    private void ApplyWarningVisuals()
    {
        Vector3 shakeOffset = new Vector3(
            Random.Range(-shakeMagnitude, shakeMagnitude),
            0f,
            Random.Range(-shakeMagnitude, shakeMagnitude));

        transform.position = originalPosition + shakeOffset;

        if (zoneRenderer != null)
            zoneRenderer.material.color = warningColor;
    }

    private void ResetVisuals()
    {
        transform.position = originalPosition;

        if (zoneRenderer != null)
            zoneRenderer.material.color = originalColor;
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, warningRadius);
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, dangerRadius);
    }
}