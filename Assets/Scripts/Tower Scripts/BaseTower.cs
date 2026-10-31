using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class BaseTower : MonoBehaviour
{

    public enum HitType { Single, Multi }

    public float damage = 30f;
    public HitType hitType = HitType.Single;
    public float splashRadius = 1.5f;   // used when hitType is Multi
    public bool canHitMetal;
    public bool canHitCamo;
    public bool canTargetDensest;


    public float range = 3f;
    public float fireRate; // this is where we will define hit per second
    public LayerMask whatIsEnemy; //layer filter detection

    private Collider[] colliderInRange; //array holding the fixed numbers of colliders that come into range
    public List<EnemyController> enemiesInRange = new List<EnemyController>();  //enemiesInRange updates the list of enemycontrollers

    private float checkCounter;
    public float checkTime = .2f; // for optimising performance. instead of update checking every frame, this checks every 0.2 seconds.

    [HideInInspector]
    public bool enemiesUpdated; //becomes true on the frame when the list of enemycontrollers gets updated

    public GameObject rangeModel; //visual range radius reference.

    public int cost = 100; //tower cost, can be updated in inspector according to the diff types of towers

    [HideInInspector]
    public TowerUpgradeController upgrader; //reference to the upgrade script

    // Start is called before the first frame update
    void Start()
    {
        checkCounter = checkTime; //set the timer to the full value

        upgrader = GetComponent<TowerUpgradeController>(); //automatically fetches the towerupgrade script. 
    }

    // Update is called once per frame
    void Update()
    {
        enemiesUpdated = false;

        checkCounter -= Time.deltaTime; //doesnt run on framerate, so after check counter is 0, it resets and checks again for an enemy
        if (checkCounter <= 0)
        {
            checkCounter = checkTime;


            colliderInRange = Physics.OverlapSphere(transform.position, range, whatIsEnemy); //update the array on the gameobjects that have the enemy layer in the range radius

            enemiesInRange.Clear(); //reset the list, and then using foreach loop, adds back all the enemy controllers present by checking for the amt of colliders (enemylayer) present
            foreach (Collider col in colliderInRange)
            {
                enemiesInRange.Add(col.GetComponent<EnemyController>());
            }

            enemiesUpdated = true; //to update the bool and signal a new list of enemies within range.
        }

        if (TowerManager.instance.selectedTower == this) //when i click and select the tower, toggle visibility for the tower's range radius
        {
            rangeModel.SetActive(true);
            rangeModel.transform.localScale = new Vector3(range, 1f, range); //the range radius model can be changed and scaled according to the range variable
        }
    }

    private void OnMouseDown() //a method which is automatically called when unity detects that the player clicks on the collider.
    {
        if (LevelManager.instance.levelActive)
        {

            if (TowerManager.instance.selectedTower != null)
            {
                TowerManager.instance.selectedTower.rangeModel.SetActive(false); //to prevent from selecting multiple towers at once
            }

            TowerManager.instance.selectedTower = this;

            UIController.instance.OpenTowerUpgradePanel();

            TowerManager.instance.MoveTowerSelectionEffect(); //vfx to tell the player that this is the highlighted/ selected tower
        }
    }
}
