using UnityEditor;
using UnityEngine;

public class MyExt : EditorWindow
{
    private int param1 = -1;
    private float param2 = 0;
    private string name = "";
    private int id = 0;

    [MenuItem("MyExt/Edit", false, 1)]
    private static void Init()
    {
        MyExt editor = (MyExt)EditorWindow.GetWindow(typeof(MyExt));
        editor.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Section1", EditorStyles.boldLabel);
        param1 = EditorGUILayout.IntField("param1 int", param1);
        if (GUILayout.Button("Load"))
        {
            Debug.Log("Load");
        }

        GUILayout.Label("Section2", EditorStyles.boldLabel);
        param2 = EditorGUILayout.FloatField("param2 float", param2);
        name = EditorGUILayout.TextField("Name", name);
        id = EditorGUILayout.IntSlider("id 0~8", id, 0, 100);
    }
}