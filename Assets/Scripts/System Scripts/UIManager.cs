using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    private void Awake()
    {
        instance = this;
    }

    public TMP_Text goldText;
    public GameObject notEnoughMoneyWarning; //change this and goldText in tandem with what shin wants for UI

    public GameObject levelCompleteScreen, levelFailScreen;

    public GameObject towerButtons;

    public string levelSelectScene, mainMenuScene;

    public GameObject pauseScreen; //add UI canvas for all these gameobjects (start of with 0 visibility/disabled)

    public TowerUpgradePanel towerUpgradePanel;

    public TMP_Text waveText;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseUnpause();
        }
    }

    public void PauseUnpause()
    {
        if (pauseScreen.activeSelf == false)
        {
            pauseScreen.SetActive(true);//toggles the visibility of the gameobjects, this being the pause screen

            Time.timeScale = 0f; //stops the whole game from moving
        }
        else
        {
            pauseScreen.SetActive(false);

            Time.timeScale = 1f;
        }
    }

    public void StoreSelect() // this will most likely not be a seperate scene but a UI canvas
    {
        Time.timeScale = 1f; //if player presses pause , and then main menu or a level select, then time would remain frozen if not for this line of code.

        SceneManager.LoadScene(levelSelectScene);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuScene);
    }

    public void TryAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel()
    {
       //SceneManager.LoadScene(LevelManager.instance.nextLevel);
    }

    public void OpenTowerUpgradePanel()
    {
        //if (LevelManager.instance.levelActive)         to be changed (self note)
        //{
        //    towerUpgradePanel.gameObject.SetActive(true);
        //    towerUpgradePanel.SetupPanel();
        //}
    }

    public void CloseTowerUpgradePanel()
    {
        towerUpgradePanel.gameObject.SetActive(false);

        if (TowerManager.instance.selectedTower != null)
        {
            TowerManager.instance.selectedTower.rangeModel.SetActive(false);
            TowerManager.instance.selectedTower = null;
        }

        TowerManager.instance.selectedTowerEffect.SetActive(false);

        notEnoughMoneyWarning.SetActive(false);
    }
}
