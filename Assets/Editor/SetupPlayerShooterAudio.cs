using UnityEngine;
using UnityEditor;

public class SetupPlayerShooterAudio
{
    [MenuItem("Tools/Setup Player Shooter Audio")]
    public static void Setup()
    {
        AudioClip fireSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/CrystalShard_Fire.wav");
        if (fireSfx == null)
        {
            Debug.LogError("Fire SFX not found!");
            return;
        }

        GameObject player = GameObject.Find("---Player---/Player");
        if (player != null)
        {
            // Add AudioSource
            AudioSource audioSource = player.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = player.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D

            // Assign to PlayerShooter
            var playerShooter = player.GetComponent("PlayerShooter");
            if (playerShooter != null)
            {
                SerializedObject so = new SerializedObject(playerShooter);
                so.Update();
                SerializedProperty prop = so.FindProperty("fireSfx");
                if (prop != null)
                {
                    prop.objectReferenceValue = fireSfx;
                }
                SerializedProperty audioProp = so.FindProperty("audioSource");
                if (audioProp != null)
                {
                    audioProp.objectReferenceValue = audioSource;
                }
                so.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(player);
            Debug.Log("Updated Player in scene");
        }
        else
        {
            Debug.LogError("Player not found in scene!");
        }
    }
}
