using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float invulnerabilityDuration = 1f; 

    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool isInvulnerable;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public void Hit()
    {
        if (isInvulnerable) return;
        Respawn();
    }

    private void Respawn()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;

        StartCoroutine(InvulnerabilityWindow());
    }

    private IEnumerator InvulnerabilityWindow()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }
}