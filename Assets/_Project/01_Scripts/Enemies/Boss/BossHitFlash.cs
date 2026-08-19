using System.Collections;
using UnityEngine;

public class BossHitFlash : MonoBehaviour
{
    private SpriteRenderer sr;
    private Material normalMat;
    [SerializeField] private Material flashMat;
    [SerializeField] private float flashTime = 0.1f;

    private Coroutine co;

    private void Awake()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (normalMat == null)
            normalMat = sr.material;
    }

    public void PlayFlash()
    {
        if (co != null)
            StopCoroutine(co);

        co = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        sr.material = flashMat;
        yield return new WaitForSeconds(flashTime);
        sr.material = normalMat;
        co = null;
    }
}
