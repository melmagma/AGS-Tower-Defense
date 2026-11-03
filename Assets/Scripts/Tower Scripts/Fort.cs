using System;
using UnityEngine;

public class Fort : MonoBehaviour
{
    // Lets enemies find the fort without needing a reference in the prefab
    public static Fort Instance { get; private set; }

    [SerializeField] private int maxHealth = 500;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsDestroyed => CurrentHealth <= 0;

    // (currentHealth, maxHealth) - hook UI up to this
    public event Action<int, int> OnHealthChanged;
    public event Action OnFortDestroyed;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("More than one Fort in the scene. Destroying the extra one.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentHealth = maxHealth;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void TakeDamage(int amount)
    {
        if (IsDestroyed || amount <= 0) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (IsDestroyed)
        {
            Debug.Log("Fort destroyed! Game over.");
            OnFortDestroyed?.Invoke();
        }
    }
}
