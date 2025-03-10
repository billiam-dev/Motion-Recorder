using System.Collections;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
using UnityEngine;

public class EditorPhyisicsUtility : MonoBehaviour
{
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
    public static void StartSimulation(MonoBehaviour owner)
    {
        if (isSimulating)
        {
            Debug.LogWarning("Simulation already running!");
            return;
        }

        isSimulating = true;
        EditorCoroutineUtility.StartCoroutine(Simulate(), owner);
    }

    public static void EndSimulation()
    {
        if (!isSimulating)
        {
            Debug.LogWarning("Simulation is not running");
            return;
        }

        isSimulating = false;
    }

    static IEnumerator Simulate()
    {
        SaveSimulationState();
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
        ResetSimulation();
    }

    static void SaveSimulationState()
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

    static void ResetSimulation()
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
