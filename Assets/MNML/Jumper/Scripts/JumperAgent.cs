using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class JumperAgent : Agent
{
    Rigidbody rBody;
    GameObject[] goal;

    public Transform platforms;

    public Material GoalMaterial;
    public Material SafeMaterial;

    
    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        goal = GameObject.FindGameObjectsWithTag("Goal");

    }

    void ResetPlatforms()
    {
        foreach (Transform child in platforms)
        {
            child.tag = "Safe";
            child.GetComponent<Renderer>().material = SafeMaterial;
        }

        //Debug.Log(platforms.childCount);
        Transform newGoal = platforms.GetChild(Random.Range(0, platforms.childCount));
        newGoal.tag = "Goal";
        newGoal.GetComponent<Renderer>().material = GoalMaterial;
        goal = GameObject.FindGameObjectsWithTag("Goal");
    }

    public override void AgentReset()
    {
        if(this.transform.position.y < 0.65f || this.transform.position.y > 20)
        {
            //If the Agent Falls off, zero its momentum
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.position = new Vector3(0, 1, 0);
        }

        //Assign new goal to random tile by changing its tag and material
        ResetPlatforms();
        //Target.position = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
    }

    public override void CollectObservations()
    {
        // Target and Agent positions
        AddVectorObs(goal[0].transform.position);
        AddVectorObs(this.transform.position);

        // Agent velocity
        AddVectorObs(rBody.velocity.x);
        AddVectorObs(rBody.velocity.y);
        AddVectorObs(rBody.velocity.z);
    }

    public float speed = 10;
    int score = 0;


    //Detect collisions between the GameObjects with Colliders attached
    void OnCollisionEnter(Collision collision)
    {
        //Check for a match with the specific tag on any GameObject that collides with your GameObject
        if (collision.gameObject.tag == "Goal")
        {
            Debug.Log("WOW GOOD JOB!!!");
            score += 1;
            Debug.Log(score);
            SetReward(10.0f * score);
            Done();
        }
    }
    public override void AgentAction(float[] vectorAction)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
        controlSignal.y = vectorAction[2]*5;
        rBody.AddForce(controlSignal * speed);

        // Rewards

        

        float distanceToTarget = Vector3.Distance(this.transform.position,
                                                  goal[0].transform.position);

        

        if (distanceToTarget > 50)
        {
            SetReward(0.0f);
            Debug.Log("WOW YOU IDIOT!!!");
            score = 0;
            Done();
        }

        

        // Fell off platform
        if (this.transform.position.y < 0.65f)
        {
            SetReward(0.0f);
            score = 0;
            Done();
        }

        // Reset if Stuck
        //var speedy = rBody.velocity.magnitude;
        //Debug.Log(speedy);
        

    }

    public override float[] Heuristic()
    {
        var action = new float[3];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        action[2] = Input.GetAxis("Jump");
        return action;
    }

}
