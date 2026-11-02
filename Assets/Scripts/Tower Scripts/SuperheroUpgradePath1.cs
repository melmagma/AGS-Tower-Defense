using System.Collections.Generic;
using UnityEngine;

// Add this to the tower prefab, leave it unticked, then drag it into Path 1's "Ability Script" slot.
public class SuperheroUpgradePath1 : TowerPathAbility
{
    [Header("Reposition ability")]
    public float repositionCooldown = 15f;
    public int landingDamage = 250;

    [Header("Placement")]
    public Transform indicatorPrefab; // the ghost/hologram that follows the cursor while picking a landing spot

    private float cooldownTimer;
    private bool isPlacing;
    private Transform indicator;


    private readonly HashSet<EnemyController> enemiesHit = new HashSet<EnemyController>();// reused on every landing so we dont allocate a new set each time the ability is used

    public bool IsReady
    {
        get { return cooldownTimer <= 0f; } //cooldown on the landing ability
    }

    public bool IsPlacing
    {
        get { return isPlacing; }
    }



    protected override void OnPathChosen()
    {
        cooldownTimer = 0f; // ready to use straight away once it has been upgraded
        // Other one-off changes for this path go here (e.g. unlock hitting CAMO enemies).
    }

    // Only runs after this path is chosen, because the script stays disabled until then.
    void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (isPlacing)
        {
            HandlePlacement();
        }
    }

    // Hook this up to your ability button. It starts the "pick where to land" mode.
    public void StartRepositionPlacement()
    {
        if (!enabled || !IsReady || isPlacing || indicatorPrefab == null) return;
        if (TowerManager.instance.isPlacing) return;

        indicator = Instantiate(indicatorPrefab);
        isPlacing = true; // only after the instantiate succeeded
    }

    // Same flow as TowerManager.Update(): follow the cursor, hide when invalid, left click confirms, right click cancels.
    private void HandlePlacement()
    {
        TowerManager manager = TowerManager.instance;

        if (Input.GetMouseButtonDown(1)) //right click = escape out of the ability
        {
            StopPlacement();
            return;
        }

        indicator.position = manager.GetGridPosition(); //reuses TowerManager's cursor raycast + y = 0 snapping

        if (Input.mousePosition.y > Screen.height * (1f - (manager.topSafePercent / 100f))) //cursor is over the UI, so no placement
        {
            indicator.gameObject.SetActive(false);
        }
        else if (Physics.Raycast(indicator.position + new Vector3(0f, -2f, 0f), Vector3.up, 10f, manager.whatIsObstacle)) //obstacle (e.g. the path) in the way
        {
            indicator.gameObject.SetActive(false);
        }
        else
        {
            indicator.gameObject.SetActive(true);

            if (Input.GetMouseButtonDown(0)) //left click = confirm the landing spot
            {
                Vector3 landingSpot = indicator.position;

                StopPlacement();
                RepositionTo(landingSpot);
            }
        }
    }

    private void StopPlacement()
    {
        isPlacing = false;

        if (indicator != null)
        {
            Destroy(indicator.gameObject);
        }
    }

    public void RepositionTo(Vector3 landingSpot) //when the player has a position that is good to place, then this is called
    {
        if (!IsReady || tower == null) //tower is only set by Activate(), so this also stops it running before the path is chosen
        {
            return;
        }

        cooldownTimer = repositionCooldown;
        transform.position = landingSpot; // swap for a fly animation / coroutine later

        DealLandingDamage(landingSpot);
    }

    private void DealLandingDamage(Vector3 center) // AOE on landing, using the tower's range as the blast radius.
    {
        Collider[] hits = Physics.OverlapSphere(center, tower.range, tower.whatIsEnemy); //layermask detection for enemies only

        enemiesHit.Clear();
        foreach (Collider hit in hits) //for each loop to detech a collider of one enemy, = one enemy instead of if 2 colliders in 1 enemy = 2 enemies detected
        {
            EnemyController enemy = hit.GetComponentInParent<EnemyController>(); //same lookup BaseTower uses when scanning
            if (enemy != null)
            {
                enemiesHit.Add(enemy);
            }
        }

        foreach (EnemyController enemy in enemiesHit)
        {
            enemy.TakeDamage(landingDamage);
        }
    }


    void OnDisable()
    {
        StopPlacement();// Cleans up the ghost if the script/tower gets disabled or destroyed mid-placement.
    }
}