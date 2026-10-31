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
        public float delayAfterWave = 5f;  // countdown before the next wave auto-starts
    }

    [Header("Path (drag checkpoints in order)")]
    [SerializeField] private List<GameObject> checkpoints = new List<GameObject>();

    [Header("Waves")]
    [SerializeField] private List<Wave> waves = new List<Wave>();
    [SerializeField] private float startDelay = 2f;
    [Tooltip("If true, the automatic countdown waits until every enemy is dead or has reached the end before starting the next wave. The button ignores this.")]
    [SerializeField] private bool waitForWaveCleared = false;

    private readonly List<GameObject> aliveEnemies = new List<GameObject>();

    private int nextWaveIndex;        // index of the next wave to spawn
    private int wavesCurrentlySpawning;
    private bool countdownRunning;
    private float countdown;

    public int CurrentWaveNumber { get; private set; } // 1-based, 0 = not started
    public bool CanSpawnNextWave => nextWaveIndex < waves.Count;
    public bool AllWavesSpawned => nextWaveIndex >= waves.Count && wavesCurrentlySpawning == 0;
    public float TimeUntilNextWave => countdownRunning ? Mathf.Max(0f, countdown) : 0f;

    void Start()
    {
        if (checkpoints.Count == 0)
        {
            Debug.LogWarning($"{name} has no checkpoints assigned.", this);
            enabled = false;
            return;
        }

        // Sit at the first checkpoint
        transform.position = checkpoints[0].transform.position;

        // First wave starts automatically after startDelay
        countdown = startDelay;
        countdownRunning = true;
    }

    void Update()
    {
        if (!countdownRunning) return;

        countdown -= Time.deltaTime;
        if (countdown > 0f) return;

        if (waitForWaveCleared)
        {
            aliveEnemies.RemoveAll(e => e == null); // destroyed objects compare equal to null
            if (aliveEnemies.Count > 0) return;     // keep waiting
        }

        SpawnNextWave();
    }

    /// Starts the next wave immediately. Hook this up to a UI Button.
    /// Also used internally when the countdown finishes.
    public void SpawnNextWave()
    {
        if (!CanSpawnNextWave) return;

        countdownRunning = false; // cancel any running countdown

        int index = nextWaveIndex;
        nextWaveIndex++;
        CurrentWaveNumber = nextWaveIndex;

        StartCoroutine(SpawnWave(index));
    }

    private IEnumerator SpawnWave(int index)
    {
        wavesCurrentlySpawning++;
        Wave wave = waves[index];

        foreach (SpawnGroup group in wave.groups)
        {
            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.enemyPrefab);
                yield return new WaitForSeconds(group.spawnInterval);
            }
            yield return new WaitForSeconds(group.delayAfterGroup);
        }

        wavesCurrentlySpawning--;

        // Only the most recently started wave schedules the next one.
        // (If the player skipped ahead, older waves finishing shouldn't restart the countdown.)
        bool isLatestWave = index == nextWaveIndex - 1;
        if (isLatestWave && CanSpawnNextWave)
        {
            countdown = wave.delayAfterWave;
            countdownRunning = true;
        }
    }

    private void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null) return;

        GameObject enemy = Instantiate(prefab, transform.position, Quaternion.identity);
        enemy.GetComponent<EnemyController>().SetPath(checkpoints);
        aliveEnemies.Add(enemy);
    }
}
