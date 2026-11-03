using System.Collections.Generic;
using UnityEngine;

public static class TowerSynergy
{
    public const int RequiredCount = 3; // same-tag towers needed, counting the tower itself

    private static readonly List<BaseTower> towers = new List<BaseTower>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay() { towers.Clear(); } // keeps the list clean if domain reload is disabled

    public static void Register(BaseTower tower)
    {
        if (towers.Contains(tower)) return;
        towers.Add(tower);
        Recalculate();
    }

    public static void Unregister(BaseTower tower)
    {
        if (towers.Remove(tower)) Recalculate();
    }

    // call this whenever the tower list OR a tower's range changes
    public static void Recalculate()
    {
        int entered = 0; // how many towers entered synergy in this pass, decides which clip each one plays

        foreach (BaseTower a in towers)
        {
            if (a == null) continue;

            if (a.synergyTag == BaseTower.SynergyTag.None)
            {
                a.SetSynergy(0, false);
                continue;
            }

            int count = 1; // counts itself
            float rangeSqr = a.range * a.range;

            foreach (BaseTower b in towers)
            {
                if (b == null || b == a || b.synergyTag != a.synergyTag) continue;

                if ((b.transform.position - a.transform.position).sqrMagnitude <= rangeSqr)
                    count++;
            }

            if (a.SetSynergy(count, count >= RequiredCount))
            {
                a.PlaySynergySFX(entered);
                entered++;
            }
        }
    }
}