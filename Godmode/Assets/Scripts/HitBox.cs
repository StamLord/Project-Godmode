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
    public bool tossUp = false;
    public bool tossForward = false;
    public bool tossDown = false;


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

            if (t.chargable && techManager.lastTechChargeTimer > t.minChageTime)
            {
                
                float force = techManager.lastTechChargeTimer / t.fullChargeTime * t.fullChargeBlowBackForce;
                int chargeDamage = Mathf.FloorToInt(techManager.lastTechChargeTimer / t.fullChargeTime * t.fullChargeDamge);

                if (tossUp)
                    c.EnterToss(Vector3.up * force);
                else if (tossDown)
                    c.EnterToss(-Vector3.up * force);
                else if(tossForward)
                    c.EnterToss(owner.transform.forward * force);

                Debug.Log(owner + ":: Tossed " + owner.ts.lockOn + "with force " + force);
                owner.ts.hardLock = true;

                PerformHit(c, t.damage, t.stunTime, t.juggle, t.pushBack);

                techManager.lastTechChargeTimer = 0f;
                ShockwaveManager.instance.Create(transform.position);
            }
            else
            {
                PerformHit(c, t.damage, t.stunTime, t.juggle, t.pushBack);
            }

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

    void PerformHit(StateMachine c, int damage, float stunTime, bool juggle, float pushback)
    {
        bool success = c.Hit(damage, owner, stunTime, juggle, pushback, transform.position);

        if (success)
        {
            owner.hits++;
            owner.tempDamage += damage;
            particles.Play();
            Debug.Log(owner + ":: Hit: " + c.gameObject.name + "With: " + gameObject.name + "Damage: " + damage + "Stun: " + stunTime);
        }
    }
}
