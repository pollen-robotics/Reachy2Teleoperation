using UnityEngine;
using UnityEngine.UI;

public class UIConfettiSpawner : MonoBehaviour
{
    [Header("Setup")]
    public Canvas canvas; // The parent canvas (should be in Screen Space - Overlay or Camera)
    public Sprite[] confettiSprites; // Assign 4 sprites

    [Header("Timing")]
    public float spawnDuration = 2f; // seconds to spawn before stopping
    private float spawnStartTime;

    [Header("Confetti Settings")]
    public GameObject confettiPrefab; // Assign a prefab with Image component
    // public int confettiCount = 100;
    public float minScale = 0.1f;
    public float maxScale = 0.5f;
    public float speed = 40f;
    public float maxDistance = 50f;
    public float spawnInterval = 0.05f;

    private bool isSpawning = false;

    private RectTransform canvasRect;

    private void Start()
    {
        if (canvas == null || confettiPrefab == null || confettiSprites.Length == 0)
        {
            Debug.LogError("ConfettiSpawner not set up properly.");
            return;
        }

        canvasRect = canvas.GetComponent<RectTransform>();
        spawnStartTime = Time.time;
        InvokeRepeating(nameof(SpawnConfetti), 0f, spawnInterval);
        isSpawning = true;
    }

    private void Update()
    {
        if (Time.time - spawnStartTime > spawnDuration)
        {
            CancelInvoke(nameof(SpawnConfetti));
            isSpawning = false;
        }
    }

    public void PlaySpawnConfetti()
    {
        if (!isSpawning)
        {
            spawnStartTime = Time.time;
            InvokeRepeating(nameof(SpawnConfetti), 0f, spawnInterval);
        }
    }

    void SpawnConfetti()
    {
        GameObject confetti = Instantiate(confettiPrefab, canvas.transform);

        Image img = confetti.GetComponent<Image>();
        img.sprite = confettiSprites[Random.Range(0, confettiSprites.Length)];

        RectTransform rt = confetti.GetComponent<RectTransform>();

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            Camera.main.WorldToScreenPoint(transform.position),
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out localPoint
        );
        rt.anchoredPosition = localPoint;

        // Random scale
        float scale = Random.Range(minScale, maxScale);
        rt.localScale = Vector3.one * scale;

        // Random rotation
        float randomRotation = Random.Range(0f, 360f);
        rt.localRotation = Quaternion.Euler(0f, 0f, randomRotation);

        // Random movement direction
        Vector2 dir = Random.insideUnitCircle.normalized;

        UIConfettiBehavior behavior = confetti.AddComponent<UIConfettiBehavior>();
        behavior.Initialize(dir, speed, maxDistance);
    }

}
