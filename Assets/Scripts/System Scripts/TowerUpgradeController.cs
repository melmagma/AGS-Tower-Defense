using UnityEngine;

public class TowerUpgradeController : MonoBehaviour
{
    private BaseTower theTower;

    [Header("The 2 upgrade paths (element 0 = Path 1, element 1 = Path 2)")]
    public UpgradePath[] paths = new UpgradePath[] { new UpgradePath(), new UpgradePath() }; //start the array with 2 placeholdr empty slots, but when unity takes over and views the script the values in the inspector will take over.

    [Header("Mesh swap targets (found in the children automatically if left empty)")] // this is what we'll reference the models of the base unit. this allows us to change it ltr with Applylook() to continue tracking total kills and continue synergy with units without abrupt stops if we did a "destroy gameobject" "instantiate new prefab" 
    public MeshFilter towerMeshFilter;
    public MeshRenderer towerMeshRenderer;

    public int ChosenPathIndex { get; private set; } = -1; //-1 = no upgrades has been applied, if not, then itll be the index of the upgrade path chosen by the player
    public bool CanUpgrade
    {
        get { return ChosenPathIndex < 0; } //reference this with UI to able/disable the buttons.
    }

    void Awake()
    {
        theTower = GetComponent<BaseTower>();
        if (theTower == null)
        {
            Debug.LogError("TowerUpgradeController needs a BaseTower on the same GameObject.", this);
        }

        if (towerMeshFilter == null) towerMeshFilter = GetComponentInChildren<MeshFilter>();
        if (towerMeshRenderer == null) towerMeshRenderer = GetComponentInChildren<MeshRenderer>(); // a safety net basically. if there was no mesh components in inspector, these lines of code will search for it on the current gameobject n its children

        foreach (UpgradePath path in paths)
        {
            if (path.abilityScript != null)
            {
                path.abilityScript.enabled = false;// disables all upgrade path scripts incase they have not been disabled yet (as it should only be abled when we upgrade the unit)
            }
        }
    }

    
    public void ChoosePath(int pathIndex) //hook your UI buttons to this: Path 1 button -> ChoosePath(0), Path 2 button -> ChoosePath(1). ( FOR EDLYSIA )
    {
        if (!CanUpgrade || theTower == null)
        {
            return; //return and exit if we already upgraded the tower
        }

        if (pathIndex < 0 || pathIndex >= paths.Length)
        {
            Debug.LogWarning("TowerUpgradeController: there is no path " + pathIndex, this);
            return;
        }

        UpgradePath path = paths[pathIndex]; // based on what the player selected, the index value reading which upgrade pathway is updated here

        ChosenPathIndex = pathIndex; // from here on CanUpgrade is false. only one upgrade path per tower.

        ApplyStats(path);
        ApplyLook(path); //change the stats and models of the unit.

        // Switch on this path's own script (the other path's script stays disabled).
        if (path.abilityScript != null)
        {
            path.abilityScript.Activate(theTower); //checks for towers that have an ability script. towers like superhero, and paystation etc.
        }
    }

    private void ApplyStats(UpgradePath path)
    {
        if (path.range > 0f) theTower.range = path.range; //these are the final values, not added ones, that replaces the old stats.
        TowerSynergy.Recalculate(); //when range changes, recalculate whether the synergy of units affected has changed.
        if (path.fireRate > 0f) theTower.fireRate = path.fireRate; //if a value is at 0, it gets skipped. this is useful for towers where there arent changes in the stats.
        if (path.damage > 0f) theTower.damage = path.damage;

        if (path.makeMultiHit) theTower.hitType = BaseTower.HitType.Multi;
        if (path.splashRadius > 0f) theTower.splashRadius = path.splashRadius;

        if (path.unlockCamo) theTower.canHitCamo = true;
        if (path.unlockMetal) theTower.canHitMetal = true;
    }

    private void ApplyLook(UpgradePath path)
    {
        if (path.newMesh != null && towerMeshFilter != null) //safety net but basically this method finds the new model to use when upgrading a unit tower. if there arent any, the same base model is resused.
        {
            towerMeshFilter.sharedMesh = path.newMesh; 
        }

        if (path.newMaterial != null && towerMeshRenderer != null)
        {
            towerMeshRenderer.sharedMaterial = path.newMaterial;
        }
    }

}

[System.Serializable] // a data container that shows up on inspector but isnt its own monobehaviour.
public class UpgradePath // edlysia these are mostly for your ref
{
    public string pathName;                // shown in your upgrade UI
    [TextArea] public string description;  // shown in your upgrade UI (replaces fireRateText)
    public int cost;

    [Header ("Hit Type")]
    public bool makeMultiHit;
    public float splashRadius; // final value, 0 = keep current (only used when multi)

    [Header("Stats (final values, not bonuses; 0 = keep current)")]
    public float range;
    public float fireRate;  // hits per second (4 = 4 hits per second)
    public float damage;

    [Header("Targeting")]
    public bool unlockCamo;   // this path can damage CAMO enemies
    public bool unlockMetal;  // this path can damage METAL enemies

    [Header("Look")]
    public Mesh newMesh;          // leave empty to keep the current mesh
    public Material newMaterial;  // leave empty to keep the current material

    [Header("Unique abilities")]
    public TowerPathAbility abilityScript;  // this path's own script, disabled until the path is chosen
}
