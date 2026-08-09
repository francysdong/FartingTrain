using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public class StationExitEntry
{
    public NPCController npc;
    public Transform doorTarget;
}

public class TrainManager : MonoBehaviour
{
    public static TrainManager Instance { get; private set; }

    [Header("–– ª…Ë÷√")]
    public float tripDuration = 60f;
    public bool isMoving = true;

    public UnityEvent onArrived;

    [Header("µΩ’æ≈‰÷√")]
    public List<StationExitEntry> exitingNpcs = new List<StationExitEntry>();
    public float approachLeadTime = 5f;
    public UnityEvent onApproachingStation;

    private float timer = 0f;
    private bool hasArrived = false;
    private bool hasAnnouncedApproach = false;

    public float Progress => timer / tripDuration;
    public bool HasArrived => hasArrived;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (!isMoving || hasArrived) return;

        timer += Time.deltaTime;

        if (!hasAnnouncedApproach && timer >= tripDuration - approachLeadTime)
        {
            hasAnnouncedApproach = true;
            onApproachingStation?.Invoke();
            foreach (var entry in exitingNpcs)
            {
                if (entry.npc != null) entry.npc.BeginExit(entry.doorTarget);
            }
        }

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