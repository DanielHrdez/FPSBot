using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class FPSBot : Agent
{
    public float speed = 5f;
    public float turnSpeed = 180f;
    public FPSBot target;
    public float maxDistance = 10f;
    private Vector3 gunAimDirection = Vector3.forward;

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

        // Rewards, kill target
        if (shoot && KillTarget())
        {
            target.Death();
            SetReward(10f);
            EndEpisode();
        }
        
        // Rewards, see target
        if (CanSeeTarget())
        {
            AddReward(.1f);
        }

        // Rewards, not see target, see and not shoot, not see and shoot
        if ((!CanSeeTarget()) || (CanSeeTarget() && !shoot) || (!CanSeeTarget() && shoot))
        {
            AddReward(-.1f);
        }
    }

    public void Death()
    {
        SetReward(-10f);
        EndEpisode();
    }

    // CanSeeTarget
    private bool CanSeeTarget(float maxAngle = 30f)
    {
        Vector3 directionToTarget = target.transform.localPosition - transform.localPosition;
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        return angle < maxAngle;
    }

    // KillTarget, if Raycast hit target, return true
    private bool KillTarget()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.localPosition, gunAimDirection, out hit))
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
        // Target and Agent loacl positions
        sensor.AddObservation(target.transform.localPosition);
        sensor.AddObservation(transform.localPosition);
    }

    public override void OnEpisodeBegin()
    {
        // Reset target position in local range between -maxDistance and maxDistance
        target.transform.localPosition = new Vector3(
            Random.Range(-maxDistance, maxDistance),
            1f,
            Random.Range(-maxDistance, maxDistance)
        );
    }

    private void Update()
    {
        // GunAimDirection debug ray
        Debug.DrawRay(transform.localPosition, gunAimDirection * 10f, Color.red);
    }
}
