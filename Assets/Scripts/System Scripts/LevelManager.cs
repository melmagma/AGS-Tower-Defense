using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    private void Awake()
    {
        instance = this;
    }

    public bool levelActive;
    private bool levelVictory;

    public List<EnemyController> aliveEnemies = new List<EnemyController>();

    private EnemySpawner[] spawners;

    public string nextLevel;
    // Start is called before the first frame update
    void Start()
    {
        spawners = FindObjectsByType<EnemySpawner>();

        levelActive = true;

        AudioManager.instance.PlayBGM();
    }

    // Update is called once per frame
    void Update()
    {
        if (levelActive)
        {
            //if (PillowFort.currentHealth <= 0)
            //{
            //    levelActive = false;
            //    levelVictory = false;
            //
            //    UIManager.instance.towerButtons.SetActive(false);
            //}

            if (AllWavesSpawned() && NoEnemiesLeft())
            {
                EndLevel(true);
            }
        }
    }

    private bool AllWavesSpawned()
    {
        foreach (EnemySpawner spawner in spawners)
        {
            if (spawner != null && !spawner.AllWavesSpawned)
            {
                return false;
            }
        }
        return true;
    }

    private bool NoEnemiesLeft()
    {
        aliveEnemies.RemoveAll(e => e == null);
        if (aliveEnemies.Count > 0) return false;

        return FindObjectsByType<EnemyController>().Length == 0;
    }

    private void EndLevel(bool victory)
    {
        levelActive = false;
        levelVictory = victory;

        UIManager.instance.towerButtons.SetActive(false);
        UIManager.instance.levelFailScreen.SetActive(!victory);
        UIManager.instance.levelCompleteScreen.SetActive(victory);
        UIManager.instance.CloseTowerUpgradePanel();
    }
}

