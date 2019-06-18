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
    public bool explodeOnTerrain = true;
    public bool absorbProjectiles;
    public bool destroyOnDestructable;

    [Header("Slow Down")]
    public float minSlowdownTime;
    public float maxSlowdownTime;

    [Header("X Axis over Time")]
    public AnimationCurve xCurve;

    #region Private

    [Header("Timer")]
    [SerializeField] private float timer;

    private int damage;
    private float speed;
    private float blowBack;

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
        float x = xCurve.Evaluate(timer);
        Vector3 angles = transform.rotation.eulerAngles;
        angles.y = originalYRot + x;
        transform.rotation = Quaternion.Euler(angles);

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
        StateMachine c = other.transform.root.GetComponent<StateMachine>();

        if (c)
        {
            if (c == owner && timer < 1f || hits.Contains(c))
                return;

            if (c.Hit(damage, owner, transform.position))
            {
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

    }

    private void OnTriggerStay(Collider other)
    {

    }

    private void OnTriggerExit(Collider other)
    {
       
    }

    void Explode()
    {
        if (explosion)
        {
            GameObject go = Instantiate(explosion, transform.position, Quaternion.identity) as GameObject;
            go.GetComponent<Explosion>().Init(damage, hits, owner);

            Destroy(go, 5f);
            Destroy(this.gameObject);
        }
    }
}
