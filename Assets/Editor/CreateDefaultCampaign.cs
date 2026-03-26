using UnityEngine;
using UnityEditor;

public static class CreateDefaultCampaign
{
    [MenuItem("Runefell/Create Default Campaign Asset")]
    public static void Create()
    {
        var config = ScriptableObject.CreateInstance<WaveScalingConfig>();
        config.totalWaves = 9;
        config.waves = new WaveScalingConfig.WaveParams[]
        {
            // Wave 1 — Orc Boss (intro) ~3 min
            new WaveScalingConfig.WaveParams {
                waveName = "The Orc Horde",
                orcCount = 400, orcHP = 30, orcSpeed = 1.6f, orcSpawnInterval = 0.4f,
                bossHP = 800, bossSpeed = 0.8f, bossSpawnDelay = 40f,
                crystalHitsToBreak = 50, buffDuration = 30f, breatherSeconds = 8f
            },
            // Wave 2 — Troll Boss ~3 min
            new WaveScalingConfig.WaveParams {
                waveName = "Troll Assault",
                orcCount = 500, orcHP = 35, orcSpeed = 1.8f, orcSpawnInterval = 0.34f,
                bossHP = 1000, bossSpeed = 0.7f, bossSpawnDelay = 45f,
                crystalHitsToBreak = 60, buffDuration = 28f, breatherSeconds = 7f
            },
            // Wave 3 — Wraith Boss ~3.5 min
            new WaveScalingConfig.WaveParams {
                waveName = "Wraith Shadows",
                orcCount = 600, orcHP = 40, orcSpeed = 2.0f, orcSpawnInterval = 0.3f,
                bossHP = 900, bossSpeed = 2.0f, bossSpawnDelay = 50f,
                crystalHitsToBreak = 70, buffDuration = 26f, breatherSeconds = 7f
            },
            // Wave 4 — Sorcerer Boss ~3.5 min
            new WaveScalingConfig.WaveParams {
                waveName = "Sorcerer's Gambit",
                orcCount = 650, orcHP = 45, orcSpeed = 2.2f, orcSpawnInterval = 0.28f,
                bossHP = 1200, bossSpeed = 0.8f, bossSpawnDelay = 55f,
                crystalHitsToBreak = 80, buffDuration = 25f, breatherSeconds = 6f
            },
            // Wave 5 — Crystal Orc Boss ~4 min
            new WaveScalingConfig.WaveParams {
                waveName = "Crystal Orc Siege",
                orcCount = 750, orcHP = 50, orcSpeed = 2.4f, orcSpawnInterval = 0.26f,
                bossHP = 1500, bossSpeed = 0.7f, bossSpawnDelay = 60f,
                crystalHitsToBreak = 90, buffDuration = 24f, breatherSeconds = 6f
            },
            // Wave 6 — Crystal Troll Boss ~4 min
            new WaveScalingConfig.WaveParams {
                waveName = "Crystal Troll Fury",
                orcCount = 850, orcHP = 55, orcSpeed = 2.6f, orcSpawnInterval = 0.24f,
                bossHP = 1800, bossSpeed = 0.7f, bossSpawnDelay = 60f,
                crystalHitsToBreak = 100, buffDuration = 22f, breatherSeconds = 5f
            },
            // Wave 7 — Crystal Wraith Boss ~4.5 min
            new WaveScalingConfig.WaveParams {
                waveName = "Crystal Wraith Haunting",
                orcCount = 950, orcHP = 60, orcSpeed = 2.8f, orcSpawnInterval = 0.22f,
                bossHP = 1600, bossSpeed = 1.6f, bossSpawnDelay = 55f,
                crystalHitsToBreak = 110, buffDuration = 21f, breatherSeconds = 5f
            },
            // Wave 8 — Crystal Sorcerer Boss ~4.5 min
            new WaveScalingConfig.WaveParams {
                waveName = "Crystal Sorcerer Onslaught",
                orcCount = 1050, orcHP = 70, orcSpeed = 3.0f, orcSpawnInterval = 0.2f,
                bossHP = 2000, bossSpeed = 0.8f, bossSpawnDelay = 60f,
                crystalHitsToBreak = 120, buffDuration = 20f, breatherSeconds = 4f
            },
            // Wave 9 — Crystal Necromancer Boss ~5 min
            new WaveScalingConfig.WaveParams {
                waveName = "Crystal Necromancer Rising",
                orcCount = 1250, orcHP = 80, orcSpeed = 3.2f, orcSpawnInterval = 0.18f,
                bossHP = 2500, bossSpeed = 0.7f, bossSpawnDelay = 60f,
                crystalHitsToBreak = 130, buffDuration = 18f, breatherSeconds = 4f
            },
        };

        string path = "Assets/_Project/Data/Campaign/DefaultCampaign.asset";
        AssetDatabase.CreateAsset(config, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = config;
        Debug.Log($"Created campaign asset at {path}");
    }
}
