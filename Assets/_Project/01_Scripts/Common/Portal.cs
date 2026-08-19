using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Portal : MonoBehaviour
{
    Animator animator;
    StageFlowManager stageFlowManager;

    [SerializeField] private KeyCode interactKey = KeyCode.UpArrow;

    private bool playerInRange;
    private bool isActivated;

    [SerializeField]private DOTweenAnimation sceneFade;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        stageFlowManager = FindAnyObjectByType<StageFlowManager>();
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (isActivated) return;

        if (Input.GetKeyDown(interactKey))
        {
            Activate();
        }
    }

    private void Activate()
    {
        isActivated = true;

        if (animator != null)
            animator.SetTrigger("Activate");
    }

    IEnumerator SceneChange()
    {
        sceneFade.DORestart();
        yield return new WaitForSeconds(sceneFade.duration);
        if (stageFlowManager != null)
            stageFlowManager.LoadNextStage();
    }

    // 포탈 Activate 애니메이션 마지막 프레임에서 Animation Event로 호출
    public void AE_LoadNextStage()
    {
        StartCoroutine(SceneChange());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
