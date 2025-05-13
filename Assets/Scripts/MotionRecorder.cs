using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
using UnityEditor;
using UnityEngine;

namespace Billiam.UEdit.MotionRecorder
{
    public class MotionRecorder : MonoBehaviour
    {
        class TransformMotionRecording
        {
            public Transform transform;
            public List<Keyframe> keyframes;

            public TransformMotionRecording(Transform transform)
            {
                this.transform = transform;
                keyframes = new List<Keyframe>();
            }
        }

        class Keyframe
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 eulerAngles
            {
                get
                {
                    return rotation.eulerAngles;
                }
            }

            public float time;

            public Keyframe(Vector3 position, Quaternion rotation, float time)
            {
                this.position = position;
                this.rotation = rotation;
                this.time = time;
            }
        }

        [Tooltip("Objects you want to record should be in this list (objects must be children of the Motion Recorder)")]
        public Transform[] motionTargets;
        
        [Tooltip("How many physics steps the recorder will wait before creating a keyframe (recordTime >= previousKeyframeTime + (steps * 0.02f))"), Range(2, 10)]
        public int stepsPerKeyframe = 5;
        
        [Tooltip("Marked the created animation as legacy (compatable with the Legacy Animation component)")]
        public bool markClipAsLegacy;

        public AnimationClip animationClip;

        public bool IsRecording
        {
            get;
            private set;
        }

        public float ClipDuration
        {
            get;
            private set;
        }

        public Action onBeginRecording;
        public Action onEndRecording;
        public Action<float> onRecord;

        List<TransformMotionRecording> motionRecordings;

#if UNITY_EDITOR
        public void StartRecording()
        {
            if (IsRecording)
            {
                Debug.Log("Cannot start a new recording while already recording");
                return;
            }

            onBeginRecording?.Invoke();

            IsRecording = true;

            EditorCoroutineUtility.StartCoroutine(RecordKeyframes(), this);
            EditorPhyisicsUtility.StartSimulation();
        }

        public void EndRecording()
        {
            if (!IsRecording)
            {
                Debug.Log("Cannot end recording, not currently recording");
                return;
            }

            onEndRecording?.Invoke();

            EditorPhyisicsUtility.EndSimulation();

            animationClip = MakeAnimationClip(motionRecordings);
            IsRecording = false;
        }

        public void SaveClipAsAsset()
        {
            if (IsRecording)
            {
                Debug.Log("Cannot create new Animation Clip while recording");
                return;
            }

            if (animationClip == null)
            {
                Debug.Log("Could not create new Animation Clip, no saved recording");
                return;
            }

            string fileName = "New Animation Recording";

            // Create motion recordings Folder
            string parentFolder = "Motion Recordings";
            if (!AssetDatabase.IsValidFolder(string.Format("Assets/{0}", parentFolder)))
            {
                AssetDatabase.CreateFolder("Assets", parentFolder);
            }

            // Create asset
            string path = AssetDatabase.GenerateUniqueAssetPath(string.Format("Assets/{0}/{1}.anim", parentFolder, fileName));
            AssetDatabase.CreateAsset(animationClip, path);
            
            // Save assets
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            // Highlight recording in project window
            Selection.activeObject = animationClip;
            EditorUtility.FocusProjectWindow();

            Debug.Log("Saved animation as " + path);
            DeleteStoredAnimation();
        }

        IEnumerator RecordKeyframes()
        {
            float timestamp = Time.realtimeSinceStartup;

            motionRecordings = new List<TransformMotionRecording>();
            foreach (Transform recordedTransform in motionTargets)
            {
                motionRecordings.Add(new TransformMotionRecording(recordedTransform));
            }

            float lastKeyframeTime = float.MinValue;
            ClipDuration = 0;

            while (IsRecording)
            {
                float deltaTime = Mathf.Max(Time.realtimeSinceStartup - timestamp, 0.001f);

                if (ClipDuration >= lastKeyframeTime + (stepsPerKeyframe * 0.02f))
                {
                    foreach (TransformMotionRecording motionRecording in motionRecordings)
                    {
                        motionRecording.keyframes.Add(new Keyframe(motionRecording.transform.localPosition, motionRecording.transform.localRotation, ClipDuration));
                    }

                    lastKeyframeTime = ClipDuration;
                }

                onRecord?.Invoke(ClipDuration);
                ClipDuration += deltaTime;

                timestamp = Time.realtimeSinceStartup;
                yield return null;
            }
        }

        AnimationClip MakeAnimationClip(List<TransformMotionRecording> motionRecordings)
        {
            AnimationClip animationClip = new AnimationClip();
            animationClip.name = "Animation Clip";
            animationClip.legacy = markClipAsLegacy;

            foreach (TransformMotionRecording motionRecording in motionRecordings)
            {
                AddMotionRecordingToAnimationClip(animationClip, motionRecording);
            }

            return animationClip;
        }

        void AddMotionRecordingToAnimationClip(AnimationClip animationClip, TransformMotionRecording motionRecording)
        {
            List<Keyframe> keyframes = motionRecording.keyframes;

            Keyframe first = keyframes[0];
            Keyframe last = keyframes[keyframes.Count - 1];

            AnimationCurve xPosCurve = AnimationCurve.Linear(0, first.position.x, last.time, last.position.x);
            AnimationCurve yPosCurve = AnimationCurve.Linear(0, first.position.y, last.time, last.position.y);
            AnimationCurve zPosCurve = AnimationCurve.Linear(0, first.position.z, last.time, last.position.z);

            AnimationCurve xRotCurve = AnimationCurve.Linear(0, first.rotation.x, last.time, last.rotation.x);
            AnimationCurve yRotCurve = AnimationCurve.Linear(0, first.rotation.y, last.time, last.rotation.y);
            AnimationCurve zRotCurve = AnimationCurve.Linear(0, first.rotation.z, last.time, last.rotation.z);
            AnimationCurve wRotCurve = AnimationCurve.Linear(0, first.rotation.w, last.time, last.rotation.w);

            foreach (Keyframe keyframe in keyframes)
            {
                xPosCurve.AddKey(keyframe.time, keyframe.position.x);
                yPosCurve.AddKey(keyframe.time, keyframe.position.y);
                zPosCurve.AddKey(keyframe.time, keyframe.position.z);

                xRotCurve.AddKey(keyframe.time, keyframe.rotation.x);
                yRotCurve.AddKey(keyframe.time, keyframe.rotation.y);
                zRotCurve.AddKey(keyframe.time, keyframe.rotation.z);
                wRotCurve.AddKey(keyframe.time, keyframe.rotation.w);
            }

            string path = GetObjectRelativePath(motionRecording.transform);
            if (path != "_null")
            {
                animationClip.SetCurve(path, typeof(Transform), "localPosition.x", xPosCurve);
                animationClip.SetCurve(path, typeof(Transform), "localPosition.y", yPosCurve);
                animationClip.SetCurve(path, typeof(Transform), "localPosition.z", zPosCurve);

                animationClip.SetCurve(path, typeof(Transform), "localRotation.x", xRotCurve);
                animationClip.SetCurve(path, typeof(Transform), "localRotation.y", yRotCurve);
                animationClip.SetCurve(path, typeof(Transform), "localRotation.z", zRotCurve);
                animationClip.SetCurve(path, typeof(Transform), "localRotation.w", wRotCurve);
            }
        }

        string GetObjectRelativePath(Transform transform)
        {
            string path = "";
            Transform current = transform;

            while (current != this.transform)
            {
                if (current == null)
                {
                    // Object is not a child of the motion recorder, return null
                    Debug.LogWarning(transform + " is not a child of the motion recorder and has not been recorded");
                    return "_null";
                }

                path = current.name + (current == transform ? "" : "/") + path;
                current = current.parent;
            }

            return path;
        }

        void DeleteStoredAnimation()
        {
            animationClip = null;
            motionRecordings = null;
            ClipDuration = 0;
        }
#endif
    }
}
