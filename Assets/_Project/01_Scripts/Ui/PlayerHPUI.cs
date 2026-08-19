using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HPFrame hpFrame;
    [SerializeField] private RectTransform hpContainer;
    [SerializeField] private GameObject hpCellPrefab;

    [Header("Sprites")]
    [SerializeField] private Sprite fullSprite;
    [SerializeField] private Sprite emptySprite;

    [Header("Settings")]
    [SerializeField] private int minimumSlotCount = 5;

    private readonly List<Image> cells = new();

    public void RefreshUI(int currentHP, int maxHP)
    {

        int displaySlotCount = Mathf.Max(minimumSlotCount, maxHP);

        if (hpFrame != null)
            hpFrame.RefreshFrame(maxHP);

        RebuildCells(displaySlotCount);
        UpdateCells(currentHP, displaySlotCount);
    }

    private void RebuildCells(int displaySlotCount)
    {
        if (hpContainer == null || hpCellPrefab == null) return;

        while (cells.Count < displaySlotCount)
        {
            GameObject newCellObj = Instantiate(hpCellPrefab, hpContainer);
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

    private void UpdateCells(int currentHP, int displaySlotCount)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            if (!cells[i].gameObject.activeSelf) continue;

            if (i < currentHP)
                cells[i].sprite = fullSprite;
            else
                cells[i].sprite = emptySprite;
        }
    }
}