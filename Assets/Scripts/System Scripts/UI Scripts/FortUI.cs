using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FortUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text healthText; 

    void Start()
    {
        Fort fort = Fort.Instance;
        healthSlider.maxValue = fort.MaxHealth;
        healthSlider.value = fort.CurrentHealth;

        fort.OnHealthChanged += UpdateHealth;
        fort.OnFortDestroyed += ShowGameOver;
    }

    void OnDestroy()
    {
        if (Fort.Instance == null) return;
        Fort.Instance.OnHealthChanged -= UpdateHealth;
        Fort.Instance.OnFortDestroyed -= ShowGameOver;
    }

    private void UpdateHealth(int current, int max)
    {
        healthSlider.value = current;
        if (healthText != null) healthText.text = $"{current} / {max}";
    }

    private void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // freeze the game
    }
}
