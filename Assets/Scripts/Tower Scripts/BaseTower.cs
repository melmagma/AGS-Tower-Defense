using System.Collections.Generic;
using UnityEngine;

public class BaseTower : MonoBehaviour
{

    public enum HitType { Single, Multi }

    public float damage = 30f;
    public HitType hitType = HitType.Single;
    public float splashRadius = 1.5f;   // used when hitType is Multi
    public bool canHitMetal;
    public bool canHitCamo;
    public enum TargetMode { First, Last } //the diff types of targetting trackings

    public TargetMode targetMode = TargetMode.First;


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

    public enum SynergyTag { None, Stationery, Figurines, Electronics }

    [Header("Synergy")]
    public SynergyTag synergyTag = SynergyTag.None;

    [HideInInspector] public int synergyCount; // same-tag towers in range, including itself
    [HideInInspector] public bool synergyActive;

    [Header("Stats")]
    [SerializeField] private int totalDamageDealt;
    public int TotalDamageDealt => totalDamageDealt;
    public event System.Action<int> OnTotalDamageChanged;

    public void RegisterDamage(int amount)
    {
        totalDamageDealt += amount;
        OnTotalDamageChanged?.Invoke(totalDamageDealt);
    }



    // Start is called before the first frame update
    void Start()
    {
        checkCounter = checkTime; //set the timer to the full value

        upgrader = GetComponent<TowerUpgradeController>(); //automatically fetches the towerupgrade script. 

        TowerSynergy.Register(this);

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
                EnemyController e = col.GetComponentInParent<EnemyController>();
                if (e != null) enemiesInRange.Add(e);
            }

            enemiesUpdated = true; //to update the bool and signal a new list of enemies within range.

        }

        if (TowerManager.instance.selectedTower == this) //when i click and select the tower, toggle visibility for the tower's range radius
        {
            rangeModel.SetActive(true);
            rangeModel.transform.localScale = new Vector3(range, 1f, range); //the range radius model can be changed and scaled according to the range variable
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TowerManager sel = TowerManager.instance;
            bool isSelected = sel.selectedTower == this;
            Debug.Log($"{name}: Tab pressed. isSelected={isSelected}, selectedTower={(sel.selectedTower != null ? sel.selectedTower.name : "none")}", this);

            if (isSelected) CycleTargetMode();
        }
    }

    public void CycleTargetMode() // toggleable targetmode method thats called when player presses on teh button from UIManager
    {
        targetMode = targetMode == TargetMode.First ? TargetMode.Last : TargetMode.First;
    }

    public string TargetModeLabel => targetMode == TargetMode.First ? "Target: First" : "Target: Last"; 

    private void OnMouseDown() //a method which is automatically called when unity detects that the player clicks on the collider.
    {
        if (LevelManager.instance.levelActive)
        {

            if (TowerManager.instance.selectedTower != null)
            {
                TowerManager.instance.selectedTower.rangeModel.SetActive(false); //to prevent from selecting multiple towers at once
            }

            TowerManager.instance.selectedTower = this;

            UIManager.instance.OpenTowerUpgradePanel();

            TowerManager.instance.MoveTowerSelectionEffect(); //vfx to tell the player that this is the highlighted/ selected tower
        }
    }

    public EnemyController GetTarget()
    {
        EnemyController best = null;

        for (int i = 0; i < enemiesInRange.Count; i++)
        {
            EnemyController e = enemiesInRange[i];
            if (e == null) continue; // enemy may have been destroyed since the last check

            if (best == null)
            {
                best = e;
                continue;
            }

            switch (targetMode)
            {
                case TargetMode.First:
                    if (e.DistanceTravelled > best.DistanceTravelled) best = e;
                    break;
                case TargetMode.Last:
                    if (e.DistanceTravelled < best.DistanceTravelled) best = e;
                    break;
            }
        }

        return best;
    }

    void OnDrawGizmos()
    {
        bool hasEnemies = enemiesInRange != null && enemiesInRange.Count > 0;

        Gizmos.color = hasEnemies ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, range);

        if (hasEnemies)
        {
            Gizmos.color = Color.yellow;
            foreach (EnemyController e in enemiesInRange)
                if (e != null) Gizmos.DrawLine(transform.position, e.transform.position);
        }
    }

    public void SetSynergy(int count, bool active)
    {
        synergyCount = count;
        if (active == synergyActive) return;

        synergyActive = active;

        // TODO: apply / remove the real bonus here
        Debug.Log($"{name} synergy {(active ? "ACTIVE" : "inactive")} ({synergyTag}, {count}/{TowerSynergy.RequiredCount})", this);
    }

    void OnDestroy()
    {
        TowerSynergy.Unregister(this);
    }
}
