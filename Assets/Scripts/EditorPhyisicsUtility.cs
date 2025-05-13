using System.Collections;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
using UnityEngine;

public class EditorPhyisicsUtility
{
    // TODO: If the simulation is running, immediatly end it before play mode is toggled

    struct Transform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    static Rigidbody[] physicsObjects;
    static Transform[] transforms;
    static bool isSimulating;

#if UNITY_EDITOR
    public static void StartSimulation()
    {
        if (isSimulating)
        {
            Debug.LogWarning("Cannot start a new simulation while one is already running!");
            return;
        }

        isSimulating = true;
        EditorCoroutineUtility.StartCoroutineOwnerless(Simulate());
    }

    public static void EndSimulation()
    {
        if (!isSimulating)
        {
            Debug.LogWarning("Cannot end simulation, there is no simulation running");
            return;
        }

        isSimulating = false;
    }

    static IEnumerator Simulate()
    {
        SavePhysicsSnapshot();
        Physics.simulationMode = SimulationMode.Script;
        float timestamp = Time.realtimeSinceStartup;

        while (isSimulating)
        {
            float deltaTime = Mathf.Max(Time.realtimeSinceStartup - timestamp, 0.001f);

            Physics.Simulate(deltaTime);

            timestamp = Time.realtimeSinceStartup;
            yield return null;
        }

        Physics.simulationMode = SimulationMode.FixedUpdate;
        LoadPhysicsSnapshot();
    }

    static void SavePhysicsSnapshot()
    {
        physicsObjects = MonoBehaviour.FindObjectsOfType<Rigidbody>();
        transforms = new Transform[physicsObjects.Length];

        for (int i = 0; i < physicsObjects.Length; i++)
        {
            transforms[i].position = physicsObjects[i].transform.localPosition;
            transforms[i].rotation = physicsObjects[i].transform.localRotation;
            transforms[i].scale = physicsObjects[i].transform.localScale;
        }
    }

    static void LoadPhysicsSnapshot()
    {
        for (int i = 0; i < physicsObjects.Length; i++)
        {
            physicsObjects[i].transform.SetLocalPositionAndRotation(transforms[i].position, transforms[i].rotation);
            physicsObjects[i].transform.localScale = transforms[i].scale;

            if (physicsObjects[i].isKinematic == false)
            {
                physicsObjects[i].velocity = Vector3.zero;
                physicsObjects[i].angularVelocity = Vector3.zero;
            }
        }

        physicsObjects = null;
        transforms = null;
    }
#endif
}
