using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 1.0f;
    public float turnSpeed = 360f; // degrees per second

    [Header("Health")]
    [SerializeField] private int maxHealth = 50;
    private int currentHealth;

    [Header("Path")]
    [SerializeField] private List<GameObject> checkpoints = new List<GameObject>();

    // Total ground covered so far (useful for tower targeting like "first" / "last")
    public float DistanceTravelled { get; private set; }
    public int CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;

        if (checkpoints == null || checkpoints.Count == 0)
        {
            Debug.LogWarning($"{name} has no checkpoints assigned.", this);
            return;
        }

        transform.position = checkpoints[0].transform.position;
        StartCoroutine(Pathing());
    }

    public void SetPath(List<GameObject> path)
    {
        checkpoints = new List<GameObject>(path);
    }

    private IEnumerator Pathing()
    {
        for (int i = 0; i < checkpoints.Count; i++)
        {
            if (checkpoints[i] == null) continue;

            Transform currentTarget = checkpoints[i].transform;

            while (Vector3.Distance(transform.position, currentTarget.position) > 0.01f)
            {
                Vector3 previousPosition = transform.position;

                // Move toward the target
                float step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, step);

                // Track distance actually covered this frame
                DistanceTravelled += Vector3.Distance(previousPosition, transform.position);

                // Rotate to face the direction of travel
                Vector3 direction = currentTarget.position - transform.position;
                if (direction.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                }

                yield return null;
            }
        }

        // Reached the end of the path
        OnReachedEnd();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnReachedEnd()
    {
        // TODO: damage the player's base, then remove the enemy
        if (Fort.Instance != null)
            Fort.Instance.TakeDamage(currentHealth);

        Destroy(gameObject);
        
    }

}
