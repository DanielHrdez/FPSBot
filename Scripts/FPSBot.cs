using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class FPSBot : Agent
{
    public FPSEnv env;
    public float speed = 5f;
    public float turnSpeed = 90f;
    // public FPSBot target;
    public float maxDistance = 10f;
    private Vector3 gunAimDirection = Vector3.forward;
    // private int countAim = 0;
    private const int numSensors = 10;
    public float angleSensor = 120f;
    // public int[,] grid = new int[1, 1];

    public int maxAmmo = 10;
    private int ammo;
    public void ResetAmmo()
    {
        ammo = maxAmmo;
    }

    private enum SensorType
    {
        Wall,
        Enemy,
    }

    public bool alive = true;
    public int team = 0;

    public float ammoCooldown = 0.1f;

    private Vector3 lastPosition = Vector3.zero;
    public float penalty = 0f;

    public override void OnActionReceived(ActionBuffers vectorAction)
    {
        if (!alive) return;
        // Actions, fordward, turn, vertical turn and shoot
        // float forwardAmount = vectorAction.ContinuousActions[0];
        // float turnAmount = vectorAction.ContinuousActions[1];
        // float verticalTurnAmount = vectorAction.ContinuousActions[2];
        bool shoot = false;
        if (ammo > 0)
        {
            shoot = vectorAction.DiscreteActions[0] == 1;
            if (shoot && ammoCooldown <= 0)
            {
                ammoCooldown = 0.1f;
                ammo--;
            }
            ammoCooldown -= Time.deltaTime;
        }
        else
        {
            env.WithoutAmmo(this.team);
        }
        int forward = vectorAction.DiscreteActions[1];
        int turn = vectorAction.DiscreteActions[2];
        // int reload = vectorAction.DiscreteActions[2];
        // int verticalTurn = vectorAction.DiscreteActions[2];

        float forwardAmount = 0f;
        if (forward == 1) { forwardAmount = 1f; }
        if (forward == 2) { forwardAmount = -1f; }

        float turnAmount = 0f;
        if (turn == 1) { turnAmount = 1f; }
        if (turn == 2) { turnAmount = -1f; }

        // float verticalTurnAmount = 0f;
        // if (verticalTurn == 1) { verticalTurnAmount = 1f; }
        // if (verticalTurn == 2) { verticalTurnAmount = -1f; }

        // Move
        transform.Translate(Vector3.forward * forwardAmount * speed * Time.deltaTime);
        transform.Rotate(Vector3.up * turnAmount * turnSpeed * Time.deltaTime);
        gunAimDirection.x = transform.forward.x;
        gunAimDirection.z = transform.forward.z;
        // gunAimDirection.y = verticalTurnAmount;

        // // Rewards, kill target
        FPSBot killedBot = CanKill();
        if (shoot && killedBot != null)
        // if (CanKill())
        {
            if (this.team != killedBot.team)
            {
                killedBot.alive = false;
                // remove the killed bot collider
                killedBot.GetComponent<Collider>().enabled = false;
                // set invisible
                killedBot.GetComponent<Renderer>().enabled = false;
                env.KillBot(this.team);
            }
            // AddReward(0.1f);
            // target.EndEpisode();
            // EndEpisode();
        }

        // Rewards based on angle to target
        // float angle = Vector3.Angle(
        //     transform.forward,
        //     target.transform.localPosition - transform.localPosition
        // );
        // AddReward(0.1f - angle / 45f);

        // if (CanKill())
        // {
        //     // countAim++;
        //     AddReward(0.01f);
        //     target.AddReward(-0.01f);
        // }
        // else
        // {
        //     countAim = 0;
        // }

        // // Rewards, can kill and not shoot, cant kill and shoot
        // if ((canKill && !shoot) || (!canKill && shoot))
        // {
        //     AddReward(-0.05f);
        //     target.AddReward(0.05f);
        // }

        // if (AimWall())
        // {
        //     AddReward(-0.0001f);
        //     target.AddReward(0.0001f);
        // }

        // if (IsWallBetween())
        // {
        //     AddReward(-0.0005f);
        //     target.AddReward(0.0005f);
        // }
    }

    // private bool IsWallBetween()
    // {
    //     Vector3 direction = target.transform.localPosition - transform.localPosition;
    //     RaycastHit hit;
    //     if (Physics.Raycast(transform.position, direction, out hit))
    //     {
    //         if (hit.collider.tag == "Wall")
    //         {
    //             return true;
    //         }
    //     }
    //     return false;
    // }

    // CanKill, if Raycast hit target, return true
    private FPSBot CanKill()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, gunAimDirection, out hit))
        {
            if (hit.collider.TryGetComponent<FPSBot>(out FPSBot bot))
            {
                return bot;
            }
        }
        return null;
    }

    // CanKill, if Raycast hit target, return true
    private float DistanceFacing()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, gunAimDirection, out hit))
        {
            return hit.distance / maxDistance;
        }
        return maxDistance;
    }

    // CanKill, if Raycast hit target, return true
    // private float DistanceToTarget()
    // {
    //     Vector3 direction = target.transform.localPosition - transform.localPosition;
    //     return direction.magnitude;
    // }

    private bool AimWall()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, gunAimDirection, out hit))
        {
            if (hit.collider.tag == "Wall" && hit.distance < 2f)
            {
                return true;
            }
        }
        return false;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Actions, fordward, turn, vertical turn and shoot
        var actions = actionsOut.DiscreteActions;
        actions[0] = Input.GetButton("Fire1") ? 1 : 0;
        actions[1] = Input.GetAxis("Vertical") > 0 ? 1 : Input.GetAxis("Vertical") < 0 ? 2 : 0;
        actions[2] = Input.GetAxis("Horizontal") > 0 ? 1 : Input.GetAxis("Horizontal") < 0 ? 2 : 0;
        // actions[3] = Input.mousePosition.y / Screen.height * 2f - 1f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent local positions
        // sensor.AddObservation(transform.localPosition.x);
        // sensor.AddObservation(transform.localPosition.z);
        // speed = position - lastPosition;
        // Vector3 currentSpeed = (transform.localPosition - lastPosition) / Time.deltaTime;
        // sensor.AddObservation(currentSpeed.x);
        // sensor.AddObservation(currentSpeed.z);
        // lastPosition = transform.localPosition;

        // sensor.AddObservation(target.transform.localPosition.x);
        // sensor.AddObservation(target.transform.localPosition.z);
        // sensor.AddObservation(gunAimDirection.x);
        // sensor.AddObservation(gunAimDirection.z);

        // Add the raycast info (from current to target)
        sensor.AddObservation(CanKill());

        // Add the sensors info
        // sensor.AddObservation(GetDistanceSensors(SensorType.Wall, false));
        // sensor.AddObservation(GetDistanceSensors(SensorType.Enemy, false));
        // sensor.AddObservation(GetDistanceSensors(SensorType.Wall, true));
        // sensor.AddObservation(GetDistanceSensors(SensorType.Enemy, true));

        // sensor.AddObservation(AimWall());
        // sensor.AddObservation(IsWallBetween());
        // sensor.AddObservation(DistanceFacing());
        // sensor.AddObservation(DistanceToTarget());

        // float angle_to_target = Vector3.Angle(
        //     transform.forward,
        //     target.transform.localPosition - transform.localPosition
        // );
        // sensor.AddObservation(angle_to_target / 180f);

        // ammo between 0 and 1
        float ammoPercent = ((float)ammo) / ((float)maxAmmo);
        sensor.AddObservation(ammoPercent);

        sensor.AddObservation(alive);

        // // Add the grid to the sensor
        // for (int i = 0; i < grid.GetLength(0); i++)
        // {
        //     for (int j = 0; j < grid.GetLength(1); j++)
        //     {
        //         sensor.AddObservation(grid[i, j]);
        //     }
        // }
    }

    private float[] GetDistanceSensors(SensorType type, bool behind)
    {
        float[] sensors = new float[numSensors];
        for (int i = 0; i < numSensors; i++)
        {
            if (behind)
            {
                sensors[i] = GetDistanceSensorBehind(type, i);
            }
            else
            {
                sensors[i] = GetDistanceSensor(type, i);
            }
        }
        return sensors;
    }

    private float GetDistanceSensorBehind(SensorType type, int i)
    {
        float angle = i * angleSensor / (numSensors - 1) - angleSensor / 2f;
        Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * -transform.forward;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit))
        {
            if (type == SensorType.Wall && hit.collider.tag == "Wall")
            {
                return hit.distance / maxDistance;
            }
            else if (type == SensorType.Enemy &&
                hit.collider.TryGetComponent<FPSBot>(out FPSBot bot))
            {
                if (bot.team != team)
                {
                    return hit.distance / maxDistance;
                }
            }
        }
        return maxDistance;
    }

    // Method to return the distance of sensor i
    private float GetDistanceSensor(SensorType type, int i)
    {
        float angle = i * angleSensor / (numSensors - 1) - angleSensor / 2f;
        Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit))
        {
            if (type == SensorType.Wall && hit.collider.tag == "Wall")
            {
                return hit.distance / maxDistance;
            }
            else if (type == SensorType.Enemy &&
                hit.collider.TryGetComponent<FPSBot>(out FPSBot bot))
            {
                if (bot.team != team)
                {
                    return hit.distance / maxDistance;
                }
            }
        }
        return 1f;
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(
            Random.Range(-maxDistance / 2 + 2, maxDistance / 2 - 2),
            1f,
            Random.Range(-maxDistance / 2 + 2, maxDistance / 2 - 2)
        );
        ammo = maxAmmo;
        // enable the collider
        GetComponent<Collider>().enabled = true;
        alive = true;
        // set visible
        GetComponent<MeshRenderer>().enabled = true;
        penalty = 0f;
    }

    private void Update()
    {
        if (!alive) return;
        Debug.DrawRay(transform.position, gunAimDirection * 10f, Color.red);
        if (AimWall())
        {
            penalty += 0.000f;
        }
    }

    // Action Masking
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if (ammo <= 0)
        {
            actionMask.SetActionEnabled(0, 1, false);
        }
        else
        {
            actionMask.SetActionEnabled(0, 1, true);
        }

        // ahora mismo no se mueven
        // actionMask.SetActionEnabled(1, 1, false);
        // actionMask.SetActionEnabled(1, 2, false);
    }

    // on collision with a respawn
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Respawn")
        {
            alive = false;
            // remove the killed bot collider
            GetComponent<Collider>().enabled = false;
            // set invisible
            GetComponent<Renderer>().enabled = false;
            // Abs of (team - 1) to get the other team
            env.KillBot(Mathf.Abs(team - 1));
        }
    }
}
