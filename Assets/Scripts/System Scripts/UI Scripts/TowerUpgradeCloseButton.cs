using UnityEngine;

public class TowerUpgradeCloseButton : MonoBehaviour
{
    [SerializeField] private GameObject UpgradePanel;

    public void ClosePanel()
    {
        UpgradePanel.SetActive(false);
    }
}
