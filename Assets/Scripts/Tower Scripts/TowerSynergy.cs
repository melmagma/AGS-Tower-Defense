using System.Collections.Generic;
using UnityEngine;

public static class TowerSynergy
{
    public const int RequiredCount = 3;
    public static readonly int[] SfxIndices = { 0, 1, 2 };

    private static readonly List<BaseTower> towers = new List<BaseTower>();
    private static readonly List<BaseTower[]> groups = new List<BaseTower[]>();

    // NEW: highest count already "announced" per tower
    private static readonly Dictionary<BaseTower, int> lastProgress = new Dictionary<BaseTower, int>();

    // REPLACES your old ResetOnPlay (it now also clears lastProgress)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay() { towers.Clear(); groups.Clear(); lastProgress.Clear(); }


    public static void Register(BaseTower tower)
    {
        if (towers.Contains(tower)) return;
        towers.Add(tower);
        Recalculate();
    }

    public static void Unregister(BaseTower tower)
    {
        if (!towers.Remove(tower)) return;
        lastProgress.Remove(tower);

        // selling / removing a member breaks its trio and frees the other two
        for (int g = groups.Count - 1; g >= 0; g--)
        {
            if (System.Array.IndexOf(groups[g], tower) < 0) continue;

            foreach (BaseTower member in groups[g])
                if (member != null && member != tower) member.SetSynergy(0, false);

            groups.RemoveAt(g);
        }

        Recalculate(); // the freed towers may pair up with other free towers right away
    }

    // call when the tower list or a tower's range changes. only forms NEW groups, existing trios stay locked
    public static void Recalculate()
    {
        int sfxProgress = 0; // highest new step reached this pass, 0 = stay silent

        foreach (BaseTower seed in towers)
        {
            if (seed == null || seed.synergyActive) continue;

            if (seed.synergyTag == BaseTower.SynergyTag.None)
            {
                seed.SetSynergy(0, false);
                continue;
            }

            List<BaseTower> nearby = new List<BaseTower>();
            Vector3 pos = seed.transform.position;
            float rangeSqr = seed.range * seed.range;

            foreach (BaseTower other in towers)
            {
                if (other == null || other == seed || other.synergyActive) continue;
                if (other.synergyTag != seed.synergyTag) continue;

                if ((other.transform.position - pos).sqrMagnitude <= rangeSqr)
                    nearby.Add(other);
            }

            int count = nearby.Count + 1;
            seed.SetSynergy(count, false);

            // progress went up since we last announced it -> this pass deserves a note
            lastProgress.TryGetValue(seed, out int last);
            if (count > last) sfxProgress = Mathf.Max(sfxProgress, count);
            lastProgress[seed] = count; // also stores decreases, so re-gaining progress plays again

            if (count < RequiredCount) continue;

            nearby.Sort((a, b) =>
                (a.transform.position - pos).sqrMagnitude.CompareTo((b.transform.position - pos).sqrMagnitude));

            BaseTower[] group = new BaseTower[RequiredCount];
            group[0] = seed;
            for (int i = 1; i < RequiredCount; i++) group[i] = nearby[i - 1];

            groups.Add(group);

            foreach (BaseTower member in group)
            {
                member.SetSynergy(RequiredCount, true);
                lastProgress[member] = RequiredCount;
            }

            sfxProgress = RequiredCount; // trio formed: always play the final note
        }

        if (sfxProgress > 0) PlaySfx(sfxProgress - 1); // one note per pass, not one per tower
    }

    private static void PlaySfx(int order)
    {
        if (AudioManager.instance == null) return;

        int index = SfxIndices[order % SfxIndices.Length];
        Debug.Log($"Synergy SFX -> sfx[{index}]"); // delete once you've confirmed all 3 fire
        AudioManager.instance.PlaySFX(index);
    }
}