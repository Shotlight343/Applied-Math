using UnityEngine;

public class FinishZone : MonoBehaviour
{
    [SerializeField] private float winRadius = 2f;
 
   
    [SerializeField] private GameObject winPanel;
 
    
   
    [SerializeField] private bool pauseOnWin = true;
 
    private Transform player;
    private bool hasWon = false;
 
    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning($"{name}: No GameObject tagged 'Player' found in scene.");
 
        if (winPanel != null)
            winPanel.SetActive(false);
    }
 
    private void Update()
    {
        if (hasWon || player == null) return;
 
        // Manual distance check -- this replaces OnTriggerEnter.
        float distance = Vector3.Distance(player.position, transform.position);
 
        if (distance <= winRadius)
        {
            TriggerWin();
        }
    }
 
    private void TriggerWin()
    {
        hasWon = true;
 
        if (winPanel != null)
            winPanel.SetActive(true);
 
        if (pauseOnWin)
            Time.timeScale = 0f;
    }
 
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, winRadius);
    }
}