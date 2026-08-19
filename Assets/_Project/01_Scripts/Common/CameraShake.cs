using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource impulseSource;

    [Header("Shake Forces")]
    [SerializeField] private float lightShake = 0.5f;
    [SerializeField] private float normalShake = 1f;
    [SerializeField] private float strongShake = 2f;

    private void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }


    public void ShakeLight()
    {
        Shake(lightShake);
    }

    public void ShakeNormal()
    {
        Shake(normalShake);
    }

    public void ShakeStrong()
    {
        Shake(strongShake);
    }

    private void Shake(float shakeForce)
    {
        if (impulseSource == null) return;

        impulseSource.GenerateImpulseWithForce(shakeForce);
    }
}
