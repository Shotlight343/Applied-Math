using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Assign two Image components (type = Filled, Fill Method = Horizontal)
// stacked on top of each other: ghostFillImage behind, realFillImage in front.
public class HPBarUI : MonoBehaviour
{
    [SerializeField] private Image realFillImage;
    [SerializeField] private Image ghostFillImage;
    [SerializeField] private float ghostDelay = 0.4f;    // how long the ghost bar holds before easing down
    [SerializeField] private float ghostDuration = 0.5f;

    private Coroutine ghostRoutine;

    public void SetHP(int current, int max, bool snapGhost = false)
    {
        float targetFill = max > 0 ? (float)current / max : 0f;

        if (realFillImage != null) realFillImage.fillAmount = targetFill; // real bar snaps immediately

        if (snapGhost)
        {
            if (ghostFillImage != null) ghostFillImage.fillAmount = targetFill;
            return;
        }

        if (ghostRoutine != null) StopCoroutine(ghostRoutine);
        ghostRoutine = StartCoroutine(AnimateGhost(targetFill));
    }

    private IEnumerator AnimateGhost(float targetFill)
    {
        if (ghostFillImage == null) yield break;

        yield return new WaitForSeconds(ghostDelay);

        float startFill = ghostFillImage.fillAmount;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / ghostDuration;
            float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 2f); // ease-out
            ghostFillImage.fillAmount = Mathf.Lerp(startFill, targetFill, eased);
            yield return null;
        }

        ghostFillImage.fillAmount = targetFill;
    }
}
