using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(PageController))]
public class MyMonoBehaviourEditor : Editor
{
    private ReorderableList list;
    private string[] enumNames;

    private void OnEnable()
    {
        enumNames = Enum.GetNames(typeof(PageName));

        list = new(serializedObject, serializedObject.FindProperty("pages"), true, true, true, true)
        {
            drawHeaderCallback = rect => EditorGUI.LabelField(rect, "pages")
        };

        list.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            string title = index < enumNames.Length ? enumNames[index] : $"Element {index}";

            rect.y += 2;
            EditorGUI.PropertyField(rect, element, new GUIContent(title), true);
        };
        // 自動對齊 enum 長度
        list.onCanAddCallback = l => false;
        list.onCanRemoveCallback = l => false;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 讓 pages 數量自動等於 enum 長度
        var listProp = serializedObject.FindProperty("pages");
        int targetCount = enumNames.Length;
        while (listProp.arraySize < targetCount)
            listProp.InsertArrayElementAtIndex(listProp.arraySize);
        while (listProp.arraySize > targetCount)
            listProp.DeleteArrayElementAtIndex(listProp.arraySize - 1);

        list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}