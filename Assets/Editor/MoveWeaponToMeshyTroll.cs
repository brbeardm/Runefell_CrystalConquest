using UnityEditor;
using UnityEngine;

public class MoveWeaponToMeshyTroll
{
    [MenuItem("Tools/Move Weapon to Meshy Troll")]
    public static void MoveWeapon()
    {
        // Find the old weapon mount
        GameObject oldWeaponMount = GameObject.Find("Troll_Idle_5meters/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand/WeaponMount");
        if (oldWeaponMount == null)
        {
            Debug.LogError("Old WeaponMount not found!");
            return;
        }

        // Find the new troll's right hand
        GameObject newRightHand = GameObject.Find("Meshy_CrystalTroll/Armature/Hips/Spine02/Spine01/Spine/RightShoulder/RightArm/RightForeArm/RightHand");
        if (newRightHand == null)
        {
            Debug.LogError("Meshy Troll RightHand not found!");
            return;
        }

        // Duplicate the weapon mount
        GameObject weaponCopy = Object.Instantiate(oldWeaponMount, newRightHand.transform);
        weaponCopy.name = "WeaponMount";

        // Reset local transform to attach properly
        weaponCopy.transform.localPosition = Vector3.zero;
        weaponCopy.transform.localRotation = Quaternion.identity;
        weaponCopy.transform.localScale = Vector3.one;

        EditorUtility.SetDirty(newRightHand);
        EditorUtility.SetDirty(weaponCopy);

        Debug.Log("Weapon successfully moved to Meshy Troll's right hand!");
    }
}
