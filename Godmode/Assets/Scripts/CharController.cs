using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class CharController : MonoBehaviour
{
    #region Variables

    [Header("Ground Controls")]
    public float runSpeed = 25f;
    public float runBoost = 3f;
    public bool yLock;
    public float yInputLockTimer = 0f;

    [Header("Jump Controls")] 
    public float jumpForce = 25;
    public bool canDoubleJump = true;
    public int maxJumps;
    public int jumpNumber = 0;
    public AnimationCurve jumpVelocity;
    public float jumpTimer;

    [Header("Wallrun Controls")]
    
    public float wallRunSpeed = 25f;
    public AnimationCurve wallRunHeight;
    public RaycastHit currentWall;
    public float originalY;
    public float wallRunTimer;

    [Header("Flight Controls")]
    public bool canFly = true;
    public float flightSpeed = 25f;
    public float flightBoost = 3f;

    [Header("Gravity")]
    public float gravity = 30f;

    [Header("Inertia")]
    [Range(0, 1)] public float groundDamp = 0.1f;
    [Range(0, 1)] public float airDamp = 0.5f;
    public Vector3 lastDir;

    [Header("States")]
    public bool isWallRunning;
    public bool isJumping;
    public bool inAir;
    public bool isFlying;
    public bool inDash;
    public bool isChargingTech;
    public bool isGuarding;
    public bool isChargingEnergy;
    public bool isCasting;
    public bool isFiringBeam;
    public bool isTossed;
    public bool isTeleporting;
    public bool isRagdoll;

    [Header("Melee")]
    public int punchNum = 0;
    public int maxCombo = 4;
    public bool canClick = true;
    public float fightingRadius = 3f;
    public Vector3 fightDir = Vector3.zero;
    public Vector3 teleportDestination;
    private int _hits;
    public int hits {
        get { return this._hits; }
        set { this._hits = value; UpdateHitCounter(); }
    }
    private int _tempDamage;
    public int tempDamage
    {
        get { return this._tempDamage; }
        set { this._tempDamage = value; UpdateDamageCounter(); }
    }

    [Header("Body")]
    public Rigidbody grabPoint;

    [Header("Toss")]
    public Vector3 tossDir = Vector3.zero;
    public LayerMask tossColMask;
    public float timeBeforeTossDamp = 1f;
    private float tossStartTime;
    public float gravityHolder = 0f;

    [Header("Stats")]
    public int maxHealth = 1000;
    public int maxEnergy = 1000;
    public int maxStamina = 1000;

    public int health = 1000;
    public int energy = 1000;
    public int stamina = 1000;

    public int healthRegenRate = 0;
    public float healthRegenTimer;
    public int energyRegenRate = 10;
    public float energyRegenTimer;
    public int staminaRegenRate = 10;
    public float staminaRegenTimer;

    public int energyChargeRate = 100;
    public float energyChargeTimer;

    [Header("Techniques")]
    public List<Technique> techniques = new List<Technique>();
    public int activeSlot = 1;

    public bool firedTech;
    public float spamTimer;
    public float techChargeTimer;
    public float lastTechChargeTimer;

    public Vector3 castPosition;

    public GameObject beamHead;
    float beamStartTime;

    public GameObject chargeObject;
    public bool attackStandby;
    public Technique attackAnimating;

    [Header("Guarding Timer")]
    public float guardTimer;
    public float perfectGuardTime = .2f;

    [Header("Destruction")]
    public float destructionRadius = 1f;
    public float destructionForce = 1f;

    [Header("References")]
    public CharacterController cr;
    public GameObject cam;
    public ThirdPersonCam camScript;
    public Animator anim;
    public Transform projectileSpotR;
    public Transform projectileSpotL;
    public Transform currentProjectileSpot;
    public TargetingSystem tarSys;
    public AnimationScript animScript;
    public VirtualInput vi;

    [Header("Particle Systems")]
    public bool playedFullCharge;
    public ParticleSystem fullCharge;
    public ParticleSystem chargeAura;
    public ParticleSystem chargeDust;

    [Header("Input Controls")]
    public Vector3 moveDir = Vector3.zero;
    public float doubleTapWindow = 0.5f;
    private float doubleTapTimer;
    private KeyCode lastKey;
    public bool movingForward;

    [Header("Local Player")]
    public bool localPlayer;
    public Transform autoTarget;
    public float fireEvery = 1f;
    public float fireTimer = 0;
    public bool needToCharge;
    //public bool w, a, s, d, sp, ls, f, q, e, rm, lm;

    [Header("UI")]
    public Image hBar;
    public Image[] eBars = new Image[5];
    public Image[] sBars = new Image[5];
    public Image chargeParent;
    public Image chargeBar;
    public Image minCharge;
    public TextMeshProUGUI hitCounter;
    public TextMeshProUGUI damageCounter;

    [Header("Physics")]
    public float pushPower;

    #endregion

    void Start()
    {
        cr = GetComponent<CharacterController>();
        if(localPlayer) cam = GameObject.Find("Camera");
        if (localPlayer) camScript = cam.GetComponent<ThirdPersonCam>();
        anim = GetComponentsInChildren<Animator>()[0];
        //hitBoxAnim = GetComponent<Animator>();
        tarSys = GetComponent<TargetingSystem>();
        animScript = GetComponentInChildren<AnimationScript>();
        animScript.anim = anim;
        hits = 0;
        currentProjectileSpot = projectileSpotR;
    }

    private void Update()
    {
        UpdateFunctions();
    }

    public void UpdateFunctions()
    {
        #region AI
        /*
        if(!localPlayer)
        {
            if (autoTarget)
            {
                tarSys.lockOn = autoTarget;
                tarSys.hardLock = true;
            }

            if (fireTimer >= fireEvery && !isChargingEnergy)
            {
                if (energy >= techniques[activeSlot - 1].energyCost)
                {
                    UseTechnique(activeSlot);
                    fireTimer = 0f;
                }
                else
                    needToCharge = true;
            }

            if (needToCharge)
            {
                ChargePress();
                if (energy >= 0.8 * maxEnergy)
                {
                    ChargeRelease();
                    needToCharge = false;
                }
            }

            fireTimer += Time.deltaTime;
        }
        */
        #endregion

        GroundCheck();
        
        InputCheck();

        if(isFiringBeam)
        {
            if (beamHead == null)
                ExitBeamMode();

            
            //beamHead.transform.forward = cam.transform.forward;
        }

        if(isTeleporting)
        {
            Teleporting();
        }
        else if (!isTossed)
            Move(moveDir);
        else
        {
            Toss();
            Move(tossDir + new Vector3(0, gravityHolder, 0));
        }

        UpdateInertia();

        RegenUpdate();
        ParticleCheck();

        if (inDash)
            DestructionSphere();
    }

    private void FixedUpdate()
    {
        float h = vi.horizontal;
        float v = vi.vertical;
        anim.SetFloat("Speed", v);
        anim.SetFloat("ySpeed", moveDir.y);
        anim.SetBool("InAir", inAir);
        //anim.SetFloat("Direction", h);
    }

    private void LateUpdate()
    {
        FightingSphere();
    }

    void InputCheck()
    {
        #region Hotkeys

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            activeSlot = 1;
            OnChangeActiveSlot();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            activeSlot = 2;
            OnChangeActiveSlot();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            activeSlot = 3;
            OnChangeActiveSlot();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            activeSlot = 4;
            OnChangeActiveSlot();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            activeSlot = 5;
            OnChangeActiveSlot();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            activeSlot = 6;
            OnChangeActiveSlot();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            activeSlot = 7;
            OnChangeActiveSlot();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            activeSlot = 8;
            OnChangeActiveSlot();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            activeSlot = 9;
            OnChangeActiveSlot();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            activeSlot = 0;
            OnChangeActiveSlot();
        }

        #endregion

        #region Mouse Input

        if (vi.lmb)
        {
            MousePressMain();
        }
        else if (vi.lmbUp)
        {
            MouseReleaseMain();
        }

        if(vi.rmb)
        {
            MousePressSecondary();
        }
        else if (vi.rmbUp)
        {
            MouseReleaseSecondary();
        }

        #endregion

        #region Movement

        float inputX = vi.horizontal; 
        float inputZ = vi.vertical;

        movingForward = (inputZ > 0);

        if (isChargingEnergy || isFiringBeam || anim.GetInteger("Combo") != 0)
        {
            inputX = inputZ = 0;
        }

        if (yLock)
        {
            yInputLockTimer += Time.deltaTime;
            if (yInputLockTimer > 1f)
            {
                yLock = false;
                yInputLockTimer = 0f;
            }
        }

        //Create a vector of movement from the inputs
        Vector3 inputVec;

        //If no target, create from own forward and right
        if (!tarSys.lockOn)
        {
            inputVec = (transform.forward * inputZ) + (transform.right * inputX);
            inputVec = inputVec.normalized;
        }
        else //Find the direction to the target (clamped to magnitude of 1)
        {
            Vector3 dirToTarget = tarSys.bodyCenter.transform.position - transform.position;
            dirToTarget = dirToTarget / dirToTarget.magnitude; //Normalize

            float angleZX = Mathf.Atan2(dirToTarget.z, dirToTarget.x);
            float zLength = Mathf.Sin(angleZX);
            float xLength = Mathf.Cos(angleZX);

            float angleZY = Mathf.Atan2(dirToTarget.z, dirToTarget.y);
            float yLength = Mathf.Cos(angleZY);

            Vector3 newDir = new Vector3(xLength, yLength, zLength);

            Debug.DrawRay(transform.position, newDir, Color.green);

            //Debug.Log(Vector3.Distance(transform.position, tarSys.bodyCenter.transform.position));
            TargetCenter lockObj = tarSys.bodyCenter;
            Vector3 flatDist = new Vector3(tarSys.lockOn.position.x, 0, tarSys.lockOn.position.z) - new Vector3(transform.position.x, 0, transform.position.z);

            //Debug.Log(dirToTarget);
            //if (flatDist.magnitude < lockObj.radius)
           // {
                //transform.position = lockObj.transform.position - ((lockObj.transform.position - tarSys.lockOn.position) *0.5f);
                
                //inputZ = Mathf.Clamp(inputZ, -1, 0);
           // }

            //if (Vector3.Distance(transform.position, tarSys.lockOn.transform.position) < tarSys.bodyCenter.radius)
            //{
            //    //if (inputZ >= 0)
            //        //transform.position = tarSys.bodyCenter.transform.position;

            //    inputZ = Mathf.Clamp(inputZ, -1, 0);
            //}

            inputVec = (newDir * inputZ) + (transform.right * inputX);
        }


        if (!isFlying) //Not in Fly Mode
        {
            inputVec.y = moveDir.y;
            moveDir = inputVec;

            if (vi.spaceDown) //Jump
            {
                if (/*tarSys.lockOn == null || Vector3.Distance(tarSys.lockOn.position, transform.position) > fightingRadius || */!(isChargingTech && techniques[activeSlot-1].type == HitType.Melee))
                {
                    if (!inAir && !isTossed || jumpNumber < maxJumps && !yLock)
                    {
                        if (WallCheck())
                        {
                            isWallRunning = true;
                            isJumping = false;
                            jumpNumber = 0;
                            originalY = transform.position.y;
                        }
                        else
                        {
                            //moveDir.y = jumpForce;
                            isJumping = true;
                            jumpTimer = 0f;
                            jumpNumber++;
                            anim.SetBool("Jump", true);
                            anim.SetBool("DoubleJump", (jumpNumber > 1));
                        }
                    }
                }
                else
                {
                    yLock = true;
                }
            }

            moveDir.x *= runSpeed;

            if (inAir && !isTossed)
            {
                moveDir.y = moveDir.y- gravity * Time.deltaTime;
            }

            moveDir.z *= runSpeed;

            if (inDash)
            {
                moveDir.x *= runBoost;
                moveDir.z *= runBoost;
            }

        }
        else //Fly Mode
        {
            moveDir = inputVec;

            if (vi.space)
            {
                if(/*tarSys.lockOn == null || Vector3.Distance(tarSys.lockOn.position, transform.position) > fightingRadius ||*/ !(isChargingTech && techniques[activeSlot-1].type == HitType.Melee) && !yLock)
                    moveDir.y = 1;
                else
                    yLock = true;
            }


            if (vi.lShift)
            {
                if (/*tarSys.lockOn == null || Vector3.Distance(tarSys.lockOn.position, transform.position) > fightingRadius ||*/ !(isChargingTech && techniques[activeSlot - 1].type == HitType.Melee) && !yLock)
                    moveDir.y = -1;
                else
                    yLock = true;
            }

            moveDir = moveDir.normalized;

            moveDir *= flightSpeed;

            if (inDash)
            {
                moveDir *= flightBoost;
            }

        }

        if(isJumping)
        {
            moveDir.y = jumpVelocity.Evaluate(jumpTimer);
            jumpTimer += Time.deltaTime;
        }

        if(isWallRunning)
        {
            Vector3 forward = Vector3.Cross(currentWall.normal, Vector3.up);
            moveDir = forward * wallRunSpeed;
            moveDir.y = wallRunHeight.Evaluate(wallRunTimer);
            wallRunTimer += Time.deltaTime;

            if (wallRunTimer > 2f)
            {
                isWallRunning = false;
                wallRunTimer = 0f;
            }
        }

        if (moveDir.x == 0)
            moveDir.x = lastDir.x;
        if (moveDir.y == 0)
            moveDir.y = lastDir.y;
        if (moveDir.z == 0)
            moveDir.z = lastDir.z;

        #endregion

        #region Guard

        if (vi.q)
        {
            GuardPress();
        }
        else if (vi.qUp)
        {
            GuardRelease();
        }

        #endregion

        #region Dash

        DoubleTapCheck();

        #endregion

        #region Fly

        if (vi.fDown)
        {
            FlyKey();
        }

        #endregion

        #region Charge

        if (vi.e)
        {
            ChargePress();
        }
        else if (vi.eUp || vi.horizontal != 0 || vi.vertical != 0)
        {
            ChargeRelease();
        }

        #endregion
    }

    void GroundCheck()
    {
        if (cr.isGrounded)
        {
            inAir = false;
            isJumping = false;
            jumpTimer = 0f;
            anim.SetBool("Jump", false);
            anim.SetBool("DoubleJump", false);
            jumpNumber = 0;
        }
        else //if(!isFlying)
        {
            inAir = true;
        }
    }

    bool WallCheck()
    {
        RaycastHit left = new RaycastHit();
        RaycastHit right = new RaycastHit();

        Physics.Raycast(transform.position, -transform.right, out left, 1f);
        Physics.Raycast(transform.position, transform.right, out right, 1f);

        if(left.collider && Vector3.Dot(left.normal, Vector3.up) < 0.01f && Vector3.Dot(left.normal, Vector3.up) > -0.01f && right.collider && Vector3.Dot(right.normal, Vector3.up) < 0.01f && Vector3.Dot(right.normal, Vector3.up) > -0.01f)
        {
            if(Vector3.Distance(transform.position, left.point) < Vector3.Distance(transform.position,right.point))
            {
                currentWall = left;
            }
            else
            {
                currentWall = right;
            }
            
            return true;
        }
        else if (left.collider && Vector3.Dot(left.normal, Vector3.up) < 0.01f && Vector3.Dot(left.normal, Vector3.up) > -0.01f)
        {
            currentWall = left;
            
            return true;
        }
        else if (right.collider && Vector3.Dot(right.normal, Vector3.up) < 0.01f &&  Vector3.Dot(right.normal, Vector3.up) > -0.01f)
        {
            currentWall = right;
            
            return true;
        }
        return false;
    }

    void DoubleTapCheck()
    {
        if (vi.aDown)
        {
            if (lastKey == KeyCode.A)
            {
                EnterDash();
                lastKey = KeyCode.None;
                doubleTapTimer = 0;
            }
            else
            {
                lastKey = KeyCode.A;
                doubleTapTimer = 0;
            }
        }
        else if (vi.sDown)
        {
            if (lastKey == KeyCode.S)
            {
                EnterDash();
                lastKey = KeyCode.None;
                doubleTapTimer = 0;
            }
            else
                lastKey = KeyCode.S;
        }
        else if (vi.dDown)
        {
            if (lastKey == KeyCode.D)
            {
                EnterDash();
                lastKey = KeyCode.None;
                doubleTapTimer = 0;
            }
            else
                lastKey = KeyCode.D;
        }
        else if (vi.wDown)
        {
            if (lastKey == KeyCode.W)
            {
                EnterDash();
                lastKey = KeyCode.None;
                doubleTapTimer = 0;
            }
            else
                lastKey = KeyCode.W;
        }
        else if (vi.spaceDown)
        {
            if (lastKey == KeyCode.Space)
            {
                EnterDash();
                lastKey = KeyCode.None;
                doubleTapTimer = 0;
            }
            else
                lastKey = KeyCode.Space;
        }
        else if (vi.lShiftDown)
        {
            if (lastKey == KeyCode.LeftShift)
            {
                EnterDash();
                lastKey = KeyCode.None;
                doubleTapTimer = 0;
            }
            else
                lastKey = KeyCode.LeftShift;
        }
        else if (
            !vi.w &&
            !vi.a &&
            !vi.s &&
            !vi.d &&
            !vi.space &&
            !vi.lShift)
        {
            if (inDash)
                ExitDash();
        }

        if(lastKey != KeyCode.None) doubleTapTimer += Time.deltaTime;
        if (doubleTapTimer >= doubleTapWindow)
        {
            lastKey = KeyCode.None;
            doubleTapTimer = 0;
        }
    }

    void FlyKey()
    {
        if (isFlying)
        {
            ExitFlyMode();
            if (inDash)
                ExitDash();
        }
        else
            EnterFlyMode();
    }

    void EnterFlyMode()
    {
        isFlying = true;
        isJumping = false;
        ResetInertia();
        anim.SetBool("Flying", true);
        anim.SetBool("Jump", false);
    }

    void ExitFlyMode()
    {
        isFlying = false;
        anim.SetBool("Flying", false);
    }

    public void EnterDash()
    {
        if (isChargingEnergy)
            return;

        inDash = true;
        doubleTapTimer = 0;
        if (canFly)
            EnterFlyMode();
        if(camScript)
            camScript.SetMaxFov(true);
        anim.SetBool("Dashing", true);

        Vector3 dustPos = transform.position - new Vector3(0, cr.height / 2, 0) - transform.forward;
        DustManager.instance.Create(dustPos, -moveDir);
    }

    public void ExitDash()
    {
        inDash = false;
        doubleTapTimer = 0;
        if(camScript)
            camScript.SetMaxFov(false);
        anim.SetBool("Dashing", false);
    }

    void Move(Vector3 direction)
    {
        if(tarSys.lockOn)
        {

            //Vector3 flatDir = new Vector3(tarSys.bodyCenter.transform.position.x, 0, tarSys.bodyCenter.transform.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
                Vector3 flatDir = tarSys.bodyCenter.transform.position - transform.position;

                if (flatDir.magnitude < runSpeed * Time.deltaTime && movingForward)
                {
                    transform.position = new Vector3(tarSys.bodyCenter.transform.position.x, transform.position.y, tarSys.bodyCenter.transform.position.z);
                }
                else
                {
                    cr.Move(direction * Time.deltaTime);
                }
        }
        else
        {
            cr.Move(direction * Time.deltaTime);
        }
    }

    void MousePressMain()
    {
        Technique t = techniques[activeSlot - 1];

        if (!t.autoFire && firedTech)
        {
            return; //Can't spam
        }

        else if (t.autoFire && firedTech)
        {
            spamTimer += Time.deltaTime;
            if (spamTimer < t.timeBetweenFire)
                return;
        }

        if (!t.chargable && t.type != HitType.Cast) //Not Chargable only
        {
            //UseTechnique(activeSlot);

            AnimateAttack(t);

            firedTech = true;
            spamTimer = 0;
            return;
        }

        // Castable
        if (t.type == HitType.Cast)
        {
            if (vi.lmbDown)
            {
                Debug.Log("Mouse Down");
                if (isCasting)
                {
                    UseTechnique(activeSlot);
                }
                else
                {

                }
            }
            return;
        }

        //Chargable
        if (t.chargable)
        {
           TechCharge();
        }

    }

    void MouseReleaseMain()
    {
        Technique t = techniques[activeSlot - 1];

        if(t.type == HitType.Melee)
        {
            UseTechnique(activeSlot);
        }
        else if (isChargingTech && techChargeTimer >= t.minChageTime)
        {
            //UseTechnique(activeSlot);
            AnimateAttack(t);
            ExitTechCharge();
        }
        else if(isFiringBeam)
        {
            ExitBeamMode();
        }

        firedTech = false;
        spamTimer = 0f;

        ExitTechCharge();
    }

    void MousePressSecondary()
    {
        Technique t = techniques[activeSlot - 1];

        if (!t.secondary)
            return;

        UseTechnique(activeSlot, 1);
    }

    void MouseReleaseSecondary()
    {

    }

    public void UseTechnique(Technique t)
    {
        int i = 1;
        foreach(Technique tech in techniques)
        {
            if(tech.name == t.name)
            {
                UseTechnique(i);
            }
            i++;
        }
    }

    void UseTechnique(int slot, int mouseButton)
    {
        if (slot > techniques.Count)
            return;

        Technique t = techniques[slot - 1];

        if (t.energyCost > energy || t.healthCost > health)
            return;
        else
        {
            health -= t.healthCost;
            energy -= t.energyCost;
            UpdateUI();
        }

        GameObject go;
        Projectile po;

        switch (t.type)
        {
            case HitType.Melee:

                //if (vi.space)
                //    anim.Play("Uppercut");
                //else if (Input.GetKey(KeyCode.LeftShift))
                //else 
                if (canClick)
                {
                    if (mouseButton == 0)
                    {
                        punchNum++;
                        if (punchNum > maxCombo)
                        {
                            ResetCombo();
                            punchNum++; //If on last punch, no need to go back to 0 so instead go to first punch
                        }

                        anim.SetInteger("Combo", punchNum);
                        canClick = false;
                        ResetDoneAnim();
                    }
                    else if (mouseButton == 1)
                    {
                        anim.SetBool("Grab", true);
                    }
                }

                //if (punchNum == 1) anim.SetInteger("Combo", punchNum);

                if (tarSys.lockOn)
                {
                    fightDir = (tarSys.lockOn.position - transform.position).normalized;
                }
                else
                    fightDir = transform.forward;

                break;
            case HitType.Projectile:
                go = Instantiate(t.projectile, currentProjectileSpot.position, Quaternion.identity) as GameObject;
                go.transform.forward = (cam) ? cam.transform.forward : transform.forward;

                if (t.chargable)
                {
                    float currentScale = go.transform.localScale.x;
                    float chargedScale = Mathf.Lerp(currentScale, t.fullChargeScale, techChargeTimer / t.fullChargeTime);
                    go.transform.localScale = new Vector3(chargedScale, chargedScale, chargedScale);
                }

                po = go.GetComponent<Projectile>();

                if (po != null)
                {
                    po.damage = t.damage;
                    po.speed = t.speed;
                    po.blowBack = t.blowBackForce;
                    //po.owner = this;
                    po.target = tarSys.lockOn ? tarSys.lockOn : null;
                }

                break;
            case HitType.Beam:
                go = Instantiate(t.projectile, currentProjectileSpot.position + transform.forward, Quaternion.identity) as GameObject;
                beamHead = go;

                beamHead.transform.forward = transform.forward;

                po = beamHead.GetComponent<Projectile>();

                if (t.chargable)
                {
                    float currentScale = beamHead.transform.localScale.x;
                    float chargedScale = Mathf.Lerp(currentScale, t.fullChargeScale, techChargeTimer / t.fullChargeTime);
                    beamHead.transform.localScale = new Vector3(chargedScale, chargedScale, chargedScale);
                    TrailRenderer tr = beamHead.GetComponent<TrailRenderer>();
                }

                if (po != null)
                {
                    po.damage = t.damage;
                    po.speed = t.speed;
                    po.blowBack = t.blowBackForce;
                    //po.owner = this;
                }

                EnterBeamMode();

                break;
            case HitType.Hitscan:
                break;
            case HitType.Cast:
                go = Instantiate(t.projectile, camScript.castProj.transform.position, Quaternion.identity) as GameObject;
                go.transform.forward = cam.transform.forward;
                po = go.GetComponent<Projectile>();

                if (po != null)
                {
                    po.damage = t.damage;
                    po.speed = t.speed;

                }

                break;
            case HitType.Other:
                break;
        }
    }

    void UseTechnique(int slot)
    {
        UseTechnique(slot, 0);
    }

    void OnChangeActiveSlot()
    {
        if(isChargingTech)
        {
            ExitTechCharge();
        }

        if(isFiringBeam)
        {
            ExitBeamMode();
        }

    }

    void TechCharge()
    {
        if (isChargingEnergy)
            ChargeRelease();
        if (isGuarding)
            GuardRelease();

        Technique t = techniques[activeSlot - 1];

        isChargingTech = true;

        if(t.type == HitType.Melee)
        {
            if (techChargeTimer > 0.25f)
            {
                if (vi.space)
                {
                    anim.SetBool("ChargeKick", true);
                    anim.SetBool("ChargePunch", false);
                }
                else
                {
                    anim.SetBool("ChargePunch", true);
                    anim.SetBool("ChargeKick", false);
                }

            }
        }
        else if (t.type == HitType.Beam && techChargeTimer > 0.1f && !anim.GetBool("ChargingAttack"))
        {
            AnimateCharge(t);
            if (camScript.view != ThirdPersonCam.camView.RightZoomBeam)
                camScript.TransitionView(ThirdPersonCam.camView.RightZoomBeam);
        }
        else if(t.type == HitType.Projectile && techChargeTimer > 0.1f && !anim.GetBool("ChargingAttack"))
        {
            AnimateCharge(t);
            anim.SetInteger("AttackAnim", t.attackAnimation);
        }

        if (techChargeTimer < t.fullChargeTime)
        {
            techChargeTimer += Time.deltaTime;

            if (techChargeTimer > t.fullChargeTime)
                techChargeTimer = t.fullChargeTime;

            UpdateUI();
        }

        if (techChargeTimer >= t.minChageTime && techChargeTimer > 1f && !playedFullCharge)
        {
            fullCharge.Play();
            playedFullCharge = true;
        }

        if(!chargeObject)
        {
            if (t.chargePrefab)
                chargeObject = Instantiate(t.chargePrefab, projectileSpotR);
        }
        
    }

    void ExitTechCharge()
    {
        isChargingTech = false;
        lastTechChargeTimer = techChargeTimer;
        techChargeTimer = 0f;

        if (anim.GetBool("ChargePunch"))
            anim.SetBool("ChargePunch", false);
        if (anim.GetBool("ChargeKick"))
            anim.SetBool("ChargeKick", false);

        if (anim.GetBool("ChargingAttack"))
            anim.SetBool("ChargingAttack", false);

        if (camScript.view != ThirdPersonCam.camView.InstantFront)
            camScript.TransitionView(ThirdPersonCam.camView.TransitionFront);

        if (playedFullCharge)
        {
            playedFullCharge = false;
        }

        if (chargeObject)
        {
            Destroy(chargeObject);
            chargeObject = null;
        }
    }

    void GuardPress()
    {
        if (isChargingTech || isFiringBeam)
            return;

        if (guardTimer < perfectGuardTime)
        {
            Collider[] cols = Physics.OverlapCapsule(transform.position - transform.up, transform.position + transform.up, 1f);
            foreach (Collider c in cols)
            {
                if (c.GetComponent<HitBox>())
                {
                    anim.Play("InstinctDodgeRight");
                    return;
                }
            }
        }

        if (!isGuarding) isGuarding = true;
        guardTimer += Time.deltaTime;
        anim.SetBool("Guard", true);
    }

    void GuardRelease()
    {
        isGuarding = false;
        guardTimer = 0;
        anim.SetBool("Guard", false);
    }

    void ChargePress()
    {
        if (isChargingTech || isFiringBeam)
            return;

        if (!anim.GetCurrentAnimatorStateInfo(1).IsName("FightFlyIdle") &&
            !anim.GetCurrentAnimatorStateInfo(1).IsName("FightIdle") &&
            !anim.GetCurrentAnimatorStateInfo(1).IsName("New State"))
            return;

            if (inDash)
        {
            ExitDash();
        }

        if (!isChargingEnergy)
        {
            isChargingEnergy = true;
            anim.SetBool("Charge", true);
            //ResetInertia();
        }

        if (chargeAura && !chargeAura.isPlaying)
            chargeAura.Play();

        energyChargeTimer += Time.deltaTime;

        if (energyChargeTimer >= 1)
        {
            energyChargeTimer -= 1;
            energy += energyChargeRate;
            energy = Mathf.Clamp(energy, 0, maxEnergy);

            UpdateUI();
        }

        if (camScript && !camScript.continousShake)
            camScript.StartShake(false);

        if (inAir)
            EnterFlyMode(); 
    }

    void ChargeRelease()
    {
        isChargingEnergy = false;
        anim.SetBool("Charge", false);
        chargeAura.Stop();
        energyChargeTimer = 0f;

        if (camScript && camScript.continousShake)
            camScript.EndShake();
    }

    void ParticleCheck()
    {
        if(isChargingEnergy && chargeDust)
        {
            if (!chargeDust.isPlaying && !inAir)
            {
                chargeDust.time = 0;
                chargeDust.Play();
            }
        }
        else if(chargeDust)
        {
            if (chargeDust.isPlaying || inAir)
                chargeDust.Stop();
        }
    }

    void EnterBeamMode()
    {
        //anim.SetBool("ChargeBeam", false);
        isFiringBeam = true;
        isChargingTech = false;
        techChargeTimer = 0f;
        beamStartTime = Time.time;

        if (inAir && !isFlying)
            EnterFlyMode();

        if(inDash)
        {
            ExitDash();
        }

        anim.SetBool("Beam", isFiringBeam);
        camScript.TransitionView(ThirdPersonCam.camView.TransitionFront);
        camScript.StartShake(false);
        ResetInertia();
    }

    void ExitBeamMode()
    {
        isFiringBeam = false;
        if (beamHead)
        {
            beamHead.GetComponent<TrailRenderer>().time = Time.time - beamStartTime;
            beamHead.GetComponent<TrailRenderer>().endWidth = 0f;
            beamHead = null;
        }
        ExitAnimationCheck();
        camScript.EndShake();
    }

    void UpdateInertia()
    {
        lastDir = moveDir;

        lastDir *= Mathf.Pow(groundDamp, Time.deltaTime);

    }

    void ResetInertia()
    {
        lastDir = moveDir = Vector3.zero;
    }

    void RegenUpdate()
    {
        if (!localPlayer)
            return;

        #region Health

        healthRegenTimer += Time.deltaTime;
        if (healthRegenTimer >= 1f / healthRegenRate)
        {
            health++;
            healthRegenTimer -= 1f / healthRegenRate;
        }

        health = Mathf.Clamp(health, 0, maxHealth);

        #endregion

        #region Energy

        energyRegenTimer += Time.deltaTime;
        if(energyRegenTimer >= 1f / energyRegenRate)
        {
            energy++;
            energyRegenTimer -= 1f / energyRegenRate;
        }

        energy = Mathf.Clamp(energy, 0, maxEnergy);

        #endregion

        #region Stamina

        staminaRegenTimer += Time.deltaTime;
        if (staminaRegenTimer >= 1f / staminaRegenRate)
        {
            stamina++;
            staminaRegenTimer -= 1f / staminaRegenRate;
        }

        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        #endregion

        UpdateUI();
    }

    void UpdateUI()
    {
        if (!localPlayer)
            return;

        #region Health

        hBar.fillAmount = (float) health / maxHealth;

        #endregion

        #region Energy

        float energyPerBar = maxEnergy / 5;

        int fullBars = Mathf.FloorToInt(energy / energyPerBar);
        float leftOver = energy % energyPerBar;
        //Debug.Log(leftOver);

        int i;
        for (i = 0; i < fullBars; i++)
        {
            eBars[i].fillAmount = 1;
        }

        if (leftOver != 0 && i < eBars.Length)
            eBars[i].fillAmount = leftOver / energyPerBar;

        for(i++; i<eBars.Length; i++)
        {
            eBars[i].fillAmount = 0;
        }

        #endregion

        #region Stamina

        float staminaPerBar = maxStamina / 5;

        int fullSTMBars = Mathf.FloorToInt(stamina / staminaPerBar);
        float leftOverSTM = stamina % staminaPerBar;

        int j;
        for(j = 0; j< fullSTMBars; j++)
        {
            sBars[j].fillAmount = 1;
        }

        if (leftOverSTM != 0 && j < sBars.Length)
            sBars[j].fillAmount = leftOverSTM / staminaPerBar;

        for (j++; j < sBars.Length; j++)
        {
            sBars[j].fillAmount = 0;
        }
    

        #endregion

        #region ChargeTech

        chargeParent.gameObject.active = isChargingTech && techniques[activeSlot - 1].minChageTime > 1;

        chargeBar.fillAmount = techChargeTimer / techniques[activeSlot-1].fullChargeTime;
        float x = techniques[activeSlot - 1].minChageTime / techniques[activeSlot - 1].fullChargeTime * chargeParent.rectTransform.rect.width;
        minCharge.rectTransform.anchoredPosition = new Vector2(x, 0);
        #endregion
    }

    void UpdateHitCounter()
    {
        if (!localPlayer)
            return;
        hitCounter.text = hits.ToString();
    }

    void UpdateDamageCounter()
    {
        damageCounter.text = tempDamage.ToString();
    }

    void FightingSphere()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, fightingRadius);
        List<Transform> enemies = new List<Transform>();

        foreach(Collider c in colliders)
        {
            CharacterController ch = c.GetComponent<CharacterController>();
            if (ch != null && ch.transform.root != transform)
            {
                enemies.Add(c.transform);
            }
        }

        Transform closest = null;

        foreach (Transform enemy in enemies)
        {
            if (closest == null ||
                Vector3.Distance(transform.position, enemy.position) < Vector3.Distance(transform.position, closest.position))
                closest = enemy;
        }

        if(closest != null)
            LockOn(closest);
    }

    void LockOn(Transform target)
    {
        if (tarSys == null || tarSys.hardLock)
            return;

        tarSys.LockOn(target, false);
    }

    void DestructionSphere()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, destructionRadius);

        foreach (Collider c in colliders)
        {
            if (c.CompareTag("Projectile"))
                continue;

            Destructable d = c.GetComponent<Destructable>();
            Rigidbody r = (c.transform != transform) ? c.GetComponent<Rigidbody>() : null;
            if (d)
            {
                d.Destruction(transform.forward, destructionForce);
                camScript.StartShake(.75f, true);
            }

            if(r)
            {
                Vector3 force = (c.transform.position - transform.position).normalized * 10f;
                r.AddForce(force, ForceMode.Impulse);
                if (!c.CompareTag("Player"))
                    if(camScript)camScript.StartShake(.75f, true);
            }
        }
    }

    public void EnterToss(Vector3 dir)
    {
        isTossed = true;
        if (anim != null)
            anim.SetBool("Tossed", true);
        tossDir = dir;
        transform.forward = -tossDir;
        tossStartTime = Time.time;

        if (isFiringBeam)
            ExitBeamMode();
    }

    void Toss()
    {
        //Dampen
        if (Time.time - tossStartTime >= timeBeforeTossDamp)
        {
            tossDir *= Mathf.Pow(groundDamp, Time.deltaTime);
        }

        if (tossDir.magnitude < 0.1f)
            ExitToss();

        gravityHolder -= gravity * Time.deltaTime;
        gravityHolder *= Mathf.Pow(groundDamp, Time.deltaTime);

        anim.SetFloat("TossMag", tossDir.magnitude);
    }

    void ExitToss()
    {
        isTossed = false;
        if (anim != null)
            anim.SetBool("Tossed", false);
        tossDir = Vector3.zero;
    }

    public bool Hit(int damage, CharController enemy)
    {
        if (isGuarding)
        {
            if (guardTimer <= perfectGuardTime)
            {
                PerfectGuard(enemy);
                return false;
            }
            else
                UpdateHealth(Mathf.FloorToInt(-damage * 0.25f));
            if (camScript) camScript.StartShake(0.5f, false);
        }
        else
        {
            if(!isTossed)
                anim.SetBool("Hit", true);
            UpdateHealth(-damage);
            if(camScript) camScript.StartShake(0.25f, true);
        }
        return true;
    }

    void UpdateHealth(int amount)
    {
        health += amount;
        if (health <= 0)
            Die();
    }

    void Die()
    {
        if (isChargingEnergy)
            ChargeRelease();

        if (inDash)
            ExitDash();

        if (isFlying)
            ExitFlyMode();

        if (isFiringBeam)
            ExitBeamMode();

        if (isChargingTech)
            ExitTechCharge();

        SetRagdoll(true);
    }

    void SetRagdoll(bool active)
    {
        cr.enabled = anim.enabled = this.enabled = !active;
    }

    public void Grabbed(CharController grabber)
    {
        SetRagdoll(true);
    }

    void PerfectGuard(CharController enemy)
    {
        if (enemy.tarSys.lockOn == this) enemy.tarSys.lockOn = null;

        Vector3 dir = enemy.transform.position - transform.position;
        Vector3 destination = enemy.transform.position + dir.normalized *2f;

        StartTeleporting(destination);
    }

    void StartTeleporting(Vector3 destination)
    {
        isTeleporting = true;
        teleportDestination = destination;
    }

    void Teleporting()
    {
        transform.position = teleportDestination;
        StopTeleporting();
    }

    void StopTeleporting()
    {
        isTeleporting = false;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        if(rb)
        {
            rb.velocity = hit.moveDirection * pushPower;
        }

        if (isTossed)
        {
            RaycastHit h;
            if(Physics.SphereCast(transform.position, .5f, tossDir.normalized, out h ,1f, tossColMask))
            {
                Destructable d = h.collider.GetComponent<Destructable>();
                if(d)
                {
                    tossDir *= 0.8f;
                    d.Destruction(tossDir, destructionForce);
                }
                else
                {
                    //Debug.Log(h.transform);
                    ImpactManager.instance.Create(transform.position, -tossDir);
                    ExitToss();
                    anim.SetTrigger("Impact");
                }
            }
        }

    }

    void OnDrawGizmosSelected()
    {
        if (isTossed)
        {
            Vector3 startPoint = transform.position;
            startPoint += tossDir.normalized;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startPoint, .5f);
        }

        if(isTossed || inDash)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, destructionRadius);
        }
        
    }

    public void EnableClick()
    {
        canClick = true;
    }

    public void DisableClick()
    {
        canClick = false;
        Debug.Log("Disable");
    }

    public void ResetCombo()
    {
        punchNum = 0;
        anim.SetInteger("Combo", punchNum);
        EnableClick();
    }

    public void DoneAnim()
    {
        anim.SetBool("DoneAnim", true);
    }

    public void ResetDoneAnim()
    {
        anim.SetBool("DoneAnim", false);
    }

    void AnimateCharge(Technique t)
    {
        anim.SetInteger("ChargeAnim", t.chargeAnimation);
        anim.SetBool("ChargingAttack", true);
    }

    void AnimateAttack(Technique t)
    {
        attackAnimating = t;
        attackStandby = true;
        
        anim.SetInteger("AttackAnim", t.attackAnimation);
        
        //Finished Charging, exist charging state
        if (anim.GetBool("ChargingAttack"))
            anim.SetBool("ChargingAttack", false);

        anim.SetBool("FiringAttack", true);
    }

    public void FireAttackLeft()
    {
        currentProjectileSpot = projectileSpotL;
        UseTechnique(attackAnimating);

        attackStandby = false;
        attackAnimating = null;
    }

    public void FireAttackRight()
    {
        currentProjectileSpot = projectileSpotR;
        UseTechnique(attackAnimating);

        attackStandby = false;
        attackAnimating = null;
    }

    public void FireAttackPartLeft()
    {
        currentProjectileSpot = projectileSpotL;
        UseTechnique(attackAnimating);
    }

    public void FireAttackPartRight()
    {
        currentProjectileSpot = projectileSpotR;
        UseTechnique(attackAnimating);
    }

    public void ExitAnimationCheck()
    {
        if (isFiringBeam)
            return;

        if (anim.GetBool("FiringAttack"))
        {
            anim.SetBool("FiringAttack", false);
        }
    }
}
