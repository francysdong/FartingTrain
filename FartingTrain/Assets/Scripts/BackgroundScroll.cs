using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{
    [Header("滚动设置")]
    public float scrollSpeed = 2f;
    public float resetX = -20f;
    public float startX = 20f;

    [Header("晃动设置")]
    public float shakeInterval = 5f;
    public float shakeDuration = 0.5f;
    public float shakeIntensity = 0.1f;

    private float nextShakeTime;
    private float shakeTimer = 0f;
    private bool isShaking = false;
    private Vector3 originalPos;

    void Start()
    {
        originalPos = transform.position;
        nextShakeTime = Random.Range(shakeInterval * 0.5f, shakeInterval);
    }

    void Update()
    {
        if (TrainManager.Instance != null && !TrainManager.Instance.isMoving) return;

        // 滚动
        transform.position += Vector3.left * scrollSpeed * Time.deltaTime;

        if (transform.position.x <= resetX)
            transform.position = new Vector3(startX, transform.position.y, transform.position.z);

        // 晃动
        HandleShake();
    }

    void HandleShake()
    {
        if (!isShaking)
        {
            nextShakeTime -= Time.deltaTime;
            if (nextShakeTime <= 0f)
            {
                isShaking = true;
                shakeTimer = shakeDuration;
                nextShakeTime = Random.Range(shakeInterval * 0.5f, shakeInterval);
            }
        }
        else
        {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer > 0f)
            {
                transform.position = new Vector3(
                    transform.position.x + Random.Range(-shakeIntensity, shakeIntensity),
                    originalPos.y + Random.Range(-shakeIntensity, shakeIntensity),
                    transform.position.z
                );
            }
            else
            {
                isShaking = false;
            }
        }
    }
}