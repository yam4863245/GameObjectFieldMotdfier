using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

public class GameObjectFieldMotdfier : EditorWindow
{
    static EditorWindow Window;

    static GameObject[] GObjects;
    static Component[] components;
    static string[] componentNames, fieldNames;
    static FieldInfo[] fields;
    static int SelectedComponentIndex;
    static int SelectedFieldIndex;
    static float StartSortNum, EndSortNum, Interval = 1;
    static BindingFlags flags = BindingFlags.Default;

    [MenuItem("Tools/物件屬性修改器")]
    public static void ShowWindow()
    {
        if (Window != null) return;
        Window = GetWindow<GameObjectFieldMotdfier>("物件參數修改器");
        Selection.selectionChanged += OnObjectChanged;
        OnObjectChanged();
    }

    private void OnDestroy()
    {
        Selection.selectionChanged -= OnObjectChanged;
    }

    private void OnGUI()
    {
        if (null == GObjects || GObjects.Length <= 0)
        {
            GUILayout.Label("請先選擇物件");
            return;
        }
        GUILayout.Label(string.Format("已選擇{0}個物件", EndSortNum));
        ComponentLayout();
        PropertyLayout();
        SortLayout();
    }

    static void OnObjectChanged()
    {
        GObjects = Selection.gameObjects;
        if (null == GObjects || GObjects.Length <= 0)
            return;
        System.Array.Sort(GObjects, new UnityTransformSort());
        SelectedComponentIndex = SelectedFieldIndex = 0;
        components = GObjects[0].GetComponents<Component>();
        componentNames = new string[components.Length];
        for (int i = 0; i < components.Length; i++)
            componentNames[i] = components[i].GetType().Name;
        EndSortNum = GObjects.Length;
        flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        OnSelectComponentChanged();
    }

    static void OnSelectComponentChanged()
    {
        fields = components[SelectedComponentIndex].GetType().GetFields(flags).Where((x)=>x.FieldType.IsValueType).ToArray();
        fieldNames = new string[fields.Length];
        for (int i = 0; i < fields.Length; i++)
        {
            //Debug.LogFormat("{0} is value type:{1}", fields[i].FieldType, fields[i].FieldType.IsValueType);
            fieldNames[i] = fields[i].Name;
        }
    }

    static void ComponentLayout()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("欲修改的組件(Component):");
        EditorGUI.BeginChangeCheck();
        SelectedComponentIndex = EditorGUILayout.Popup(SelectedComponentIndex, componentNames);
        if (EditorGUI.EndChangeCheck())
            OnSelectComponentChanged();
        GUILayout.EndHorizontal();
    }

    static void PropertyLayout()
    {
        //GUILayout.Label("顯示私有參數(Field):");
        //EditorGUI.BeginChangeCheck();
        //GUILayout.Toggle(flags == BindingFlags.Default,"");
        //if (EditorGUI.EndChangeCheck())
        //{
        //    flags = flags == BindingFlags.Default ? BindingFlags.NonPublic : BindingFlags.Default;
        //    OnSelectComponentChanged();
        //}
        GUILayout.BeginHorizontal();
        GUILayout.Label("欲更改的參數(Field):");
        SelectedFieldIndex = EditorGUILayout.Popup(SelectedFieldIndex, fieldNames);
        GUILayout.EndHorizontal();
    }

    static void SortLayout()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("起始:");
        StartSortNum = EditorGUILayout.FloatField(StartSortNum);
        GUILayout.Label("每個物件增加:");
        Interval = EditorGUILayout.FloatField(Interval);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("執行"))
        {
            System.Type type = components[SelectedComponentIndex].GetType();
            FieldInfo fieldInfo = fields[SelectedFieldIndex];
            for (int i = 0; i < GObjects.Length; i++)
            {
                var o = GObjects[i].GetComponent(type);
                fieldInfo.SetValue(o, StartSortNum + i * Interval);
                EditorUtility.SetDirty(o);
            }

        }
    }
}

public class UnityTransformSort : System.Collections.Generic.IComparer<GameObject>
{
    public int Compare(GameObject lhs, GameObject rhs)
    {
        if (lhs == rhs) return 0;
        if (lhs == null) return -1;
        if (rhs == null) return 1;
        return (lhs.transform.GetSiblingIndex() > rhs.transform.GetSiblingIndex()) ? 1 : -1;
    }
}
