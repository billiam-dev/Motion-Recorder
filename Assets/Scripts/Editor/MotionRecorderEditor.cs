using UnityEditor;
using UnityEngine;

namespace JARcraft.UnityEditor.MotionRecorder
{
    [CustomEditor(typeof(MotionRecorder), true)]
    public class MotionRecorderEditor : Editor
    {
        MotionRecorder motionRecorder;

        SerializedProperty motionTargetsProperty;
        SerializedProperty stepsPerKeyframeProperty;
        SerializedProperty markAsLegacyProperty;
        SerializedProperty animationClipProperty;
        SerializedProperty clipDurationProperty;

        GUIContent startRecordingButton;
        GUIContent endRecordingButton;
        GUIContent saveRecordingButton;

        protected virtual void OnEnable()
        {
            motionRecorder = (MotionRecorder)target;

            motionTargetsProperty = serializedObject.FindProperty("motionTargets");
            stepsPerKeyframeProperty = serializedObject.FindProperty("stepsPerKeyframe");
            markAsLegacyProperty = serializedObject.FindProperty("markClipAsLegacy");
            animationClipProperty = serializedObject.FindProperty("animationClip");
            clipDurationProperty = serializedObject.FindProperty("clipDuration");

            startRecordingButton = new GUIContent(EditorGUIUtility.IconContent("Record Off"));
            startRecordingButton.tooltip = "Start Recording";

            endRecordingButton = new GUIContent(EditorGUIUtility.IconContent("Record On"));
            endRecordingButton.tooltip = "Stop Recording";

            saveRecordingButton = new GUIContent(EditorGUIUtility.IconContent("SaveAs"));
            saveRecordingButton.tooltip = "Save Recording as Animation";
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Properties
            EditorGUILayout.PropertyField(motionTargetsProperty, new GUIContent("Targets"));
            EditorGUILayout.PropertyField(stepsPerKeyframeProperty);
            EditorGUILayout.PropertyField(markAsLegacyProperty);

            // Button GUI - Start, end and save recording
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            if (motionRecorder.isRecording)
            {
                if (GUI.Button(GUILayoutUtility.GetRect(endRecordingButton, GUI.skin.button), endRecordingButton))
                {
                    motionRecorder.EndRecording();
                }
            }
            else
            {
                if (GUI.Button(GUILayoutUtility.GetRect(startRecordingButton, GUI.skin.button), startRecordingButton))
                {
                    motionRecorder.StartRecording();
                }
            }

            GUI.enabled = !motionRecorder.isRecording && motionRecorder.animationClip != null;
            if (GUI.Button(GUILayoutUtility.GetRect(saveRecordingButton, GUI.skin.button), saveRecordingButton))
            {
                motionRecorder.SaveClipAsAsset();
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();

            // Recording clip GUI
            if (motionRecorder.isRecording || motionRecorder.animationClip != null)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Clip", EditorStyles.boldLabel);

                GUI.enabled = false;
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(animationClipProperty);
                EditorGUILayout.PropertyField(clipDurationProperty, new GUIContent("Duration"));
                
                EditorGUI.indentLevel--;
                GUI.enabled = true;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
