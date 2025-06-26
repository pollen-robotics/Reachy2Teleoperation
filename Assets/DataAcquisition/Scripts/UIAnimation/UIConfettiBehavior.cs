using UnityEngine;
using UnityEngine.UI;

public class UIConfettiBehavior : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float maxDistance;
    private Vector2 startPos;
    private RectTransform rectTransform;

    private float spinSpeed;
    private Image image;
    private float fadeDistance; // Distance from maxDistance where fading starts
    private Color originalColor;

    public void Initialize(Vector2 dir, float spd, float maxDist)
    {
        direction = dir;
        speed = spd;
        maxDistance = maxDist;
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;

        spinSpeed = Random.Range(-90f, 90f); // Optional spin
        image = GetComponent<Image>();
        originalColor = image.color;

        fadeDistance = maxDistance * 0.3f; // Start fading out during the last 30% of travel
    }

    void Update()
    {
        rectTransform.anchoredPosition += direction * speed * Time.deltaTime;
        rectTransform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

        float traveledDistance = Vector2.Distance(startPos, rectTransform.anchoredPosition);

        if (traveledDistance > maxDistance)
        {
            Destroy(gameObject);
        }
        else if (traveledDistance > (maxDistance - fadeDistance))
        {
            float fadeProgress = (traveledDistance - (maxDistance - fadeDistance)) / fadeDistance;
            Color fadedColor = originalColor;
            fadedColor.a = Mathf.Lerp(1f, 0f, fadeProgress);
            image.color = fadedColor;
        }
    }
}
