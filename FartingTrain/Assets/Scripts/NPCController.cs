using UnityEngine;
using System.Collections;

public class NPCController : MonoBehaviour
{
    public enum NPCState { Relaxed, Alert, Suspicious, Confirmed }
    public enum PassengerState { OnBoard, MovingToExit, Exited }

    [Header("反应阈值（屁的大小）")]
    public float smallThreshold = 0.3f;
    public float mediumThreshold = 0.8f;
    public float largeThreshold = 1.5f;

    [Header("状态机参数")]
    public float alertToSuspiciousTime = 0.8f;   // 从 2f 改短
    public float suspicionDecayRate = 0.06f;
    public float suspicionFloor = 0.2f;    // 新增：消散下限，闻到过不会完全归零
    public float cooldownDuration = 5f;

    [Header("接触冷却")]
    public float reactionCooldown = 0.8f;

    [Header("下车设置")]
    public float exitLeadTime = 0f;
    public float walkSpeed = 2f;

    private NPCState currentState = NPCState.Relaxed;
    private PassengerState passengerState = PassengerState.OnBoard;
    private float suspicion = 0f;
    private float alertTimer = 0f;
    private bool isInContact = false;
    private float currentFartSize = 0f;
    private float lastReactionTime;
    private int currentReactionLevel = 0;
    private bool isExploded = false;

    private Animator animator;
    private Transform playerTransform;
    private Coroutine cooldownCoroutine;

    private bool hasEscalatedThisContact = false;  // 新增字段


    // ─── 动画名常量（对应你的文件名）──────────────────────────
    const string ANIM_IDLE = "idle";
    const string ANIM_ALERT = "notice_S";   // 循环动画
    const string ANIM_SUSPICIOUS = "notice_M";
    const string ANIM_CONFIRM_R = "notice_L";           // 玩家在右边
    const string ANIM_CONFIRM_L = "notice_L_reversed";  // 玩家在左边
    const string ANIM_WALK = "walk";


    void Start()
    {
        animator = GetComponent<Animator>();

        // 缓存玩家 Transform，避免每帧 Find
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
    }

    void Update()
    {
        if (isExploded) return;
        UpdateStateMachine();
    }

    // ─── 状态机 ───────────────────────────────────────────────
    void UpdateStateMachine()
    {
        switch (currentState)
        {
            case NPCState.Relaxed:
                suspicion = Mathf.Max(0f, suspicion - suspicionDecayRate * Time.deltaTime);
                if (isInContact)
                    EnterState(NPCState.Alert);
                break;

            case NPCState.Alert:


                if (!isInContact)
                {
                    suspicion = Mathf.Max(0f, suspicion - suspicionDecayRate * Time.deltaTime);
                    if (suspicion <= 0f) EnterState(NPCState.Relaxed);
                    break;
                }
                alertTimer += Time.deltaTime;
                suspicion = Mathf.Max(suspicionFloor, suspicion - suspicionDecayRate * Time.deltaTime);
                if (alertTimer >= alertToSuspiciousTime || currentFartSize >= largeThreshold)
                    EnterState(NPCState.Suspicious);
                break;

            case NPCState.Suspicious:
                if (!isInContact)
                {
                    suspicion = Mathf.Max(0f, suspicion - suspicionDecayRate * Time.deltaTime);
                    if (suspicion <= 0f) EnterState(NPCState.Relaxed);
                    break;
                }
                suspicion = Mathf.Max(suspicionFloor, suspicion - suspicionDecayRate * Time.deltaTime);
                if (suspicion >= 1f)
                    EnterState(NPCState.Confirmed);
                break;

            case NPCState.Confirmed:
                break;
        }
    }

    void EnterState(NPCState newState)
    {
        currentState = newState;
        alertTimer = 0f;

        switch (newState)
        {
            case NPCState.Relaxed:
                suspicion = 0f;
                animator.Play(ANIM_IDLE, 0, 0f);
                break;

            case NPCState.Alert:
                animator.Play(ANIM_ALERT, 0, 0f);
                InnocentManager.Instance?.Deduct(1);  // 补上
                break;

            case NPCState.Suspicious:
                animator.Play(ANIM_SUSPICIOUS, 0, 0f);
                InnocentManager.Instance?.Deduct(2);
                break;

            case NPCState.Confirmed:
                // 玩家在右边 → 播 L（指右）
                // 玩家在左边 → 播 L_reversed（指左）
                string confirmAnim = IsPlayerOnRight() ? ANIM_CONFIRM_L : ANIM_CONFIRM_R;
                animator.Play(confirmAnim, 0, 0f);
                InnocentManager.Instance?.Deduct(3);
                if (cooldownCoroutine != null) StopCoroutine(cooldownCoroutine);
                cooldownCoroutine = StartCoroutine(CooldownRoutine());
                break;

        }
    }

    IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(1.2f + cooldownDuration);
        suspicion = 0f;
        isInContact = false;
        currentReactionLevel = 0;
        EnterState(NPCState.Relaxed);
        cooldownCoroutine = null;
    }

    // ─── 外部接口（签名不变，FartEffect 直接对接）────────────
    public void OnFartContact(float fartSize)
    {
        if (isExploded) return;
        if (currentState == NPCState.Confirmed) return;

        currentFartSize = fartSize;
        int newLevel = GetLevel(fartSize);
        if (newLevel == 0) return;

        if (!isInContact)
        {
            // 全新的一次接触
            isInContact = true;
            hasEscalatedThisContact = false;
            currentReactionLevel = newLevel;
            lastReactionTime = Time.time;
            // 第一次接触：Relaxed/Alert → 上升一级
            if (currentState == NPCState.Relaxed)
                EnterState(NPCState.Alert);
            else if (currentState == NPCState.Alert)
                EnterState(NPCState.Suspicious);
            else if (currentState == NPCState.Suspicious)
                EnterState(NPCState.Confirmed);
        }
        else
        {
            // 同一次接触持续触发，什么都不做
            // 状态升级只在 OnFartLeave 之后的新接触里触发
        }
    }

    public void OnFartLeave()
    {
        if (isExploded) return;
        if (currentState == NPCState.Confirmed) return;

        isInContact = false;
        hasEscalatedThisContact = false;  // 重置，等待下次新接触
        currentFartSize = 0f;
        currentReactionLevel = 0;
    }

    public void OnBigFartExplosion()
    {
        isExploded = true;
        currentState = NPCState.Confirmed;
        if (cooldownCoroutine != null) StopCoroutine(cooldownCoroutine);
        // 宇宙大屁：直接用朝向玩家的指认动画
        string confirmAnim = IsPlayerOnRight() ? ANIM_CONFIRM_R : ANIM_CONFIRM_L;
        animator.Play(confirmAnim, 0, 0f);
    }

    // ---- 下车逻辑（由 TrainManager 到站广播触发） ----
    public void BeginExit(Transform doorTarget)
    {
        if (passengerState != PassengerState.OnBoard) return;
        StartCoroutine(ExitRoutine(doorTarget));
    }

    IEnumerator ExitRoutine(Transform doorTarget)
    {
        yield return new WaitForSeconds(exitLeadTime);

        passengerState = PassengerState.MovingToExit;
        float dir = doorTarget.position.x - transform.position.x;
        transform.localScale = new Vector3(
            dir > 0 ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x),
            transform.localScale.y, transform.localScale.z);

        while (Vector3.Distance(transform.position, doorTarget.position) > 0.05f)
        {
            if (currentState == NPCState.Relaxed)
            {
                if (!IsPlaying(ANIM_WALK)) animator.Play(ANIM_WALK, 0, 0f);
                transform.position = Vector3.MoveTowards(transform.position, doorTarget.position, walkSpeed * Time.deltaTime);
            }
            yield return null;
        }

        if (currentState == NPCState.Relaxed) animator.Play(ANIM_IDLE, 0, 0f);
        yield return new WaitUntil(() => TrainManager.Instance != null && TrainManager.Instance.HasArrived);

        passengerState = PassengerState.Exited;
        gameObject.SetActive(false);
    }

    // ─── 工具方法 ─────────────────────────────────────────────
    bool IsPlayerOnRight()
    {
        if (playerTransform == null) return true;
        bool result = playerTransform.position.x > transform.position.x;
        Debug.Log($"Player x: {playerTransform.position.x}, NPC x: {transform.position.x}, IsRight: {result}");
        return result;
    }

    bool IsPlaying(string animName)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(animName);
    }

    int GetLevel(float size)
    {
        if (size >= largeThreshold) return 3;
        if (size >= mediumThreshold) return 2;
        if (size >= smallThreshold) return 1;
        return 0;
    }

    public NPCState CurrentState => currentState;
    public float SuspicionLevel => suspicion;
}