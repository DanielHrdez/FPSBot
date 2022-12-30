using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class FPSBot : Agent
{
    public string name;
    public float speed = 5f;
    public float turnSpeed = 180f;
    public FPSBot target;
    public float maxDistance = 5f;
    private Vector3 gunAimDirection = Vector3.forward;
    private int countAim = 0;
    private const int numSensors = 10;
    public float angleSensor = 90f;

    public override void OnActionReceived(ActionBuffers vectorAction)
    {
        // Actions, fordward, turn, vertical turn and shoot
        float forwardAmount = vectorAction.ContinuousActions[0];
        float turnAmount = vectorAction.ContinuousActions[1];
        float verticalTurnAmount = vectorAction.ContinuousActions[2];
        bool shoot = vectorAction.DiscreteActions[0] == 1;

        // Move
        transform.Translate(Vector3.forward * forwardAmount * speed * Time.deltaTime);
        transform.Rotate(Vector3.up * turnAmount * turnSpeed * Time.deltaTime);
        gunAimDirection.x = transform.forward.x;
        gunAimDirection.z = transform.forward.z;
        gunAimDirection.y = verticalTurnAmount;

        bool canKill = CanKill();

        // // Rewards, kill target
        if (shoot && canKill)
        {
            AddReward(100f);
            EndEpisode();
        }
        
        // Rewards based on angle to target
        // float angle = Vector3.Angle(
        //     transform.forward,
        //     target.transform.localPosition - transform.localPosition
        // );
        // AddReward(1f - angle / 90f);
        // if angle less than 5, reward 1, else reward -1
        // if (angle < 10f)
        if (CanKill())
        {
            countAim++;
            AddReward(1f);
        }
        else
        {
            countAim = 0;
        }

        // Rewards, can kill and not shoot, cant kill and shoot
        if ((canKill && !shoot) || (!canKill && shoot))
        {
            AddReward(-1f);
        }
    }

    // CanKill, if Raycast hit target, return true
    private bool CanKill()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, gunAimDirection, out hit))
        {
            if (hit.collider.TryGetComponent<FPSBot>(out FPSBot bot))
            {
                return bot == target;
            }
        }
        return false;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Actions, fordward, turn, vertical turn and shoot
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
        continuousActionsOut[2] = Input.mousePosition.y / Screen.height * 2f - 1f;

        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = Input.GetButton("Fire1") ? 1 : 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent local positions
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(target.transform.localPosition);
        sensor.AddObservation(transform.forward);

        // Add the raycast info (from current to target)
        sensor.AddObservation(CanKill());

        // Add the sensors info
        float[] sensors = GetDistanceSensors();
        for (int i = 0; i < numSensors; i++)
        {
            sensor.AddObservation(sensors[i]);
        }
    }

    private float[] GetDistanceSensors()
    {
        float[] sensors = new float[numSensors];
        for (int i = 0; i < numSensors; i++)
        {
            sensors[i] = GetDistanceSensor(i);
        }
        return sensors;
    }

    // Method to return the distance of sensor i
    private float GetDistanceSensor(int i)
    {
        float angle = i * angleSensor / (numSensors - 1) - angleSensor / 2f;
        Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit))
        {
            return hit.distance;
        }
        return 100f;
    }

    public override void OnEpisodeBegin()
    {
        ResetLocation();
    }

    private void ResetLocation()
    {
        target.transform.localPosition = new Vector3(
            Random.Range(-maxDistance, maxDistance),
            1f,
            Random.Range(-maxDistance, maxDistance)
        );
    }

    private void Update()
    {
        // GunAimDirection debug ray
        Debug.DrawRay(transform.position, gunAimDirection * 10f, Color.red);
    }
}
