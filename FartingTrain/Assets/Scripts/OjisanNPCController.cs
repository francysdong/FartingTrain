using UnityEngine;
using System.Collections;

public class OjisanNPCController : NPCController
{
    public enum DozeState { Awake, Yawning, Asleep, Shocked }

    [Header("打盹设置")]
    public float dozeCheckInterval = 5f;
    public float dozeChance = 0.3f;
    public float wakeChance = 0.3f;
    public float yawnDuration = 1f;
    public float shockDuration = 0.6f;
    public float sleepSensitivityMultiplier = 2f;
    public float sleepDeductionMultiplier = 1.5f;

    const string ANIM_YAWN = "yawn";
    const string ANIM_SLEEP = "sleep";
    const string ANIM_SHOCK = "shock";

    private DozeState dozeState = DozeState.Awake;
    private float dozeCheckTimer = 0f;
    private Coroutine dozeCoroutine;

    protected override void OnUpdateExtra()
    {
        if (currentState != NPCState.Relaxed || passengerState != PassengerState.OnBoard) return;

        if (dozeState == DozeState.Awake)
        {
            dozeCheckTimer += Time.deltaTime;
            if (dozeCheckTimer < dozeCheckInterval) return;
            dozeCheckTimer = 0f;

            if (Random.value < dozeChance)
                dozeCoroutine = StartCoroutine(DozeRoutine());
        }
        else if (dozeState == DozeState.Asleep)
        {
            dozeCheckTimer += Time.deltaTime;
            if (dozeCheckTimer < dozeCheckInterval) return;
            dozeCheckTimer = 0f;

            if (Random.value < wakeChance)
                WakeNaturally();
        }
    }

    protected override bool CanWalk => dozeState != DozeState.Shocked;

    void WakeNaturally()
    {
        dozeState = DozeState.Awake;
        animator.Play(ANIM_IDLE, 0, 0f);
    }

    IEnumerator DozeRoutine()
    {
        dozeState = DozeState.Yawning;
        animator.Play(ANIM_YAWN, 0, 0f);
        yield return new WaitForSeconds(yawnDuration);

        dozeState = DozeState.Asleep;
        animator.Play(ANIM_SLEEP, 0, 0f);
    }

    void CancelDoze()
    {
        if (dozeCoroutine != null) { StopCoroutine(dozeCoroutine); dozeCoroutine = null; }
        dozeState = DozeState.Awake;
    }

    public override void OnFartContact(float fartSize)
    {
        if (dozeState == DozeState.Shocked) return;

        if (dozeState == DozeState.Asleep)
        {
            if (GetLevel(fartSize, sleepSensitivityMultiplier) == 0) return;
            CancelDoze();
            dozeState = DozeState.Shocked;
            StartCoroutine(ShockAndConfirmRoutine());
            return;
        }

        if (dozeState == DozeState.Yawning) CancelDoze();
        base.OnFartContact(fartSize);
    }

    IEnumerator ShockAndConfirmRoutine()
    {
        animator.Play(ANIM_SHOCK, 0, 0f);
        yield return new WaitForSeconds(shockDuration);
        dozeState = DozeState.Awake;
        EnterState(NPCState.Confirmed, 3, sleepDeductionMultiplier);
    }

    public override void BeginExit(Transform doorTarget)
    {
        // Shocked 不在这里取消：正在被抓包的演出不该被下车广播打断，
        // 交给 CanWalk 挡住走路逻辑，等 ShockAndConfirmRoutine 自己收尾
        if (dozeState == DozeState.Yawning || dozeState == DozeState.Asleep)
        {
            CancelDoze();
            if (exitLeadTime > 0f) animator.Play(ANIM_IDLE, 0, 0f);
        }
        base.BeginExit(doorTarget);
    }
}
