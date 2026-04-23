using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [Header("Data Source")]
    [SerializeField] private ShipDatabase shipDatabase;

    private Dictionary<int, List<ShipData>> shipsByDay = new Dictionary<int, List<ShipData>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadDatabase();
    }

    private void LoadDatabase()
    {
        if (shipDatabase == null)
        {
            Debug.LogError("DataManager: ShipDatabase not assigned in Inspector.");
            return;
        }

        shipsByDay.Clear();

        foreach (var ship in shipDatabase.allShips)
        {
            // бя FIX: use ship.day directly (set by JSON importer), not GetDayFromHullId()
            int day = ship.day;

            if (!shipsByDay.ContainsKey(day))
                shipsByDay[day] = new List<ShipData>();
            shipsByDay[day].Add(ship);
        }

        // Debug: confirm distribution
        foreach (var kvp in shipsByDay)
        {
            Debug.Log($"DataManager: Day {kvp.Key} б· {kvp.Value.Count} ships ({string.Join(", ", kvp.Value.Select(s => s.shipId))})");
        }
    }

    public List<ShipData> GetShipsForDay(int day)
    {
        if (shipsByDay.TryGetValue(day, out var ships))
            return new List<ShipData>(ships);
        else
        {
            Debug.LogWarning($"DataManager: No ship data for day {day}.");
            return new List<ShipData>();
        }
    }

    public ShipData GetShipById(string hullId)
    {
        return shipDatabase?.allShips.FirstOrDefault(s => s.shipId == hullId);
    }

    public int GetTotalDays()
    {
        return shipsByDay.Keys.Count > 0 ? shipsByDay.Keys.Max() : 0;
    }

    public void ReloadDatabase()
    {
        LoadDatabase();
    }
}