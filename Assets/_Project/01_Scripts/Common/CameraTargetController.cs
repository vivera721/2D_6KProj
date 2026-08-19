using UnityEngine;
using Unity.Cinemachine;

public class CameraTargetController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _camera;

    public void SetFollowTarget(Transform newTarget)
    {
        if (_camera == null) return;

        _camera.Follow = newTarget;
    }
}
