using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class ShipDatabaseImporter : EditorWindow
{
    [MenuItem("Tools/Import ShipDatabase from JSON")]
    public static void ShowWindow()
    {
        GetWindow<ShipDatabaseImporter>("JSON Importer");
    }

    private string jsonPath = "Assets/Resources/ship_data.json";
    private ShipDatabase targetDatabase;

    private void OnGUI()
    {
        GUILayout.Label("Import Ship Data from JSON", EditorStyles.boldLabel);
        jsonPath = EditorGUILayout.TextField("JSON Path", jsonPath);
        targetDatabase = (ShipDatabase)EditorGUILayout.ObjectField("Target Database", targetDatabase, typeof(ShipDatabase), false);

        if (GUILayout.Button("Import"))
        {
            if (targetDatabase == null)
            {
                Debug.LogError("Please assign a target ShipDatabase.");
                return;
            }
            Import();
        }
    }

    private void Import()
    {
        string fullPath = Path.Combine(Application.dataPath, jsonPath.Replace("Assets/", ""));
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"JSON file not found at {fullPath}");
            return;
        }

        string json = File.ReadAllText(fullPath);
        var root = JsonUtility.FromJson<GameDataRoot>(json);

        targetDatabase.allShips.Clear();
        targetDatabase.dayNarratives = new DayNarrative[root.days.Length];

        int shipCount = 0;
        // 新增队列索引
        int shipIndex = 0;
        for (int i = 0; i < root.days.Length; i++)
        {
            var dayData = root.days[i];

            // Build narrative (now includes manifest_display_text)
            var narrative = new DayNarrative
            {
                day = dayData.day,
                narrativeOpen = dayData.narrative_open,
                narrativeCloseDefault = dayData.narrative_close_default,
                narrativeCloseCaught = dayData.narrative_close_caught,
                narrativeCloseMissed = dayData.narrative_close_missed,
                manifestDisplayText = dayData.manifest_display_text,
                manifest = dayData.manifest
            };
            targetDatabase.dayNarratives[i] = narrative;

            // Build ships
            foreach (var ship in dayData.ships)
            {
                // 生成当前船只索引数字
                string uniqueId = "ship_" + shipIndex.ToString("D2");
                var shipData = new ShipData
                {
                    day = dayData.day,
                    shipId = ship.shipId,
                    hullCode = ship.hullCode,
                    hullRoute = ship.hullRoute,
                    q2_hull = ship.q2_hull,
                    q2_life = ship.q2_life,
                    q2_comms = ship.q2_comms,
                    q3_bayA = ship.q3_bayA,
                    q3_bayB = ship.q3_bayB,
                    q3_bayC = ship.q3_bayC,
                    q4_waveform = ship.q4_waveform,
                    q4_intent = ship.q4_intent,
                    warningCode = ship.warningCode,
                    warningCodeMeaning = ship.warningCodeMeaning,
                    isMimic = ship.isMimic,
                    anomalyExplanation = ship.anomalyExplanation,

                    // Map enum indices from JSON
                    status = (ShipStatus)ship.correctStatus,
                    cargo = (ShipCargo)ship.correctCargo,
                    intent = (ShipIntent)ship.correctIntent,
                    // 添加图片加载路径
                    morseImage = $"Ships/{uniqueId}/{uniqueId}_morse",
                    colorsImage = $"Ships/{uniqueId}/{uniqueId}_colors",
                    shapesImage = $"Ships/{uniqueId}/{uniqueId}_shapes",
                    waveformImage = $"Ships/{uniqueId}/{uniqueId}_waveform",
                    centerIconImage = $"Ships/{uniqueId}/{uniqueId}_center_icon",

                    terminalRevealText = GenerateTerminalText(ship, dayData.day)

                };
                targetDatabase.allShips.Add(shipData);
                shipCount++;
                shipIndex++; // 增加索引计数器
            }
        }

        EditorUtility.SetDirty(targetDatabase);
        AssetDatabase.SaveAssets();
        Debug.Log($"Successfully imported {shipCount} ships across {root.days.Length} days.");
    }

    private string GenerateTerminalText(ShipJson ship, int day)
    {
        string warning = string.IsNullOrEmpty(ship.warningCode) ? "NONE" : ship.warningCode;
        return $"> SIGNAL SOURCE: {ship.shipId}\n" +
               $"> ROUTE: {ship.hullRoute}\n" +
               $"> DAY: {day}\n" +
               $"> WARNING: {warning}\n" +
               $"────────────────────────\n" +
               $"> Q1 HULL CODE: {ship.hullCode}\n" +
               $"> Q2 STATUS:    {ship.q2_hull} / {ship.q2_life} / {ship.q2_comms}\n" +
               $"> Q3 CARGO:     {ship.q3_bayA} / {ship.q3_bayB} / {ship.q3_bayC}\n" +
               $"> Q4 WAVEFORM:  {ship.q4_waveform}\n" +
               $"────────────────────────\n" +
               $"> AWAITING DECODE...";
    }

    // ─── JSON wrapper classes ─────────────────────────────

    [System.Serializable]
    public class GameDataRoot
    {
        public DayJson[] days;
    }

    [System.Serializable]
    public class DayJson
    {
        public int day;
        public string narrative_open;
        public string narrative_close_default;
        public string narrative_close_caught;
        public string narrative_close_missed;
        public string manifest_display_text;   // <-- NEW
        public ManifestEntry[] manifest;
        public ShipJson[] ships;
    }

    [System.Serializable]
    public class ShipJson
    {
        public string shipId;
        public string hullCode;
        public string hullRoute;
        public string q2_hull;
        public string q2_life;
        public string q2_comms;
        public string q3_bayA;
        public string q3_bayB;
        public string q3_bayC;
        public string q4_waveform;
        public string q4_intent;
        public string warningCode;
        public string warningCodeMeaning;
        public bool isMimic;
        public string anomalyExplanation;
        public int correctStatus;              // <-- NEW: maps to ShipStatus enum
        public int correctCargo;               // <-- NEW: maps to ShipCargo enum
        public int correctIntent;              // <-- NEW: maps to ShipIntent enum
    }
}