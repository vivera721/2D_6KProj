using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [Header("Clips")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;
    [SerializeField] private AudioClip dodgeClip;

    [Header("Pitch")]
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.2f;

    [Range(0f, 1f)]
    public float volume = 1f;

    public void PlayFootstep()
    {
        PlayRandom(footstepClips, volume);
    }

    public void PlayAttack()
    {
        PlayOneShot(attackClip, volume);
    }

    public void PlayHit()
    {
        PlayOneShot(hitClip, volume);
    }

    public void PlayJump()
    {
        PlayOneShot(jumpClip, volume);
    }

    public void PlayLand()
    {
        PlayOneShot(landClip, volume);
    }

    public void PlayDodge()
    {
        PlayOneShot(dodgeClip, volume);
    }

    private void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clip, volume);
    }

    private void PlayRandom(AudioClip[] clips, float volume)
    {
        if (clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        PlayOneShot(clip, volume);
    }
}
