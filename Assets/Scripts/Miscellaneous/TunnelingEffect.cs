using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TunnelingEffect : MonoBehaviour
{
    public Material tunnelingMaterial;
    public GameObject userInputGameObject;

    [SerializeField]
    private float minRadius;
    [SerializeField]
    private float maxRadius;
    [SerializeField]
    private float maxLinearSpeed;
    [SerializeField]
    private float maxAngularSpeed;

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private float smoothedLinearSpeed = 0f;
    private float smoothedAngularSpeed = 0f;

    [Range(0f, 1f)]
    public float smoothingFactor = 0.1f;

    void Start()
    {
        if (userInputGameObject != null)
        {
            lastPosition = userInputGameObject.transform.position;
            lastRotation = userInputGameObject.transform.rotation;
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (tunnelingMaterial != null && userInputGameObject != null)
        {
            float linearSpeed  = (userInputGameObject.transform.position - lastPosition).magnitude / Time.deltaTime;
            lastPosition = userInputGameObject.transform.position;

            Quaternion currentRot = userInputGameObject.transform.rotation;
            Quaternion deltaRot = currentRot * Quaternion.Inverse(lastRotation);
            deltaRot.ToAngleAxis(out float angle, out _);
            float angularSpeed  = angle / Time.deltaTime;
            lastRotation = currentRot;

            smoothedLinearSpeed = Mathf.Lerp(smoothedLinearSpeed, linearSpeed, smoothingFactor);
            smoothedAngularSpeed = Mathf.Lerp(smoothedAngularSpeed, angularSpeed, smoothingFactor);

            float normalizedLinearSpeed = Mathf.Clamp01(smoothedLinearSpeed / maxLinearSpeed);
            float normalizedAngularSpeed = Mathf.Clamp01(smoothedAngularSpeed / maxAngularSpeed);

            float t = Mathf.Max(normalizedLinearSpeed, normalizedAngularSpeed);
            float radius = Mathf.Lerp(minRadius, maxRadius, 1 - t);

            tunnelingMaterial.SetFloat("_Radius", radius);
            Graphics.Blit(src, dest, tunnelingMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
