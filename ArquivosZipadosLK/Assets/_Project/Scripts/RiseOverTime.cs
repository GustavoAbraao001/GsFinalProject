using UnityEngine;


public sealed class RiseOverTime : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField, Min(0f)] private float riseDistance = 1f;
    [SerializeField, Min(0.01f)] private float riseSpeed = 0.5f;
    [SerializeField, Min(0f)] private float startDelay = 0f;

    private Vector3 originPosition;   
    private Vector3 targetPosition;
    private float delayRemaining;
    private bool isRising;
    private bool originCaptured;

    public bool IsRising => isRising;

    private void Awake()
    {
        CaptureOrigin();
    }

    private void OnEnable()
    {
        CaptureOrigin();
        BeginRise();
    }

    private void OnDisable()
    {
        isRising = false;

        if (originCaptured)
            transform.position = originPosition;
    }

    private void Update()
    {
        if (!isRising)
            return;

        if (delayRemaining > 0f)
        {
            delayRemaining -= Time.deltaTime;
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            riseSpeed * Time.deltaTime
        );

        if ((transform.position - targetPosition).sqrMagnitude < 0.000001f)
        {
            transform.position = targetPosition;
            isRising = false;
        }
    }

    public void RestartRise()
    {
        BeginRise();
    }

    public void ResetToStart()
    {
        CaptureOrigin();
        transform.position = originPosition;
        isRising = false;
    }

    private void CaptureOrigin()
    {
        if (originCaptured)
            return;

        originPosition = transform.position;
        originCaptured = true;
    }

    private void BeginRise()
    {
        transform.position = originPosition;
        targetPosition = originPosition + Vector3.up * riseDistance;
        delayRemaining = startDelay;
        isRising = riseDistance > 0f;
    }
}