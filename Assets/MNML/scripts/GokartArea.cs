using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



namespace Gokart
{
    public class GokartArea : MonoBehaviour
    {
        [Tooltip("The path the race will take")]
        public CinemachineSmoothPath racePath;

        [Tooltip("The prefab to use for checkpoints")]
        public GameObject checkpointPrefab;

        [Tooltip("The prefab to use for the start/end checkpoint")]
        public GameObject finishCheckpointPrefab;

        [Tooltip("If true, enable training mode")]
        public bool trainingMode;

        public List<GokartAgent> GokartAgents { get; private set; }

        public List<GameObject> Checkpoints { get; private set; }

        public GokartAcademy GokartAcademy { get; private set; }


        //execute on awake
        private void Awake()
        {
            // Find all gokart agents in the area
            GokartAgents = transform.GetComponentsInChildren<GokartAgent>().ToList();
            Debug.Assert(GokartAgents.Count > 0, "No gokartagents found");

            GokartAcademy = FindObjectOfType<GokartAcademy>();
        }

        private void Start()
        {
            // create checkpoints along race path
            Debug.Assert(racePath != null, "Race Path was not set");
            Checkpoints = new List<GameObject>();
            int numCheckpoints = (int)racePath.MaxUnit(CinemachinePathBase.PositionUnits.PathUnits);
            for(int i = 0; i < numCheckpoints; i++)
            {
                //instantiate either a checkpoint or finish line checkpoint
                GameObject checkpoint;
                if (i == numCheckpoints - 1) checkpoint = Instantiate<GameObject>(finishCheckpointPrefab);
                else checkpoint = Instantiate<GameObject>(checkpointPrefab);

                //set the parent, position and rotation
                checkpoint.transform.SetParent(racePath.transform);
                checkpoint.transform.localPosition = racePath.m_Waypoints[i].position;
                checkpoint.transform.rotation = racePath.EvaluateOrientationAtUnit(i, CinemachinePathBase.PositionUnits.PathUnits);

                //add the checkpoint to the list
                Checkpoints.Add(checkpoint);
            }
        }

        public void ResetAgentPosition(GokartAgent agent, bool randomize = false)
        {
            if (randomize)
            {
                // Pick a new next checkpoint at random
                agent.NextCheckpointIndex = Random.Range(0, Checkpoints.Count);
            }

            // Set start position to the previous checkpoint
            int previousCheckpointIndex = agent.NextCheckpointIndex - 1;
            if (previousCheckpointIndex == -1) previousCheckpointIndex = Checkpoints.Count - 1;

            float startPosition = racePath.FromPathNativeUnits(previousCheckpointIndex, CinemachinePathBase.PositionUnits.PathUnits);

            // Convert the position on the race path to a position in 3d space
            Vector3 basePosition = racePath.EvaluatePosition(startPosition);

            // Get the orientation at that position on the race path
            Quaternion orientation = racePath.EvaluateOrientation(startPosition);

            // Calculate a horizontal offset so that agents are spread out
            Vector3 positionOffset = Vector3.right * (GokartAgents.IndexOf(agent) - GokartAgents.Count / 2f)
                * UnityEngine.Random.Range(9f, 10f);

            // Set the gokart position and rotation
            agent.transform.position = basePosition + orientation * positionOffset;
            agent.transform.rotation = orientation;


        }
    }
}
