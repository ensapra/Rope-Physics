using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Test : MonoBehaviour
{
    public RandomClass theFirstOne;
    public List<RandomClass> randomList = new List<RandomClass>();
    void Start()
    {
        for(int i = 0; i< 10; i++)
        {
            if(i == 0)
            {
                randomList.Add(theFirstOne);
            }
            else
                randomList.Add(new RandomClass());
        }
    }
}
[CustomEditor(typeof(Test))]
public class TestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var property = serializedObject.FindProperty(nameof(Test.randomList));
        if (property.arraySize > 0)
        {
            EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(0), new GUIContent("First Element"));
            EditorGUILayout.PropertyField(property);
        }
        else
            base.DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }
}
[System.Serializable]
public class RandomClass
{
    public bool randomBool;
}
