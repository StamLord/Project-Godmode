using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public bool juggle;
    public bool destroyDestructables;
    [SerializeField]
    private StateMachine _owner;
    [SerializeField]
    private int _damage;
    [SerializeField]
    private float _range;
    [SerializeField]
    private float _force;
    [SerializeField]
    private List<StateMachine> hits = new List<StateMachine>();

    void Start()
    {
        Explode();    
    }

    void Explode()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, _range);

        Rigidbody rb = null;
        Destructable de = null;
        StateMachine cr = null;
        Vector3 dir = Vector3.zero;
        float ratioByRange;
        float distance;

        foreach(Collider c in cols)
        {
            rb = c.GetComponent<Rigidbody>();
            if(destroyDestructables)
                de = c.GetComponent<Destructable>();
            if (c.CompareTag("Player"))
                cr = c.GetComponent<StateMachine>();

            if(cr && _damage != 0)
            {
                if (!hits.Contains(cr))
                {
                    distance = Vector3.Distance(transform.position, c.transform.position);
                    ratioByRange = (_range - distance) / _range;

                    int newDamage = Mathf.RoundToInt(_damage * ratioByRange);
                    if (cr.Hit(newDamage, _owner, 0, MoveAttribute.Juggle, _force, transform.position))
                    {
                        _owner.hits++;
                        _owner.tempDamage += newDamage;
                    }

                    //Debug.Log(cr + "is hit for: " + newDamage);
                    hits.Add(cr);
                }
            }

            else if(rb)
            {
                distance = Vector3.Distance(transform.position, c.transform.position);
                ratioByRange = (_range - distance) / _range;
                dir = c.transform.position - transform.position;
                dir.Normalize();

                rb.AddForce(dir * _force * ratioByRange, ForceMode.Impulse);
            }
            else if (de)
            {
                distance = Vector3.Distance(transform.position, c.transform.position);
                ratioByRange = (_range - distance) / _range;
                dir = c.transform.position - transform.position;
                dir.Normalize();

                de.Destruction(dir, _force * ratioByRange);
            }
        }
    }
   
    public void Init(int damage)
    {
        _damage = damage;
    }

    public void Init(int damage, List<StateMachine> hitByProjectile, StateMachine owner)
    {
        _damage = damage;
        foreach (StateMachine h in hitByProjectile)
        {
            hits.Add(h);
        }
        _owner = owner;
    }
}
