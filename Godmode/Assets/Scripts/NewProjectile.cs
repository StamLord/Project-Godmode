using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class NewProjectile : MonoBehaviour
{
    //Prefabs
    public GameObject explosionPrefab;

    //Settings
    public bool dieOnExplosion = true;
    public bool continuousHits;
    public bool destroyDestructables;
    public float rotationOverLifetime;
    public float followTargetDuration;
    public bool canBeAbsorbed;

    public bool slowDown;
    public float minRandomSlowdown;
    public float maxRandomSlowdown;

    

    #region Terrain Collision

    public TerrainBehavior terrainBehavior;
    
    //Explode
    public ExplosionPoint explosionPointTerrain;
    public Vector3 explosionOffsetTerrain;
    public float timeBetweenExplosionTerrain;

    //Stop
    public float lifetime;
    public bool explodeOnLifeEnd;

    //Roll
    public float speedMultOnRoll;
    public float rollDuration;
    public AnimationCurve sizeOverRoll;
    public bool explodeOnRollEnd;

    //Deflect
    public float minimumAngle;
    public float speedMultOnDeflect;
    public float sizeMultOnDeflect;
    public float maxBounces;
    public bool explodeOnFailBounce;

    //Penetrate
    public float speedMultOnPenetrate;

    #endregion

    #region Character Collision 

    public CharacterBehavior characterBehvior;

    [HideInInspector] public bool ignoreOwner;

    //Explode
    public ExplosionPoint explosionPointCharacter;
    public Vector3 explosionOffsetCharacter;
    public float timeBetweenHitsCharacter;

    #endregion

    #region Projectile Collision

    public ProjectileBehavior projectileBehavior;

    public bool ignoreFromOwner;

    //Explode
    public ExplosionPoint explosionPointProjectile;
    public Vector3 explosionOffsetProjectile;
    public float timeBetweenExplosionProjectile;

    //Absorb
    public bool addToDamage;
    public bool addToSize;

    #endregion

    //Getting from outside via Initialize()
    private StateMachine _owner;
    private Transform _target;
    private float _speed;
    private int _damage;
    private float _pushBack;

    //Generating on own at Runtime
    private Vector3 _direction;
    private float _slowDownTime;

    //All targets that were hit and a timestamp
    private Dictionary<StateMachine, float> hits = new Dictionary<StateMachine, float>();

    private float _lastExplosionTerrain = -1f;
    private float _lastExplosionProjectile = -1f;

    private bool _startedPenetration; //Lewd
    private bool _startedRoll;

    private float _timer;

    public StateMachine GetOwner { get { return this._owner; } }
    public int GetDamage { get { return this._damage; } }

    void Start()
    {
        if(slowDown)
            _slowDownTime = Random.Range(minRandomSlowdown, maxRandomSlowdown);
    }

    public void Initialize(StateMachine owner, Transform target, float speed, int damage, float pushBack)
    {
        _owner = owner;
        _target = target;
        _speed = speed;
        _damage = damage;
        _pushBack = pushBack;
    }

    void GenerateDirection()
    {
        if (_target == null)
            _direction = transform.forward;
        else if (_timer < followTargetDuration)
            _direction = (_target.transform.position - transform.position).normalized;
    }

    void Update()
    {
        Rotate();
        GenerateDirection();
        Move();
        Slowdown();
        RemoveHits();
        

        _timer += Time.deltaTime;
    }

    void Rotate()
    {
        Quaternion rot = transform.rotation;
        rot.eulerAngles += new Vector3(0, 0, rotationOverLifetime) * Time.deltaTime;
        transform.rotation = rot;
    }

    void Move()
    {
        transform.position += _direction * _speed * Time.deltaTime;
    }

    void Slowdown()
    {
        if (slowDown == false)
            return;

        if(_timer >= _slowDownTime)
        {
            _speed = Mathf.Lerp(_speed, 0, 0.1f);
        }
    }

    void RemoveHits()
    {
        if (continuousHits == false)
            return;

        List<StateMachine> toRemove = new List<StateMachine>();

        foreach (KeyValuePair<StateMachine, float> pair in hits)
        {
            if (Time.time - pair.Value >= timeBetweenHitsCharacter)
            {
                toRemove.Add(pair.Key);
            }
        }

        foreach (StateMachine s in toRemove)
        {
            hits.Remove(s);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        StateMachine character = other.GetComponent<StateMachine>();
        NewProjectile proj = other.GetComponent<NewProjectile>();
        Destructable destructable = other.GetComponent<Destructable>();

        if(character)
        {
            if (character == _owner && ignoreOwner || hits.ContainsKey(character))
                return;

                if(character.Hit(_damage, _owner, transform.position))
                {
                    Debug.Log("Hit::" + character + "Owner::" + _owner);
                    if (_owner)
                    {
                        _owner.AddToCombo(1, _damage);
                    }
                    AddHit(character);
                }

                switch (characterBehvior)
                {
                    case CharacterBehavior.Explode:

                    if (explosionPointCharacter == ExplosionPoint.Self)
                        Explode(transform.position);
                    else
                        Explode(character.transform.position);

                        break;
                    case CharacterBehavior.Consume:
                        break;
                    case CharacterBehavior.Kill:
                        break;
                }

            return;
        }

        if(proj)
        {
            if (proj.GetOwner == _owner && ignoreFromOwner)
            {
                return;
            }

            switch (projectileBehavior)
            {
                case ProjectileBehavior.Explode:

                    if (Time.time - _lastExplosionProjectile >= timeBetweenExplosionProjectile)
                    {
                        if (explosionPointProjectile == ExplosionPoint.Self)
                            Explode(transform.position);
                        else
                            Explode(proj.transform.position);

                        _lastExplosionProjectile = Time.time;
                    }
                    break;
                case ProjectileBehavior.Absorb:
                    if(proj.canBeAbsorbed && proj.GetDamage < _damage)
                    {
                        _damage += proj.GetDamage;
                        proj.Kill();
                    }
                    else
                    {
                        Explode(transform.position);
                    }
                    break;
                case ProjectileBehavior.Ignore:
                    break;
            }

            return;
        }

        if(destructable && destroyDestructables)
        {
            destructable.Destruction(transform.forward, _pushBack);
            return;
        }

        //Terrain
        switch (terrainBehavior)
        {
            case TerrainBehavior.Explode:
                if (Time.time - _lastExplosionTerrain >= timeBetweenExplosionTerrain)
                {
                    if (explosionPointTerrain == ExplosionPoint.Self)
                        Explode(transform.position);
                    else
                        Explode(other.transform.position);

                    _lastExplosionTerrain = Time.time;
                }
                break;
            case TerrainBehavior.Penetrate:
                if(_startedPenetration == false)
                {
                    _speed *= speedMultOnPenetrate;
                    _startedPenetration = true;
                }
                break;
            case TerrainBehavior.Roll:
                RaycastHit hitInfo;
                Physics.Raycast(transform.position, transform.forward, out hitInfo, 0.1f);
                transform.forward = Vector3.Cross(hitInfo.normal, Vector3.ProjectOnPlane(transform.forward, hitInfo.normal)).normalized;
                if(_startedRoll == false)
                {
                    _speed *= speedMultOnRoll;
                    _startedRoll = true;
                }
                break;
            case TerrainBehavior.Stop:
                _speed = 0;
                break;
            case TerrainBehavior.Deflect:
                RaycastHit hitInfo2;
                Physics.Raycast(transform.position, transform.forward, out hitInfo2, 0.1f);
                transform.forward = Vector3.Reflect(transform.forward, hitInfo2.normal);
                break;

        }


    }

    private void AddHit(StateMachine target)
    {
        hits.Add(target, Time.time);
    }

    private void Explode(Vector3 location)
    {
        if (explosionPrefab != null)
        {
            GameObject go = Instantiate(explosionPrefab, location, Quaternion.identity) as GameObject;
            //go.GetComponent<Explosion>().Init(_damage, hits, _owner);
            Destroy(go, 5f);
        }

        if (dieOnExplosion)
            Kill();
    }

    public void Kill()
    {
        Destroy(this.gameObject);
    }

    #region enums

    public enum TerrainBehavior { Explode, Stop, Roll, Deflect, Penetrate}

    public enum CharacterBehavior { Explode, Consume, Kill }

    public enum ProjectileBehavior { Explode, Absorb, Ignore}

    public enum ExplosionPoint { Self, Target}

    #endregion
}
