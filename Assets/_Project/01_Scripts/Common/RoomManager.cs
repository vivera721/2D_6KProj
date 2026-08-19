using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    Normal,
    Boss
}

public class RoomManager : MonoBehaviour
{
    [Header("Room")]
    [SerializeField] private RoomType roomType;
    public RoomType RoomType => roomType;

    [Header("Enemies")]
    [SerializeField] private List<GameObject> enemies = new();

    [Header("Doors")]
    [SerializeField] private List<Door> doors = new();

    [Header("Flow")]
    [SerializeField] private StageFlowManager stageFlowManager;

    private bool started;
    private bool cleared;

    public void StartRoom()
    {
        if (started) return;

        started = true;
        cleared = false;

        CloseDoors();

        foreach (var enemy in enemies)
            enemy.SetActive(true);
    }

    private void ClearRoom()
    {
        if (cleared) return;

        cleared = true;

        OpenDoors();

        if (stageFlowManager != null)
        {
            stageFlowManager.OnRoomCleared(this);
        }
    }

    public void CheckEnemyDead(GameObject enemy)
    {
        if(cleared) return;

        enemies.Remove(enemy);

        if (enemies.Count <= 0)
            ClearRoom();
    }

    private void CloseDoors()
    {
        foreach (var door in doors)
            if(door != null)
                door.Close();
    }

    private void OpenDoors()
    {
        foreach (var door in doors)
            if(door != null)
                door.Open();
    }
}
