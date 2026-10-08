using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3 originalPosition;

    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

    public void Shake(float duration, float strength)
    {
        StartCoroutine(ShakeCoroutine(duration, strength));
    }

    private IEnumerator ShakeCoroutine(float duration, float strength)
    {
        float timer = 0f;

        while (timer < duration)
        {
            Vector2 randomPosition = Random.insideUnitCircle * strength;

            transform.localPosition =
                originalPosition +
                new Vector3(randomPosition.x, randomPosition.y, 0f);

            timer += Time.unscaledDeltaTime;

            yield return null;
        }

        transform.localPosition = originalPosition;
    }
}