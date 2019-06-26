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
    public int maxBounces;

    public bool dieOnExplosion = true;
    public bool explodeOnTerrain = true;
    public bool explodeOnProjectile = true;
    public bool absorbProjectiles;
    public bool ignoreSiblingProjectiles;
    public bool ignoreOwner;
    public bool destroyOnDestructable;
    public float timeBetweenExplosions = .5f;
    public bool hitMoreThanOnce;

    [Header("Slow Down")]
    public float minSlowdownTime;
    public float maxSlowdownTime;

    [Header("X Axis over Time")]
    public AnimationCurve xCurve;

    #region Private

    [Header("Timer")]
    [SerializeField] private float timer;

    private int damage;
    [SerializeField]private float speed;
    private float blowBack;

    private float lastExplosion;
    private Vector3 dir;
    private float originalYRot;
    private bool _stopped;
    private int bounce;
    private float _slowdownTime;
    
    #endregion

    private void Start()
    {
        originalYRot = transform.rotation.eulerAngles.y;
        dir = dir.normalized;
        _slowdownTime = Random.Range(minSlowdownTime, maxSlowdownTime);
    }

    /// <summary>
    /// Sets necessary values for a projectlie. Should be called from TechManager when instantiating new projectile.
    /// </summary>
    /// <param name="blowBack">The amount it will push a character back when on hit</param>
    public void Initialize(StateMachine owner, Transform target, int damage, float speed, float blowBack)
    {
        this.owner = owner;
        this.target = target;
        this.damage = damage;
        this.speed = speed;
        this.blowBack = blowBack;
    }

    void Update()
    {
        if (timer <= followDuration && target != null)
        {
            transform.LookAt(target);
            dir = (target.position - transform.position).normalized;
        }
        else
        {
            dir = transform.forward;
        }

        //Curve
        if (xCurve.length > 0)
        {
            float x = xCurve.Evaluate(timer);
            Vector3 angles = transform.rotation.eulerAngles;
            angles.y = originalYRot + x;
            transform.rotation = Quaternion.Euler(angles);
        }

        //Move
        if(!_stopped)
            transform.position += dir * speed * Time.deltaTime;

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

        timer += Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Projectiles
        Projectile p = other.GetComponent<Projectile>();
        if (p)
        {
            if (p.owner == this.owner && ignoreSiblingProjectiles)
                return;

            if (absorbProjectiles)
            {
                if (p.damage < damage)
                {
                    damage += p.damage;
                    return;
                }
            }

            if (explodeOnProjectile)
            {
                Explode();
                return;
            }
            else
                return;
        }
        
        //Characters
        StateMachine c = other.transform.root.GetComponent<StateMachine>();
        if (c)
        {
            //Ignore self for the first second and anybody that was already hit
            if (c == owner && timer < 1f || hits.Contains(c) && !hitMoreThanOnce || c == owner && ignoreOwner)
                return;

            if (c.Hit(damage, owner, transform.position))
            {
                Debug.Log("Hit::" + c + "Owner::" + owner);
                if (owner)
                {
                    owner.AddToCombo(1, damage);
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

        //Rigidbodies
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.AddForce(transform.forward * blowBack, ForceMode.Impulse );
        }

        if (other.gameObject.layer == 10) return;

        //Destructable
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
        
        //Terrain
        if(explodeOnTerrain)
        {
            Explode();
        }
        else
        {
            _stopped = true;
        }

    }

    private void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
       
    }

    void Explode()
    {
        if (Time.time - lastExplosion < timeBetweenExplosions)
            return;

        if (explosion)
        {
            GameObject go = Instantiate(explosion, transform.position, Quaternion.identity) as GameObject;
            go.GetComponent<Explosion>().Init(damage, hits, owner);

            if (dieOnExplosion)
            {
                Destroy(go, 5f);
                Destroy(this.gameObject);
            }
            else
            {
                lastExplosion = Time.time;
            }
        }
    }
}
