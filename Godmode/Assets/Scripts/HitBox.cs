using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public StateMachine owner;
    public TechManager techManager;
    public List<StateMachine> hits = new List<StateMachine>();
    public ParticleSystem particles;
    
    Collider col;

    [Header("State")]
    public bool grab = false;

    void Start()
    {
        owner = transform.root.GetComponent<StateMachine>();
        techManager = transform.root.GetComponent<TechManager>();
        col = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        StateMachine c = other.transform.root.GetComponent<StateMachine>();
        Move t = owner.techManager.currentMove;

        //Ignore own character
        if (c && c != owner)
        {
            //Ignore targets already hit by this collider
            foreach (StateMachine cr in hits)
            {
                if (c == cr)
                    return;
            }

            PerformHit(c, t.damage, t.stunTime, t.attribute, t.pushBack);

            hits.Add(c);

            if (grab)
            {
                //c.Grabbed(owner);
                owner.anim.SetBool("Grab", false);
                owner.anim.SetBool("Grabbing", true);
            }
        }
    }
    
    void LateUpdate()
    {
        if (col.enabled == false && hits.Count > 0)
            hits.Clear();
    }

    void PerformHit(StateMachine c, int damage, float stunTime, MoveAttribute attribute, float pushback)
    {
        bool success = c.Hit(damage, owner, stunTime, attribute, pushback, transform.position);

        if (success)
        {
            owner.hits++;
            owner.tempDamage += damage;
            particles.Play();
            Debug.Log(owner + ":: Hit: " + c.gameObject + "With: " + gameObject + "for damage: " + damage);
        }
    }
}
