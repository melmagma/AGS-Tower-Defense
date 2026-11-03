using TMPro;
using UnityEngine;

public class DamageDealtDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text valueText;
    private BaseTower currentTower;

    public void SetTower(BaseTower tower)
    {
        if (currentTower != null)
            currentTower.OnTotalDamageChanged -= UpdateText;

        currentTower = tower;

        if (currentTower != null)
        {
            currentTower.OnTotalDamageChanged += UpdateText;
            UpdateText(currentTower.TotalDamageDealt);
        }
        else
        {
            valueText.text = "-";
        }
    }

    private void UpdateText(int total) => valueText.text = total.ToString();

    void OnDisable()
    {
        if (currentTower != null)
            currentTower.OnTotalDamageChanged -= UpdateText;
    }
}