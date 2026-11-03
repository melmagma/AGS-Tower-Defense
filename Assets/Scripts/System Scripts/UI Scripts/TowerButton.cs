using UnityEngine;

public class TowerButton : MonoBehaviour
{
    public BaseTower towerToPlace;

    public void SelectTower()
    {
        if (LevelManager.instance.levelActive)
        {

            TowerManager.instance.StartTowerPlacement(towerToPlace);
        }
    }
}
