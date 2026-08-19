using UnityEngine;
using UnityEngine.UI;

public class StaminaFrame : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LayoutElement middleCapLayout;
    [SerializeField] private RectTransform capRoot;

    [Header("Slot Layout")]
    [SerializeField] private float cellWidth = 15.625f;
    [SerializeField] private float spacing = 0f;

    [Header("Frame Padding")]
    [SerializeField] private float leftPadding = 6f;
    [SerializeField] private float rightPadding = 6f;

    [Header("Minimum Slots")]
    [SerializeField] private int minimumSlotCount = 5;

    public void RefreshFrame(int maxStamina)
    {
        if (middleCapLayout == null)
        {
            Debug.LogWarning("middleCapLayout is null");
            return;
        }

        int displaySlotCount = Mathf.Max(minimumSlotCount, maxStamina);

        float totalCellsWidth = (cellWidth * displaySlotCount) + (spacing * (displaySlotCount - 1));
        float finalWidth = leftPadding + totalCellsWidth + rightPadding;

        middleCapLayout.preferredWidth = finalWidth;

        if (capRoot != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(capRoot);
    }
}
