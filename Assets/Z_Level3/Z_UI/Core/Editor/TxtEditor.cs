using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using Z_Ui.Base;

namespace Z_Ui_Editor
{
    [CustomEditor(typeof(Txt), true)]
    [CanEditMultipleObjects]
    public class TxtEditor : TMPro.EditorUtilities.TMP_EditorPanelUI
    {
        SerializedProperty languageTranslatable;
        SerializedProperty imageEnable;
        protected override void OnEnable()
        {
            base.OnEnable();
            languageTranslatable = serializedObject.FindProperty("languageTranslatable");
            imageEnable = serializedObject.FindProperty("imageEnable");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(languageTranslatable); 
            EditorGUILayout.PropertyField(imageEnable);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
