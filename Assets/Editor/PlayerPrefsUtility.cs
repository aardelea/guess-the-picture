using UnityEngine;
using UnityEditor;

public class PlayerPrefsUtility : MonoBehaviour
{
    [MenuItem("Tools/Clear Player Preferences")]
    public static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Player preferences cleared.");
    }
}