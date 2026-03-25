using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class SetupHeroCrystal
{
    public static void Execute()
    {
        // 1. Update Mat_HeroCrystal to be transparent blue-purple
        Material crystalMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/HeroCrystal/Mat_HeroCrystal.mat");
        if (crystalMat != null)
        {
            crystalMat.SetFloat("_Surface", 1); // 1 = Transparent
            crystalMat.SetColor("_BaseColor", new Color(0.3f, 0.1f, 0.8f, 0.5f)); // blue-purple with 0.5 alpha
            crystalMat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            crystalMat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            crystalMat.SetInt("_ZWrite", 0);
            crystalMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            crystalMat.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(crystalMat);
        }

        // 2. Create Mat_HeroInside
        Material heroMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        heroMat.SetFloat("_Surface", 1); // Transparent
        heroMat.SetColor("_BaseColor", new Color(1f, 0.84f, 0f, 0.05f)); // golden with 0.05 alpha
        heroMat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        heroMat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        heroMat.SetInt("_ZWrite", 0);
        heroMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        heroMat.renderQueue = (int)RenderQueue.Transparent;
        AssetDatabase.CreateAsset(heroMat, "Assets/_Project/Prefabs/HeroCrystal/Mat_HeroInside.mat");

        // 3. Modify HeroCrystal prefab
        string prefabPath = "Assets/_Project/Prefabs/HeroCrystal/HeroCrystal.prefab";
        GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefab != null)
        {
            // Create HeroMeshInside
            GameObject heroMeshInside = new GameObject("HeroMeshInside");
            heroMeshInside.transform.SetParent(prefab.transform);
            heroMeshInside.transform.localPosition = new Vector3(0, 0, 0);
            heroMeshInside.transform.localRotation = Quaternion.identity;
            heroMeshInside.transform.localScale = Vector3.one;

            // Create capsule (body)
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(heroMeshInside.transform);
            body.transform.localPosition = new Vector3(0, 1.2f, 0);
            body.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.GetComponent<MeshRenderer>().sharedMaterial = heroMat;

            // Create sphere (head)
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(heroMeshInside.transform);
            head.transform.localPosition = new Vector3(0, 2.2f, 0);
            head.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            Object.DestroyImmediate(head.GetComponent<Collider>());
            head.GetComponent<MeshRenderer>().sharedMaterial = heroMat;

            // Create cubes (arms)
            GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftArm.name = "LeftArm";
            leftArm.transform.SetParent(heroMeshInside.transform);
            leftArm.transform.localPosition = new Vector3(-0.6f, 1.4f, 0);
            leftArm.transform.localScale = new Vector3(0.3f, 1f, 0.3f);
            Object.DestroyImmediate(leftArm.GetComponent<Collider>());
            leftArm.GetComponent<MeshRenderer>().sharedMaterial = heroMat;

            GameObject rightArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightArm.name = "RightArm";
            rightArm.transform.SetParent(heroMeshInside.transform);
            rightArm.transform.localPosition = new Vector3(0.6f, 1.4f, 0);
            rightArm.transform.localScale = new Vector3(0.3f, 1f, 0.3f);
            Object.DestroyImmediate(rightArm.GetComponent<Collider>());
            rightArm.GetComponent<MeshRenderer>().sharedMaterial = heroMat;

            // Assign to HeroCrystal component
            var heroCrystalComp = prefab.GetComponent("HeroCrystal");
            if (heroCrystalComp != null)
            {
                SerializedObject so = new SerializedObject(heroCrystalComp);
                so.FindProperty("heroMeshInside").objectReferenceValue = heroMeshInside;
                
                SerializedProperty renderersProp = so.FindProperty("heroRenderers");
                renderersProp.ClearArray();
                
                MeshRenderer[] renderers = heroMeshInside.GetComponentsInChildren<MeshRenderer>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderersProp.InsertArrayElementAtIndex(i);
                    renderersProp.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
                }
                
                so.ApplyModifiedProperties();
            }

            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefab);
            Debug.Log("HeroCrystal prefab updated successfully.");
        }
    }
}