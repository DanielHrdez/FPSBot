using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSEnv : MonoBehaviour
{
    public FPSBot[] botsTeam0;
    public FPSBot[] botsTeam1;

    private int killsTeam0 = 0;
    private int killsTeam1 = 0;

    private bool team0WithoutAmmo = false;
    private bool team1WithoutAmmo = false;

    public float penalty = 0f;

    public int maxTimeSeconds = 10;
    private float time = 0f;

    private void AddTeamReward(FPSBot[] team, float reward, bool set)
    {
        for (int i = 0; i < team.Length; i++)
        {
            if (set)
            {
                team[i].SetReward(reward - penalty - team[i].penalty);
            }
            else
            {
                team[i].AddReward(reward);
            }
        }
    }

    private void RewardCondition(bool condition, float reward, System.Action action = null, bool end = false)
    {
        if (condition)
        {
            AddTeamReward(botsTeam0, reward, end);
            AddTeamReward(botsTeam1, -reward, end);
            if (action != null)
            {
                action();
            }
        }
    }

    public void KillBot(int team)
    {
        RewardCondition(team == 0, 0.0f, () => {
            killsTeam0++;
        }, false);
        RewardCondition(team == 1, -0.0f, () => {
            killsTeam1++;
        }, false);
    }

    // Update is called once per frame
    void Update()
    {
        RewardCondition(killsTeam0 >= botsTeam1.Length, 1f, () => {
            EndEpisode();
        }, true);
        RewardCondition(killsTeam1 >= botsTeam0.Length, -1f, () => {
            EndEpisode();
        }, true);

        // AddTeamReward(botsTeam0, -0.003f, false);
        // AddTeamReward(botsTeam1, -0.003f, false);
        
        // time -= Time.deltaTime;
        // if (time <= 0)
        // {
        //     EndEpisode(true);
        //     time = maxTimeSeconds;
        // }
    }

    private void EndEpisode(bool draw = false)
    {
        if (draw) {
            AddTeamReward(botsTeam0, 0f, true);
            AddTeamReward(botsTeam1, 0f, true);
        }
        for (int i = 0; i < botsTeam0.Length; i++)
        {
            botsTeam0[i].EndEpisode();
        }
        for (int i = 0; i < botsTeam1.Length; i++)
        {
            botsTeam1[i].EndEpisode();
        }
        killsTeam0 = 0;
        killsTeam1 = 0;
        team0WithoutAmmo = false;
        team1WithoutAmmo = false;
    }

    public void WithoutAmmo(int team)
    {
        RewardCondition(team == 0, -0.00f, null, false);
        RewardCondition(team == 1, 0.00f, null, false);
        if (team == 0)
        {
            team0WithoutAmmo = true;
        }
        else
        {
            team1WithoutAmmo = true;
        }
        if (team0WithoutAmmo && team1WithoutAmmo)
        {
            for (int i = 0; i < botsTeam0.Length; i++)
            {
                botsTeam0[i].ResetAmmo();
            }
            for (int i = 0; i < botsTeam1.Length; i++)
            {
                botsTeam1[i].ResetAmmo();
            }
            team0WithoutAmmo = false;
            team1WithoutAmmo = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        time = maxTimeSeconds;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        penalty += 0.000f;
    }
}