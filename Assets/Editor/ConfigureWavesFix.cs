using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;

public class ConfigureWavesFix
{
    public static void Execute()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            Debug.Log("Exited play mode.");
            // We need to wait for play mode to actually exit before modifying scenes
            EditorApplication.delayCall += DoUpdate;
        }
        else
        {
            DoUpdate();
        }
    }

    private static void DoUpdate()
    {
        UpdateScene("Assets/Scenes/Gameplay_BridgeRange.unity");
        UpdateScene("Assets/Scenes/Gameplay.unity");
        EditorSceneManager.OpenScene("Assets/Scenes/Gameplay_BridgeRange.unity", OpenSceneMode.Single);
    }

    private static void UpdateScene(string scenePath)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        WaveSpawner spawner = Object.FindAnyObjectByType<WaveSpawner>();
        
        if (spawner != null)
        {
            GameObject orc = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/Enemy_Orc.prefab");
            GameObject skeleton = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/Enemy_SkeletonWarrior.prefab");
            GameObject troll = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/Enemy_Troll.prefab");
            GameObject balrog = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/Enemy_Balrog.prefab");
            GameObject sauron = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/Enemy_Sauron.prefab");

            WaveSpawner.WaveDefinition[] newWaves = new WaveSpawner.WaveDefinition[6];

            // Wave 1
            newWaves[0] = new WaveSpawner.WaveDefinition {
                waveName = "Orc Horde",
                delayAfterWave = 3f,
                enemyGroups = new WaveSpawner.EnemyGroup[] {
                    new WaveSpawner.EnemyGroup { enemyPrefab = orc, count = 10, spawnInterval = 0.6f }
                }
            };

            // Wave 2
            newWaves[1] = new WaveSpawner.WaveDefinition {
                waveName = "Skeleton Assault",
                delayAfterWave = 3f,
                enemyGroups = new WaveSpawner.EnemyGroup[] {
                    new WaveSpawner.EnemyGroup { enemyPrefab = skeleton, count = 12, spawnInterval = 0.5f }
                }
            };

            // Wave 3
            newWaves[2] = new WaveSpawner.WaveDefinition {
                waveName = "Mixed Horde",
                delayAfterWave = 4f,
                enemyGroups = new WaveSpawner.EnemyGroup[] {
                    new WaveSpawner.EnemyGroup { enemyPrefab = orc, count = 8, spawnInterval = 0.5f },
                    new WaveSpawner.EnemyGroup { enemyPrefab = skeleton, count = 8, spawnInterval = 0.5f }
                }
            };

            // Wave 4
            newWaves[3] = new WaveSpawner.WaveDefinition {
                waveName = "Troll Smash",
                delayAfterWave = 4f,
                enemyGroups = new WaveSpawner.EnemyGroup[] {
                    new WaveSpawner.EnemyGroup { enemyPrefab = orc, count = 10, spawnInterval = 0.4f },
                    new WaveSpawner.EnemyGroup { enemyPrefab = troll, count = 3, spawnInterval = 2.0f }
                }
            };

            // Wave 5
            newWaves[4] = new WaveSpawner.WaveDefinition {
                waveName = "Balrog Rises",
                delayAfterWave = 5f,
                enemyGroups = new WaveSpawner.EnemyGroup[] {
                    new WaveSpawner.EnemyGroup { enemyPrefab = orc, count = 12, spawnInterval = 0.3f },
                    new WaveSpawner.EnemyGroup { enemyPrefab = skeleton, count = 8, spawnInterval = 0.4f },
                    new WaveSpawner.EnemyGroup { enemyPrefab = balrog, count = 1, spawnInterval = 0f }
                }
            };

            // Wave 6
            newWaves[5] = new WaveSpawner.WaveDefinition {
                waveName = "Sauron Arrives",
                delayAfterWave = 5f,
                enemyGroups = new WaveSpawner.EnemyGroup[] {
                    new WaveSpawner.EnemyGroup { enemyPrefab = orc, count = 15, spawnInterval = 0.3f },
                    new WaveSpawner.EnemyGroup { enemyPrefab = troll, count = 3, spawnInterval = 1.5f },
                    new WaveSpawner.EnemyGroup { enemyPrefab = sauron, count = 1, spawnInterval = 0f }
                }
            };

            FieldInfo wavesField = typeof(WaveSpawner).GetField("waves", BindingFlags.NonPublic | BindingFlags.Instance);
            if (wavesField != null)
            {
                wavesField.SetValue(spawner, newWaves);
            }

            FieldInfo endlessField = typeof(WaveSpawner).GetField("endlessMode", BindingFlags.NonPublic | BindingFlags.Instance);
            if (endlessField != null)
            {
                endlessField.SetValue(spawner, true);
            }

            EditorUtility.SetDirty(spawner);
            Debug.Log($"Configured WaveSpawner in {scene.name}");
        }
        
        EditorSceneManager.SaveScene(scene);
    }
}