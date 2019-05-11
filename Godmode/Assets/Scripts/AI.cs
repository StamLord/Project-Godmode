using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : CharController
{
    public enum AiState { Search, Fight, Flight }
    public enum AiAction { Charge, Flee, Attack, Roam }
    Dictionary<AiAction, float> actionWeights = new Dictionary<AiAction, float> {
        {AiAction.Charge,0f}, {AiAction.Flee, 0f}, {AiAction.Attack, 0f}, {AiAction.Roam, 0f}};

    [Header("Debug")]
    public float chargeWeight;
    public float fleeWeight;
    public float attackWeight;
    public float roamWeight;

    [Header("AI")]
    public bool aiActive = true;
    public AiState state;
    public AiAction action;

    public Technique chosenTech;
    float lastAttack;
    float startedCharging;
    float randChargeTime;

    public float scanSpeed = 0.5f;
    public List<CharController> enemiesVisible = new List<CharController>();

    void Update()
    {
        if (aiActive)
        {
            SimpleThink();
            StateMachine();
        }

        UpdateFunctions();
    }

    void SimpleThink()
    {
        float healthP = (float)health / (float)maxHealth;
        float energyP = (float)energy / (float)maxEnergy;

        if(tarSys.lockOn == null)
        {
            //tarSys.LockOn(FindNearestFoe().transform, true);
        }

        float distance = -1f;
        if (tarSys.lockOn)
            distance = Vector3.Distance(transform.position, tarSys.lockOn.position);

        if (healthP < 0.3f)
        {
            action = AiAction.Flee;
        }
        else if(energyP < 0.3f || energyP < 0.7f && distance > 50f || action == AiAction.Charge && Time.time - startedCharging < 5f)
        {
            if(action != AiAction.Charge)
                startedCharging = Time.time;
            action = AiAction.Charge;
        }
        else if(tarSys.lockOn)
        {
            action = AiAction.Attack;
        }
        else
        {
            action = AiAction.Roam;
        }
    }

    void Think()
    {
        chargeWeight = actionWeights[AiAction.Charge] = GetWeight(AiAction.Charge);
        fleeWeight = actionWeights[AiAction.Flee] = GetWeight(AiAction.Flee);
        attackWeight = actionWeights[AiAction.Attack] = GetWeight(AiAction.Attack);
        roamWeight = actionWeights[AiAction.Roam] = GetWeight(AiAction.Roam);

        AiAction heaviestAction = AiAction.Roam;
        float weight = -Mathf.Infinity;

        foreach (KeyValuePair<AiAction, float> pair in actionWeights)
        {

            if (pair.Value >= weight)
            {
                heaviestAction = pair.Key;
                weight = pair.Value;
            }
        }

        if (action != AiAction.Charge && heaviestAction == AiAction.Charge)
        {
            startedCharging = Time.time;
        }

        action = heaviestAction;
    }

    void StateMachine()
    {
        switch (action)
        {
            case AiAction.Charge:
                vi.e = true;
                break;
            case AiAction.Flee:
                vi.e = false;
                FleeState();
                break;
            case AiAction.Attack:
                vi.e = false;
                SimpleAttack();
                break;
            case AiAction.Roam:
                vi.e = false;
                break;
        }
    }

    void AttackState()
    {
        RotateToTarget();
        chosenTech = ChooseAttack();

        float distance = Vector3.Distance(transform.position, tarSys.lockOn.position);

        if (chosenTech.type == HitType.Melee)
        {
            if (distance > fightingRadius)
            {
                vi.vertical = 1f;
            }

            if (distance > 20 && !inDash)
            {
                vi.w = true;
                EnterDash();
            }
            else if (distance < fightingRadius && inDash)
            {
                vi.w = false;
                vi.vertical = 0f;
            }

            if (distance <= fightingRadius)
            {
                UseTechnique(chosenTech);
                lastAttack = Time.time;
            }
        }
        else if (chosenTech.type == HitType.Projectile)
        {
            if (distance > 50)
            {
                vi.vertical = 1f;
                if (!inDash)
                {
                    vi.w = true;
                    EnterDash();
                }
            }
            else
            {
                if (Time.time - lastAttack > 1f)
                {
                    UseTechnique(chosenTech);
                    lastAttack = Time.time;
                }

                if (inDash)
                {
                    vi.w = false;
                    vi.vertical = 0f;
                }
            }
        }
    }

    Technique ChooseAttack()
    {
        Technique tech = null;
        float weight = Mathf.Infinity;
        float distance = Vector3.Distance(transform.position, tarSys.lockOn.position);

        foreach (Technique t in techniques)
        {
            float w = (energy - t.energyCost);
            w += (health - t.healthCost);
            w *= t.damage;

            if(t.type == HitType.Melee)
            {
                w *= distance * distance;
            }

            if (!tech || w < weight)
            {
                tech = t;
                weight = w;
            }
        }
        return tech;
    }

    void FleeState()
    {
        RotateToTarget();
        transform.forward *= -1f;

        float distance = Vector3.Distance(transform.position, tarSys.lockOn.position);

        if(distance < 100)
        {
            vi.vertical = 1f;

            if (distance <= 50f)
            {
                EnterDash();
                vi.w = true;
            }
        }
        else
        {
            vi.vertical = 0f;
            if(inDash)
            {
                vi.w = false;
            }
        }

        
    }

    float GetWeight(AiAction action)
    {
        float weight = 0f;
        CharController c;

        switch(action)
        {
            case AiAction.Charge:
                float missingEnergy = maxEnergy - energy;
                weight = missingEnergy / maxEnergy;
                weight *= 1.5f;
                if(action == AiAction.Charge && Time.time - startedCharging < 2f)
                {
                    weight *= 1000f;
                }
                else if(action == AiAction.Charge && energy/maxEnergy < 0.7f)
                {
                    weight *= 1000f;
                }
                break;
            case AiAction.Flee:
                //c = FindNearestFoe();
                float missingHealth = maxHealth - health;
                weight = missingHealth / maxHealth -0.6f; //I want flee to no be an option until 60% of Health is missing

                //if (c)
                //{
                //    float healthDifference = c.health - health;
                //    float distance = Vector3.Distance(c.transform.position, transform.position);
                //    weight += (healthDifference / 500f) / (distance * distance);
                //}
                //else
                //    weight = 0f;

                break;
            case AiAction.Attack:
                //c = FindNearestFoe();
                weight = (float)health / (float)maxHealth + 0.6f; //Attack is an option above 60% Health
                //if (c)
                //{
                //    float healthDifference = health - c.health;
                //    float distance = Vector3.Distance(c.transform.position, transform.position);

                //    weight += (healthDifference / 500f) / (distance * distance);

                //    if(tarSys.lockOn != c.transform) tarSys.LockOn(c.transform, true);
                //}
                //else
                //    weight = 0f;
                break;
            case AiAction.Roam:
                //c = FindNearestFoe();
                //if (!c)
                //    weight = .001f;
                break;
        }

        return weight;
    }

    CharController ChooseEnemy(CharController[] enemiesVisible)
    {
        CharController closestEnemy = null;

        foreach (CharController cr in enemiesVisible)
        {
            Vector3 dir = cr.transform.position - transform.position;
            RaycastHit hit;

            Physics.Raycast(transform.position, dir, out hit, Mathf.Infinity, tarSys.scanLayerMask);
            Debug.Log(hit.transform);
            Debug.DrawRay(transform.position, dir, Color.blue);

            if (hit.transform == cr.transform)
            {
                if (!closestEnemy || Vector3.Distance(transform.position, cr.transform.position) < Vector3.Distance(transform.position, closestEnemy.transform.position))
                {
                    closestEnemy = cr;
                }
            }
        }

        return closestEnemy;
    }

    void RotateToTarget()
    {
        Vector3 toPlayer = (transform.position - tarSys.lockOn.transform.position);

        float angle = Mathf.Atan2(toPlayer.z, toPlayer.x); //Rads
        float zLength = Mathf.Sin(angle) * tarSys.bodyCenter.radius;
        float xLength = Mathf.Cos(angle) * tarSys.bodyCenter.radius;
        Vector3 offset = new Vector3(xLength, 0, zLength);

        //Debug.Log( offset);

        tarSys.bodyCenter.transform.position = tarSys.lockOn.transform.position + offset;

        if (Vector3.Distance(transform.position, tarSys.lockOn.transform.position) <= tarSys.bodyCenter.radius + .1f)
            tarSys.transform.LookAt(tarSys.lockOn);
        else
            transform.LookAt(tarSys.bodyCenter.transform);

        if(state == AiState.Flight)
            transform.forward = -transform.forward;
    }

    void MoveInput()
    {
        float distance = Vector3.Distance(transform.position, tarSys.lockOn.position);

        switch(state)
        {
            case AiState.Fight:
                if (distance > fightingRadius)
                {
                    vi.vertical = 1;
                    if (distance > 10f)
                    {
                        if (!inDash)
                            EnterDash();
                    }
                }
                else
                {
                    vi.vertical = 0;
                    if (distance < fightingRadius / 2)
                        if (inDash)
                            ExitDash();
                }
                break;
            case AiState.Flight:
                vi.vertical = 1;
                if (distance < 30f)
                {
                    if (!inDash)
                        EnterDash();
                }
                else if(distance > 300f)
                {
                    vi.vertical = 0;
                    if (inDash)
                        ExitDash();
                }
                break;
        }
        

    }

    //CharController FindNearestFoe()
    //{
    //    StateMachine cr = null;
    //    float distance = Mathf.Infinity;

    //    foreach(StateMachine c in tarSys.allTargets)
    //    {
    //        if (c == this)
    //            continue;

    //        float d = Vector3.Distance(transform.position, c.transform.position);
    //        if (!cr || d < distance)
    //        {
    //            cr = c;
    //            distance = d;
    //        }
    //    }

    //    return cr;
    //}

    void SimpleAttack()
    {
        RotateToTarget();

        if(Time.time - lastAttack < 2f)
        {
            return;
        }

        //Find usable techniques
        List<Technique> usable = new List<Technique>();

        foreach (Technique t in techniques)
        {
            if(energy >= t.energyCost && health > t.healthCost)
            {
                usable.Add(t);
            }
        }

        //Find best suited Attack
        float distance = Vector3.Distance(transform.position, tarSys.lockOn.position);
        Technique bestTechnique = null;
        float weight = Mathf.Infinity;

        foreach(Technique t in usable)
        {
            float w = energy - t.energyCost;
            w += health - t.healthCost;
            w += t.damage;
            if (t.type == HitType.Melee)
            {
                if (distance >= 50f)
                    w -= 100f;
                else if (distance <= 30f)
                    w += 100f;
            }

            if(bestTechnique == null || w < weight)
            {
                bestTechnique = t;
                weight = w;
            }
        }
        Debug.Log(gameObject + ":: Using: " + bestTechnique.name);

        chosenTech = bestTechnique;

        //Move depending on best attack
        if (chosenTech.type == HitType.Melee)
        {
            if (distance > fightingRadius)
            {
                vi.vertical = 1f;
            }

            if (distance > 20 && !inDash)
            {
                vi.w = true;
                EnterDash();
            }
            else if (distance < fightingRadius && inDash)
            {
                vi.w = false;
                vi.vertical = 0f;
            }

            if (distance <= fightingRadius)
            {
                UseTechnique(chosenTech);
                lastAttack = Time.time;
            }
        }
        else if (chosenTech.type == HitType.Projectile)
        {
            if (distance > 50)
            {
                vi.vertical = 1f;
                if (!inDash)
                {
                    vi.w = true;
                    EnterDash();
                }
            }
            else
            {
                if (Time.time - lastAttack > 1f)
                {
                    UseTechnique(chosenTech);
                    lastAttack = Time.time;
                }

                if (inDash)
                {
                    vi.w = false;
                    vi.vertical = 0f;
                }
            }
        }

        UseTechnique(bestTechnique);
        lastAttack = Time.time;
    }
    
}
