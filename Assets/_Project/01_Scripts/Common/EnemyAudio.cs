using UnityEngine;
using UnityEngine.Rendering;
[RequireComponent(typeof(AudioSource))]
public class EnemyAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [Header("Clips")]
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip attack2Clip;
    [SerializeField] private AudioClip attack3Clip;

    [Header("Pitch")]
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.2f;

    [Range(0f, 1f)]
    public float volume = .5f;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayAttack()
    {
        PlayOneShot(attackClip, volume);
    }

    public void PlayAttack2()
    {
        PlayOneShot(attack2Clip, volume);
    }

    public void PlayAttack3()
    {
        PlayOneShot(attack3Clip, volume);
    }

    public void PlayHit()
    {
        PlayOneShot(hitClip, volume);
    }
    private void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clip, volume);
    }

}
