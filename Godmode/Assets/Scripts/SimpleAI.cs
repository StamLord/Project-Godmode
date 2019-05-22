using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SimpleAI : MonoBehaviour
{
    public enum AtkState { MELEE, RANGED};
    public AtkState atkState;
    public StateMachine self;
    public TechManager techManager;

    public StateMachine enemy;
    public Vector3 currentDirection;

    public StateMachine[] allEnemiesArray;
    public List<StateMachine> allEnemies;

    public float lmbSmashRate = 5;
    protected float lastLMB = 0f;

    // Start is called before the first frame update
    void Start()
    {
        self = GetComponent<StateMachine>();
        techManager = GetComponent<TechManager>();
        allEnemiesArray = FindObjectsOfType<StateMachine>();
        allEnemies = allEnemiesArray.OfType<StateMachine>().ToList();
        allEnemies.Remove(self);
    }

    // Update is called once per frame
    void Update()
    {
        if(enemy == null)
        {
            enemy = FindeNearestEnemy();
            return;
        }
        
        switch (atkState)
        {
            case AtkState.MELEE:
                MeleeStrategy();
                break;

            case AtkState.RANGED:
                RangedStrategy();
                break;
        }
    }

    StateMachine FindeNearestEnemy()
    {
        StateMachine nearest = null;
        float distance = Mathf.Infinity;

        foreach (StateMachine s in allEnemies)
        {
            float dist = Vector3.Distance(transform.position, s.transform.position);
            if(nearest == null || dist < distance)
            {
                nearest = s;
                distance = dist;
            }
        }

        return nearest;
    }

    void MeleeStrategy()
    {
        RotateToEnemy();
        Vector3 flatSelf = transform.position;
        Vector3 flatEnemy = enemy.transform.position;
        flatSelf.y = flatEnemy.y = 0f;

        if(Vector3.Distance(flatSelf, flatEnemy) > 1.5f)
        {
            self.vi.vertical = 1f;
        }
        else
        {
            self.vi.vertical = 0f;
            SimulateLMB();
        }

        ResetLMB();
    }

    void RangedStrategy()
    {

    }

    void RotateToEnemy()
    {
        Vector3 flatEnemy = enemy.transform.position;
        Vector3 flatSelf = transform.position;
        flatSelf.y = flatEnemy.y =0;

        currentDirection = (flatEnemy - flatSelf).normalized;
        transform.forward = currentDirection;
    }

    void SimulateLMB()
    {
        if (Time.time - lastLMB >= 1f / lmbSmashRate )
        {
            lastLMB = Time.time;
            self.vi.lmbDown = true;
            self.vi.lmb = true;
        }
    }

    void ResetLMB()
    {
        if (self.vi.lmbDown)
            self.vi.lmbDown = false;

        if (self.vi.lmb && Time.time - lastLMB > 0.1f)
        {
            self.vi.lmb = false;
        }
    }

}
