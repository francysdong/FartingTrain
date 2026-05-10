using UnityEngine;
using UnityEngine.Events;

public class TrainManager : MonoBehaviour
{
    public static TrainManager Instance { get; private set; }

    [Header("ÐÐÊ»ÉèÖÃ")]
    public float tripDuration = 60f;
    public bool isMoving = true;

    public UnityEvent onArrived;

    private float timer = 0f;
    private bool hasArrived = false;

    public float Progress => timer / tripDuration;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (!isMoving || hasArrived) return;

        timer += Time.deltaTime;

        if (timer >= tripDuration)
        {
            hasArrived = true;
            timer = tripDuration;
            onArrived?.Invoke();
        }
    }

    public void StopTrain()
    {
        isMoving = false;
    }
}