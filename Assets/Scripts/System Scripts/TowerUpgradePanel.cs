using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class TowerUpgradePanel : MonoBehaviour //notes and changes to be updated
{
    public GameObject rangeButton, firerateButton;
    public TMP_Text rangeText, firerateText;

    public void SetupPanel()
    {
        if (TowerManager.instance.selectedTower.upgrader.hasRangeUpgrade)
        {
            TowerUpgradeController upgrader = TowerManager.instance.selectedTower.upgrader;
            rangeText.text = "Upgrade\nRange\n(" + upgrader.rangeUpgrades[upgrader.currentRangeUpgrade].cost + "G)";

            rangeButton.SetActive(true);
        }
        else
        {
            rangeButton.SetActive(false);
        }

        if (TowerManager.instance.selectedTower.upgrader.hasFirerateUpgrade)
        {
            TowerUpgradeController upgrader = TowerManager.instance.selectedTower.upgrader;
            firerateText.text = upgrader.fireRateText + "(" + upgrader.firerateUpgrades[upgrader.currentFirerateUpgrade].cost + "G)";

            firerateButton.SetActive(true);
        }
        else
        {
            firerateButton.SetActive(false);
        }
    }

    public void RemoveTower()
    {
        MoneyManager.instance.SpendMoney(-50);

        Destroy(TowerManager.instance.selectedTower.gameObject);

        UIManager.instance.CloseTowerUpgradePanel();

        AudioManager.instance.PlaySFX(9);
    }

    public void UpgradeRange()
    {
        TowerUpgradeController upgrader = TowerManager.instance.selectedTower.upgrader;

        if (upgrader.hasRangeUpgrade)
        {
            if (MoneyManager.instance.SpendMoney(upgrader.rangeUpgrades[upgrader.currentRangeUpgrade].cost))
            {
                upgrader.UpgradeRange();

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

    public void UpgradeFireRate()
    {
        TowerUpgradeController upgrader = TowerManager.instance.selectedTower.upgrader;

        if (upgrader.hasFirerateUpgrade)
        {
            if (MoneyManager.instance.SpendMoney(upgrader.firerateUpgrades[upgrader.currentFirerateUpgrade].cost))
            {
                upgrader.UpgradeFireRate();

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
}
