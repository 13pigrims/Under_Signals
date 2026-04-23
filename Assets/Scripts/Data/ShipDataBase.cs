using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShipDatabase", menuName = "Game/Ship Database")]
public class ShipDatabase : ScriptableObject
{
    public List<ShipData> allShips;

    public DayNarrative[] dayNarratives;

    /// <summary>
    /// Get the narrative data for a specific day
    /// </summary>
    public DayNarrative GetNarrative(int day)
    {
        if (dayNarratives == null) return null;
        foreach (var n in dayNarratives)
        {
            if (n.day == day) return n;
        }
        return null;
    }
}

[System.Serializable]
public class DayNarrative
{
    public int day;

    [TextArea(3, 10)]
    public string narrativeOpen;

    [TextArea(3, 10)]
    public string narrativeCloseDefault;

    [TextArea(3, 10)]
    public string narrativeCloseCaught;

    [TextArea(3, 10)]
    public string narrativeCloseMissed;

    [TextArea(5, 20)]
    public string manifestDisplayText;         // <-- NEW: the Daily Arrivals panel text

    public ManifestEntry[] manifest;
}

[System.Serializable]
public class ManifestEntry
{
    public string shipId;
    public string intent;
}