using System.Collections;
using System.Collections.Generic;
using UnityEngine;

# if UNITY_EDITOR
using UnityEditor;
# endif

[CreateAssetMenu(menuName="Switch/DB",fileName="Switch Database")]
public class SwitchDatabase : ScriptableObject
{
    [SerializeField]
    private List<Switch> switches;

    public IReadOnlyList<Switch> Switches => switches;

# if UNITY_EDITOR
    [ContextMenu("FindSwitch")]
    private void FindSwitch()
    {
        FindDeviceBy<Switch>();
    }

    private void FindIoTSensor()
    {
        //FindDeviceBy<IoT>(); //센서
    }

    private void FindDeviceBy<T>() where T : Switch
    {
        switches = new List<Switch>();

        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}");
        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var switchObj = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            if (switchObj.GetType() == typeof(T) && !switchObj.IsNotUse)
                switches.Add(switchObj);

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
# endif
}
