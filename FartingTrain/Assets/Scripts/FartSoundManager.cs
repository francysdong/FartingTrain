using UnityEngine;

public class FartSoundManager : MonoBehaviour
{
    public static FartSoundManager Instance { get; private set; }

    [Header("小屁音效池")]
    public AudioClip[] smallFartSounds;

    [Header("大屁音效池")]
    public AudioClip[] bigFartSounds;

    [Header("阈值")]
    [Range(0f, 1f)]
    public float bigFartThreshold = 0.7f;

    private AudioSource audioSource;
    private int lastSmallIndex = -1;
    private int lastBigIndex = -1;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    public void PlayFartSound(float chargeRatio)
    {
        if (chargeRatio >= bigFartThreshold)
            PlayFromPool(bigFartSounds, ref lastBigIndex);
        else
            PlayFromPool(smallFartSounds, ref lastSmallIndex);
    }

    void PlayFromPool(AudioClip[] pool, ref int lastIndex)
    {
        if (pool == null || pool.Length == 0) return;

        int index;
        do
        {
            index = Random.Range(0, pool.Length);
        } while (pool.Length > 1 && index == lastIndex);

        lastIndex = index;
        audioSource.PlayOneShot(pool[index]);
    }
}