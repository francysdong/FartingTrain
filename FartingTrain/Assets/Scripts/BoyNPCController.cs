using UnityEngine;

public class BoyNPCController : NPCController
{
    public enum HeadphoneState { Off, On }
    public enum Posture { Sitting, Standing }

    [Header("耳机设置")]
    public float headphoneCheckInterval = 5f;
    public float putOnChance = 0.3f;
    public float takeOffChance = 0.3f;
    public float headphoneSoundDampen = 0.3f;   // 戴耳机时在soundSensitivity基础上再乘这个（<1更迟钝）

    [Header("站坐设置")]
    public Posture posture = Posture.Sitting;   // 直接就是初始状态，Inspector里选

    const string ANIM_EARPHONE = "earphone";

    private HeadphoneState headphoneState = HeadphoneState.Off;
    private float checkTimer = 0f;

    protected override void OnStartExtra() =>
        animator.Play(ResolveAnimName(ANIM_IDLE), 0, 0f);

    protected override string ResolveAnimName(string baseName) =>
        posture == Posture.Standing ? baseName + "_stand" : baseName;

    protected override void OnUpdateExtra()
    {
        if (currentState != NPCState.Relaxed || passengerState != PassengerState.OnBoard) return;

        checkTimer += Time.deltaTime;
        if (checkTimer < headphoneCheckInterval) return;
        checkTimer = 0f;

        if (headphoneState == HeadphoneState.Off && Random.value < putOnChance)
        {
            headphoneState = HeadphoneState.On;
            animator.Play(ANIM_EARPHONE, 0, 0f);
        }
        else if (headphoneState == HeadphoneState.On && Random.value < takeOffChance)
        {
            headphoneState = HeadphoneState.Off;
            animator.Play(ResolveAnimName(ANIM_IDLE), 0, 0f);
        }
    }

    protected override float EffectiveSoundSensitivity =>
        soundSensitivity * (headphoneState == HeadphoneState.On ? headphoneSoundDampen : 1f);

    public override void BeginExit(Transform doorTarget)
    {
        posture = Posture.Standing;
        if (currentState == NPCState.Relaxed) animator.Play(ResolveAnimName(ANIM_IDLE), 0, 0f);
        base.BeginExit(doorTarget);
    }
}
