using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using Z_Ui.Base;

namespace Z_Ui_Editor
{
    [CustomEditor(typeof(Img), true)]
    [CanEditMultipleObjects]
    public class ImgEditor : ImageEditor
    {
        SerializedProperty cn;
        SerializedProperty en;
        protected override void OnEnable()
        {
            base.OnEnable();
            cn = serializedObject.FindProperty("cn");
            en = serializedObject.FindProperty("en");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(cn);
            EditorGUILayout.PropertyField(en);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
