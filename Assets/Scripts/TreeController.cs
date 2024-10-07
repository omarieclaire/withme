using System.Collections;
using UnityEngine;

public class TreeController : MonoBehaviour
{
    public Material treeMaterial;
    public Material timeoutMaterial;
    private Vector3 initialScale;

    

    void Start()
    {
        initialScale = transform.localScale;
    }

    public void EnableTree()
    {
        gameObject.SetActive(true);  // Make tree visible
    }

    public void DisableTree()
    {
        gameObject.SetActive(false);  // Make tree invisible
    }

    public void ChangeMaterialToTimeout()
    {
        if (timeoutMaterial != null)
        {
            GetComponent<Renderer>().material = timeoutMaterial;
        }
    }

    public void ResetMaterial()
    {
        if (treeMaterial != null)
        {
            GetComponent<Renderer>().material = treeMaterial;
        }
    }

    public void StartGrowingTree(float growDuration, float targetYScale, float shrinkScale, float finalShrinkDuration)
    {
        StartCoroutine(ScaleTreeOverTime(growDuration, targetYScale, shrinkScale, finalShrinkDuration));
    }

    private IEnumerator ScaleTreeOverTime(float totalDuration, float targetYScale, float shrinkScale, float finalShrinkDuration)
    {
        float currentTime = 0;
        Vector3 targetScale = new Vector3(shrinkScale, targetYScale, shrinkScale);
        float initialPhaseDuration = totalDuration - finalShrinkDuration;

        while (currentTime < totalDuration)
        {
            currentTime += Time.deltaTime;

            if (currentTime <= initialPhaseDuration)
            {
                float normalizedTime = currentTime / initialPhaseDuration;
                transform.localScale = Vector3.Lerp(initialScale, targetScale, normalizedTime);
            }
            else
            {
                float shrinkTime = (currentTime - initialPhaseDuration) / finalShrinkDuration;
                Vector3 zeroScale = Vector3.Lerp(targetScale, Vector3.zero, shrinkTime);
                transform.localScale = zeroScale;
            }

            yield return null;
        }

        transform.localScale = Vector3.zero;
    }
}
