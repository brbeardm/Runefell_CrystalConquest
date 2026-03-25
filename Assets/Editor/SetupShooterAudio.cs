using UnityEngine;
using UnityEditor;

public class SetupShooterAudio
{
    [MenuItem("Tools/Setup Shooter Audio")]
    public static void Setup()
    {
        AudioClip fireSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/CrystalShard_Fire.wav");
        if (fireSfx == null)
        {
            Debug.LogError("Fire SFX not found!");
            return;
        }

        string[] prefabPaths = new string[]
        {
            "Assets/_Project/Prefabs/Shooters/CloneShooter.prefab",
            "Assets/_Project/Prefabs/Shooters/HeroShooter.prefab"
        };

        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                // Add AudioSource
                AudioSource audioSource = prefab.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = prefab.AddComponent<AudioSource>();
                }
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D

                // Assign to AutoShooter
                var autoShooter = prefab.GetComponent("AutoShooter");
                if (autoShooter != null)
                {
                    SerializedObject so = new SerializedObject(autoShooter);
                    so.Update();
                    SerializedProperty prop = so.FindProperty("fireSfx");
                    if (prop != null)
                    {
                        prop.objectReferenceValue = fireSfx;
                    }
                    so.ApplyModifiedProperties();
                }

                // Assign to PlayerShooter
                var playerShooter = prefab.GetComponent("PlayerShooter");
                if (playerShooter != null)
                {
                    SerializedObject so = new SerializedObject(playerShooter);
                    so.Update();
                    SerializedProperty prop = so.FindProperty("fireSfx");
                    if (prop != null)
                    {
                        prop.objectReferenceValue = fireSfx;
                    }
                    so.ApplyModifiedProperties();
                }

                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
                Debug.Log("Updated " + prefab.name);
            }
        }
    }
}
