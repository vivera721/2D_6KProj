using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class BossIntroUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image darkPanel;
    [SerializeField] private CanvasGroup nameUIGroup;

    [Header("Fade Settings")]
    [SerializeField] private float panelTargetAlpha = 0.6f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Boss Name")]
    [SerializeField] private TextMeshProUGUI bossNameText;
    private string bossName = "Boss Name"; // 실제 보스 이름으로 설정

    private Sequence seq;


    private void Awake()
    {
        ResetIntroUI();
        gameObject.SetActive(false);
        bossNameText = GetComponentInChildren<TextMeshProUGUI>();
        bossName = FindAnyObjectByType<BossBase>()?.name ?? "Boss Name";
    }

    private void Start()
    {
        bossNameText.text = bossName;
    }

    public void PlayIntro()
    {
        gameObject.SetActive(true);

        seq?.Kill();
        ResetIntroUI();

        seq = DOTween.Sequence();

        // Panel 0 -> 0.6
        seq.Join(darkPanel.DOFade(panelTargetAlpha, fadeDuration));

        // Name UI 0 -> 1, Panel fade와 동시에 실행
        seq.Join(nameUIGroup.DOFade(1f, fadeDuration));

        // 이름 잠깐 유지
        seq.AppendInterval(holdDuration);

        // 사라지게 하고 싶으면 아래 사용
        seq.Append(nameUIGroup.DOFade(0f, fadeOutDuration));
        seq.Join(darkPanel.DOFade(0f, fadeOutDuration));

        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    private void ResetIntroUI()
    {
        if (darkPanel != null)
        {
            Color c = darkPanel.color;
            c.a = 0f;
            darkPanel.color = c;
        }

        if (nameUIGroup != null)
        {
            nameUIGroup.alpha = 0f;
        }
    }

}
