using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class TowerUpgradePanel : MonoBehaviour
{
    [FormerlySerializedAs("rangeButton")] public GameObject path1Button; //formelyserialisedas is a way to rename the field without changing its values, which is helpful when making the upgrade system
    [FormerlySerializedAs("firerateButton")] public GameObject path2Button;
    [FormerlySerializedAs("rangeText")] public TMP_Text path1Text;
    [FormerlySerializedAs("firerateText")] public TMP_Text path2Text;
    [SerializeField] int CostOfSell;

    public void SetupPanel()
    {
        TowerUpgradeController upgrader = TowerManager.instance.selectedTower.upgrader;

        if (upgrader.CanUpgrade)
        {
            path1Text.text = BuildButtonText(upgrader.paths[0]); // both paths are on offer until the player buys one.
            path2Text.text = BuildButtonText(upgrader.paths[1]);

            path1Button.SetActive(true);
            path2Button.SetActive(true);
        }
        else
        {
            path1Button.SetActive(false); // to prevent from re-upgrading the same dude 2wice
            path2Button.SetActive(false);
        }
    }

    // Shown on the button, e.g. "Flyer\n(235G)". path.description is also available if you want to show it.
    private string BuildButtonText(UpgradePath path)
    {
        return path.pathName + "\n(" + path.cost + "G)";
    }

    public void RemoveTower()
    {
        MoneyManager.instance.SpendMoney(-CostOfSell);

        Destroy(TowerManager.instance.selectedTower.gameObject);

        UIManager.instance.CloseTowerUpgradePanel();

        AudioManager.instance.PlaySFX(9);
    }
    public void ChoosePath(int pathIndex)
    {
        TowerUpgradeController upgrader = TowerManager.instance.selectedTower.upgrader;

        if (!upgrader.CanUpgrade)
        {
            return;
        }

        if (pathIndex < 0 || pathIndex >= upgrader.paths.Length)
        {
            return;
        }

        if (MoneyManager.instance.SpendMoney(upgrader.paths[pathIndex].cost))
        {
            upgrader.ChoosePath(pathIndex);

            SetupPanel();

            UIManager.instance.notEnoughMoneyWarning.SetActive(false);

            AudioManager.instance.PlaySFX(10);
        }
        else
        {
            UIManager.instance.notEnoughMoneyWarning.SetActive(true);
        }
    }
}