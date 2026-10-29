using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnGroup
    {
        public GameObject enemyPrefab;
        public int count = 5;
        public float spawnInterval = 1f;   // time between each enemy in this group
        public float delayAfterGroup = 1f; // pause before the next group starts
    }

    [System.Serializable]
    public class Wave
    {
        public string name = "Wave";
        public List<SpawnGroup> groups = new List<SpawnGroup>();
        public float delayAfterWave = 5f;  // pause before the next wave
    }

    [Header("Path (drag checkpoints in order)")]
    [SerializeField] private List<GameObject> checkpoints = new List<GameObject>();

    [Header("Waves")]
    [SerializeField] private List<Wave> waves = new List<Wave>();
    [SerializeField] private float startDelay = 2f;
    [Tooltip("If true, the next wave only begins once every enemy is dead or has reached the end.")]
    [SerializeField] private bool waitForWaveCleared = false;

    private readonly List<GameObject> aliveEnemies = new List<GameObject>();

    public int CurrentWaveNumber { get; private set; } // 1-based, 0 = not started
    public bool AllWavesSpawned { get; private set; }

    void Start()
    {
        if (checkpoints.Count == 0)
        {
            Debug.LogWarning($"{name} has no checkpoints assigned.", this);
            return;
        }

        // Sit at the first checkpoint
        transform.position = checkpoints[0].transform.position;
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(startDelay);

        for (int w = 0; w < waves.Count; w++)
        {
            CurrentWaveNumber = w + 1;
            Wave wave = waves[w];

            foreach (SpawnGroup group in wave.groups)
            {
                for (int i = 0; i < group.count; i++)
                {
                    SpawnEnemy(group.enemyPrefab);
                    yield return new WaitForSeconds(group.spawnInterval);
                }
                yield return new WaitForSeconds(group.delayAfterGroup);
            }

            if (waitForWaveCleared)
            {
                // Destroyed objects compare equal to null in Unity
                while (aliveEnemies.RemoveAll(e => e == null) < 0 || aliveEnemies.Count > 0)
                {
                    aliveEnemies.RemoveAll(e => e == null);
                    yield return null;
                }
            }

            if (w < waves.Count - 1)
                yield return new WaitForSeconds(wave.delayAfterWave);
        }

        AllWavesSpawned = true;
    }

    private void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null) return;

        GameObject enemy = Instantiate(prefab, transform.position, Quaternion.identity);
        enemy.GetComponent<EnemyController>().SetPath(checkpoints);
        aliveEnemies.Add(enemy);
    }
}
