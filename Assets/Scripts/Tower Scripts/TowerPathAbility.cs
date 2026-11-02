using UnityEngine;

// Base class for every upgrade-path script (one script per path, per unit).
//
// How to use it:
//   1. Make a script that inherits from this one (see SuperheroPath1.cs).
//   2. Add it to the tower prefab and leave it unticked (disabled).
//   3. Drag it into that path's "Ability Script" slot on TowerUpgradeController.
//
// TowerUpgradeController enables the script and calls Activate() the moment the path
// is chosen, so Update() and friends only start running after the player has picked it.
public abstract class TowerPathAbility : MonoBehaviour
{
    // The tower this path belongs to (range, damage, fireRate...). Set in Activate().
    protected BaseTower tower;

    // Called by TowerUpgradeController when this path is chosen. Don't call it yourself.
    public void Activate(BaseTower owner)
    {
        tower = owner;
        enabled = true;  // switches on Update(), OnEnable() and so on
        OnPathChosen();
    }

    // Override this for one-time setup when the path is chosen
    // (reset cooldowns, spawn an effect, etc)
    protected virtual void OnPathChosen() 
    { 
    }
}