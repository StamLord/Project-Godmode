using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject explosion;

    [Header("Info")]
    public Transform target;
    public StateMachine owner;
    public List<StateMachine> hits = new List<StateMachine>();

    [Header("Behaviour")]
    public float lifeTime;
    public float followDuration;
    public bool continousBeam;
    private bool beamHit;
    private float beamHitTimer;
        
    public bool explodeOnTerrain = true;
    public int maxBounces;
    protected int bounce;
    public bool absorbProjectiles;
    public bool destroyOnDestructable;

    [Header("Slow Down")]
    public float minSlowdownTime;
    public float maxSlowdownTime;
    protected float _slowdownTime;

    float followTimer;
    [HideInInspector] public int damage;
    [HideInInspector] public float speed;
    [HideInInspector] public float blowBack;
    public Vector3 dir;
    public float originalYRot;
    public Vector3 lastDir;

    public AnimationCurve xCurve;
    public float timer;

    protected bool _stopped;

    private void Start()
    {
        originalYRot = transform.rotation.eulerAngles.y;
        dir = dir.normalized;
        _slowdownTime = Random.Range(minSlowdownTime, maxSlowdownTime);
    }

    void Update()
    {
        if (followTimer <= followDuration && target != null)
        {
            transform.LookAt(target);
            dir = (target.position - transform.position).normalized;
        }
        else if(!beamHit)
        {
            dir = transform.forward;
        }

        //Curve
        float x = xCurve.Evaluate(timer);
        Vector3 angles = transform.rotation.eulerAngles;
        angles.y = originalYRot + x;
        transform.rotation = Quaternion.Euler(angles);

        //Move
        if(!_stopped)
            transform.position += dir * speed * Time.deltaTime;

        followTimer += Time.deltaTime;
        timer += Time.deltaTime;

        if(_slowdownTime > 0)
        {
            if(timer >= _slowdownTime)
            {
                speed = Mathf.Lerp(speed, 0, 0.1f);
            }
        }

        if(lifeTime > 0)
        {
            if(timer>=lifeTime)
            {
                Destroy(this.gameObject);
            }
        }

        //RaycastHit hit;
        //Physics.Raycast(transform.position, dir, out hit, 50f);

        //if (hit.collider  && bounce < maxBounces)
        //{
        //    Debug.Log("Hit ray");
        //    dir = -dir;
        //    bounce++;
        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        StateMachine c = other.transform.root.GetComponent<StateMachine>();

        if (c)
        {
            if (c == owner && timer < 1f || hits.Contains(c))
                return;

            if (c.Hit(damage, owner, transform.position))
            {
                if (owner)
                {
                    owner.hits++;
                    owner.tempDamage += damage;
                }
                hits.Add(c);
                Explode();
                return;

            }
            else
            {
                target = null;
                return;
            }
        }

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.AddForce(transform.forward * blowBack, ForceMode.Impulse );
        }

        if (other.gameObject.layer == 10) return;

        Destructable d = other.GetComponent<Destructable>();
        if (d && explodeOnTerrain)
        {
            d.Destruction(transform.forward, blowBack);

            if (destroyOnDestructable)
            {
                Explode();
                return;
            }
        }

        Projectile p = other.GetComponent<Projectile>();
        if(p && absorbProjectiles)
        {
            if (p.damage < damage)
            {
                damage += p.damage;
                return;
            }
        }

        if(explodeOnTerrain)
        {
            Explode();
        }
        else
        {
            _stopped = true;
        }

        if (continousBeam)
        {
            lastDir = dir;
            dir = Vector3.zero;
            beamHit = true;
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (beamHit)
        {
            beamHitTimer += Time.deltaTime;
            if (beamHitTimer >= 0.5f)
            {
                Explode();
                beamHitTimer = 0f;
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (beamHit)
        {
            dir = lastDir;
            beamHit = false;
        }
    }

    void Explode()
    {
        if (explosion)
        {
            GameObject go = Instantiate(explosion, transform.position, Quaternion.identity) as GameObject;
            //go.transform.localScale = transform.localScale;
            go.GetComponent<Explosion>().Init(damage, hits, owner);

            Destroy(go, 5f);
            Destroy(this.gameObject);
        }
    }
}
