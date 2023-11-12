using UnityEditor;
using UnityEngine;

/******************************************************************************
 * Project: MajorProjectEndlessRunner
 * File: ReadOnlyAttribute.cs
 * Version: 1.0
 * Autor: It3ration
 * Source: https://discussions.unity.com/t/how-to-make-a-readonly-property-in-inspector/75448
 * 
 * These coded instructions, statements, and computer programs contain
 * proprietary information of the author and are protected by Federal
 * copyright law. They may not be disclosed to third parties or copied
 * or duplicated in any form, in whole or in part, without the prior
 * written consent of the author.
 * 
 * ChangeLog
 * ----------------------------
 *  18.08.2022  FM  added Script to project
 *  
 *****************************************************************************/
public class ReadOnlyAttribute : PropertyAttribute
{

}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property,
        GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position,
        SerializedProperty property,
        GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}


