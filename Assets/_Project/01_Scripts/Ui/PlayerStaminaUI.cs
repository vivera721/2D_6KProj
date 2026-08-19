using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStaminaUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StaminaFrame staminaFrame;
    [SerializeField] private RectTransform staminaContainer;
    [SerializeField] private GameObject staminaCellPrefab;

    [Header("Sprites")]
    [SerializeField] private Sprite fullSprite;
    [SerializeField] private Sprite emptySprite;

    [Header("Settings")]
    [SerializeField] private int minimumSlotCount = 5;

    private readonly List<Image> cells = new();

    public void RefreshUI(int currentStamina, int maxStamina)
    {

        int displaySlotCount = Mathf.Max(minimumSlotCount, maxStamina);

        if (staminaFrame != null)
            staminaFrame.RefreshFrame(maxStamina);

        RebuildCells(displaySlotCount);
        UpdateCells(currentStamina, displaySlotCount);
    }

    private void RebuildCells(int displaySlotCount)
    {
        if (staminaContainer == null || staminaCellPrefab == null) return;

        while (cells.Count < displaySlotCount)
        {
            GameObject newCellObj = Instantiate(staminaCellPrefab, staminaContainer);
            Image newCellImage = newCellObj.GetComponent<Image>();

            if (newCellImage == null)
                newCellImage = newCellObj.GetComponentInChildren<Image>();

            if (newCellImage != null)
                cells.Add(newCellImage);
        }

        for (int i = 0; i < cells.Count; i++)
        {
            bool shouldBeActive = i < displaySlotCount;
            cells[i].gameObject.SetActive(shouldBeActive);
        }
    }

    private void UpdateCells(int currentStamina, int displaySlotCount)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            if (!cells[i].gameObject.activeSelf) continue;

            if (i < currentStamina)
                cells[i].sprite = fullSprite;
            else
                cells[i].sprite = emptySprite;
        }
    }
}