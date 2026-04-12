using UnityEngine;

public class NPCController : MonoBehaviour
{
    private Animator animator;

    [Header("反应阈值（屁的大小）")]
    public float smallThreshold = 0.3f;
    public float mediumThreshold = 0.8f;
    public float largeThreshold = 1.5f;

    [Header("冷却时间")]
    public float reactionCooldown = 0.8f;

    private float lastReactionTime;
    private int currentReactionLevel = 0;
    private bool isInContact = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnFartContact(float fartSize)
    {
        int newLevel = GetLevel(fartSize);

        // 还没到最小阈值就忽略
        if (newLevel == 0) return;

        if (!isInContact)
        {
            isInContact = true;
            currentReactionLevel = newLevel;
            lastReactionTime = Time.time;
            TriggerReaction(newLevel);
            return;
        }

        if (newLevel != currentReactionLevel && Time.time - lastReactionTime >= reactionCooldown)
        {
            currentReactionLevel = newLevel;
            lastReactionTime = Time.time;
            TriggerReaction(newLevel);
        }
    }

    public void OnFartLeave()
    {
        isInContact = false;
        currentReactionLevel = 0;
        animator.Play("Idle", 0, 0f);   // 屁离开时才回 Idle
    }

    int GetLevel(float size)
    {
        if (size >= largeThreshold) return 3;
        if (size >= mediumThreshold) return 2;
        if (size >= smallThreshold) return 1;
        return 0;
    }

    void TriggerReaction(int level)
    {
        if (level == 1) animator.Play("ReactSmall", 0, 0f);
        else if (level == 2) animator.Play("ReactMedium", 0, 0f);
        else if (level == 3) animator.Play("ReactLarge", 0, 0f);
    }
}