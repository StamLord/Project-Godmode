using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(StateMachine))]

public class TechManager : MonoBehaviour
{
    [Header("References")]
    public StateMachine character;
    public CharacterStats stats;
    public Animator anim;
    public VirtualInput vi;
    public ThirdPersonCam camScript;
    public Transform leftProjectileSpot;
    public Transform rightProjectileSpot;
    public Transform currentProjectileSpot;

    [Header("UI")]
    public GameObject chargeParent;
    public Image emptyBar;
    public Image chargeBar;
    public Image minCharge;
    public Image overCharge;

    [Header("Instantiated")]
    [SerializeField] private GameObject chargeObject;
    [SerializeField] private GameObject beamHead;
    [SerializeField] private float beamStartTime;

    [Header("Particle Systems")]
    public ParticleSystem fullCharge;
    private bool playedFullCharge;

    [Header("Melee")]
    [SerializeField] private bool canClick = true;
    [SerializeField] private int punchNum;

    public Move currentMove { get; private set; } //The move currently being executed
    private Move _nextMove;                       //Next move to be executed
    private bool clicked;                         //Flag to avoid executing combo infinitely when holding LMB

    [Header("Techniques")]
    public List<Technique> techniques;

    [SerializeField] private int activeSlot = 1;
    [SerializeField] private float techChargeTimer;
    [SerializeField] private float lastTechChargeTimer;

    public Technique GetSelected { get { return this.techniques[activeSlot - 1]; } }

    [Header("Animation")]
    public Technique attackAnimating;
    public bool animatedCam;

    #region Flags for outside references

    public bool isChargingTech { get; private set; }

    #endregion

    #region Temporary holders for charging attack

    private int healthHolder;
    private int energyHolder;
    private int staminaHolder;

    #endregion

    private void Start()
    {
        character = GetComponent<StateMachine>();
        stats = character.stats;
        anim = character.anim;
        vi = character.vi;
        camScript = character.camScript;
        UpdateUI();
    }

    void Update()
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
            activeSlot = 10;
            OnChangeActiveSlot();
        }

        #endregion
    }

    public void OnChangeActiveSlot()
    {
        ExitTechCharge(false);
    }

    #region Special

    public void UseTechnique()
    {
        UseTechnique(GetSelected, 0);
    }

    public void UseTechnique(Technique t)
    {
        UseTechnique(t, 0);
    }

    public void UseTechnique(Technique tech, int mouseButton)
    {
        if (!tech)
        {
            UnityEngine.Debug.LogWarning("Tried to use Technique that is null!");
            return;
        }

        SpecialArt t = tech as SpecialArt;
        if(t == null)
        {
            UnityEngine.Debug.LogWarning("Technique could not be cast as Special Art");
            return;
        }

        if(t.chargable == false)
        {
            stats.UpdateHealth(-t.healthCost);
            stats.UpdateEnergy(-t.energyCost);
            stats.UpdateStamina(-t.staminaCost);
        }

        GameObject go;
        Projectile po;

        if (chargeObject != null)
            Destroy(chargeObject);

        switch (t.type)
        {
            case HitType.Projectile:

                go = Instantiate(t.projectile, currentProjectileSpot.position, Quaternion.identity) as GameObject;
                go.transform.forward = (character.cam) ? character.cam.transform.forward : transform.forward;

                if (t.chargable)
                {
                    float currentScale = go.transform.localScale.x;
                    float chargedScale = Mathf.Lerp(currentScale, t.fullChargeScale, techChargeTimer / t.fullChargeTime);
                    go.transform.localScale = new Vector3(chargedScale, chargedScale, chargedScale);
                }

                po = go.GetComponent<Projectile>();

                if (po != null)
                {
                    po.Initialize(
                        character,
                        character.ts.lockOn ? character.ts.lockOn : null,
                        t.damage,
                        t.speed,
                        t.blowBackForce);
                }

                break;
            case HitType.Beam:

                go = Instantiate(t.projectile, currentProjectileSpot.position + transform.forward, Quaternion.identity, character.transform) as GameObject;
                beamHead = go;

                beamHead.transform.forward = transform.forward;

                Projectile[] pro = beamHead.GetComponentsInChildren<Projectile>();

                if (t.chargable)
                {
                    float currentScale = beamHead.transform.localScale.x;
                    float chargedScale = Mathf.Lerp(currentScale, t.fullChargeScale, techChargeTimer / t.fullChargeTime);
                    beamHead.transform.localScale = new Vector3(chargedScale, chargedScale, chargedScale);
                }
                
                foreach(Projectile p in pro)
                {
                    if (p != null)
                    {
                        p.Initialize(
                            character,
                            null,
                            t.damage,
                            0,
                            t.blowBackForce);
                    }
                }

                beamStartTime = Time.time;

                break;
            case HitType.Hitscan:
                break;
            case HitType.Cast:
                break;
            case HitType.Other:
                break;
        }
    }

    #endregion

    #region Melee

    public void UseMartialArt()
    {
        MartialArt m = GetSelected as MartialArt;
        UseMartialArt(m, 0);
    }

    public void UseMartialArt(MartialArt m)
    {
        UseMartialArt(m, 0);
    }

    public void UseMartialArt(MartialArt m, int mouseButton)
    {
        if (character.GetCurrentState is DashState)
        {
            if (m.dashAttack.energyCost > stats.GetEnergy ||
                m.dashAttack.healthCost > stats.GetHealth ||
                 m.dashAttack.staminaCost > stats.GetStamina)
                return;

            currentMove = m.dashAttack;
            character.SetState<ChargeAttackState>();

            stats.UpdateHealth(-m.dashAttack.healthCost);
            stats.UpdateStamina(-m.dashAttack.staminaCost);
            stats.UpdateEnergy(-m.dashAttack.energyCost);
            return;
        }

        #region Check

        Move nextMove = m.moveArray[(punchNum + 1)%m.moveArray.Length];

        if (nextMove.healthCost > stats.GetHealth ||
            nextMove.staminaCost > stats.GetStamina ||
            nextMove.energyCost > stats.GetEnergy)
            return;

        #endregion

        if (canClick)
        {
            if (character.GetCurrentState is AttackState)
                (character.GetCurrentState as AttackState).ResetState();
            else
                character.SetState<AttackState>();

            if (mouseButton == 0)
            {
                punchNum++;
                if (punchNum > m.moveArray.Length)
                {
                    ResetCombo();
                    punchNum++; //If on last punch, no need to go back to 0 so instead go to first punch
                }

                _nextMove = m.moveArray[punchNum - 1];

                

                anim.SetInteger("Combo", punchNum);
                canClick = false;
                ResetDoneAnim();
            }
            else if (mouseButton == 1)
            {
                //Grab / Finisher
            }
        }
    }

    public void EnableClick()
    {
        canClick = true;
    }

    public void DisableClick()
    {
        canClick = false;
    }

    public void ResetCombo()
    {
        punchNum = 0;
        anim.SetInteger("Combo", punchNum);
        EnableClick();
    }

    #endregion

    /// <summary>
    /// Called each frame while holding a button and a chargable Technique is selected
    /// </summary>
    public void TechCharge()
    {
        isChargingTech = true;

        UpdateUI();

        Technique t = GetSelected;
        MartialArt martial = t as MartialArt;
        SpecialArt special = t as SpecialArt;

        float fullChargeTime = 0;
        float minChargeTime = 0;

        if (martial != null)
        {
            fullChargeTime = martial.moveArray[punchNum].fullChargeTime;
            minChargeTime = martial.moveArray[punchNum].minChargeTime;
        }
        else if (special != null)
        {
            fullChargeTime = special.fullChargeTime;
            minChargeTime = special.minChargeTime;
        }

        //Timer
        if (techChargeTimer < fullChargeTime)
        {
            if(special != null)
            {
                int tempHealth = Mathf.CeilToInt((special.healthCost / fullChargeTime) * Time.deltaTime);
                healthHolder += tempHealth;
                stats.UpdateHealth(-tempHealth);

                int tempEnergy = Mathf.CeilToInt((special.energyCost / fullChargeTime) * Time.deltaTime);
                energyHolder += tempEnergy;
                stats.UpdateEnergy(-tempEnergy);

                int tempStamina = Mathf.CeilToInt((special.staminaCost / fullChargeTime) * Time.deltaTime);
                staminaHolder += tempStamina;
                stats.UpdateStamina(-tempStamina);
            }

            techChargeTimer += Time.deltaTime;

            if (techChargeTimer > fullChargeTime)
                techChargeTimer = fullChargeTime;

            
        }
        else if (techChargeTimer >= minChargeTime && techChargeTimer > 0f && playedFullCharge == false)
        {
            fullCharge.Play();
            playedFullCharge = true;
        }

        #region Visual

        //Charge Effect
        if(t is SpecialArt)
        {
            if ((t as SpecialArt).chargePrefab && !chargeObject)
            {
                chargeObject = Instantiate((t as SpecialArt).chargePrefab, rightProjectileSpot);
            }
        }

        //Animations
        if (t is MartialArt)
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
        else if (t is SpecialArt)
        {
            if ((t as SpecialArt).type == HitType.Beam && techChargeTimer > 0.1f)
            {
                if (camScript && camScript.view != ThirdPersonCam.CamView.RightZoomBeam)
                    camScript.TransitionView(ThirdPersonCam.CamView.RightZoomBeam);

                //If not moving activates 360 rotating camera
                if (vi.vertical == 0 && vi.horizontal == 0)
                {
                    if (animatedCam == false)
                        camScript.SetExternalCamera(0, true);
                }
                else
                {
                    camScript.ResetAllExternalCams();
                }

                animatedCam = true; //Sets flag regardless if cam was activated or not so it will only rotate if standing still in the begining
            }
            if (anim.GetBool("ChargingAttack") == false)
                AnimateCharge(t as SpecialArt);
        }

        #endregion
    }

    /// <summary>
    /// Called to stop charging: updates UI, animations and resets temporary holders. 
    /// </summary>
    public void ExitTechCharge(bool executingAttack)
    {
        isChargingTech = false;
        animatedCam = false;

        UpdateUI();

        #region Visual

        anim.SetBool("ChargingAttack", false);
        anim.SetBool("ChargePunch", false);
        anim.SetBool("ChargeKick", false);

        if (camScript && camScript.view != ThirdPersonCam.CamView.InstantFront)
        {
            camScript.TransitionView(ThirdPersonCam.CamView.TransitionFront);
            camScript.ResetAllExternalCams();
        }

        #endregion

        lastTechChargeTimer = techChargeTimer;
        techChargeTimer = 0f;
        playedFullCharge = false;

        if(executingAttack == false)
        {
            stats.UpdateHealth(healthHolder);
            stats.UpdateEnergy(energyHolder);
            stats.UpdateStamina(staminaHolder);

            if (chargeObject)
                Destroy(chargeObject);
        }

        healthHolder = staminaHolder = energyHolder = 0;
    }

    public void ExitBeamMode()
    {
        if(beamHead)
        {
            Destroy(beamHead);
            beamHead = null;
        }
        ExitAnimationCheck();
    }

    /// <summary>
    /// Called from States that allow Main Mouse Button to be pressed
    /// </summary>
    public void MousePressMain()
    {
        Technique t = GetSelected;
        MartialArt martial = t as MartialArt;
        SpecialArt special = t as SpecialArt;

        if (martial != null)
        {
            //if ((t as MartialArt).moveArray[(punchNum + 1) % (t as MartialArt).moveArray.Length].chargable)
            //    TechCharge();

            //Avoids repetedly attacking when holding mouse button 
            if (clicked)
                return;

            UseMartialArt(t as MartialArt);
        }

        else if (special != null)
        {
            if (special.energyCost > stats.GetEnergy ||
                    special.healthCost > stats.GetHealth ||
                    special.staminaCost > stats.GetStamina)
                return;

            if (special.chargable)
                TechCharge();
            else if (anim.GetCurrentAnimatorStateInfo(1).IsName("New State"))
                AnimateAttack(special);
        }

        clicked = true;
    }

    /// <summary>
    /// Called from States that allow Main Mouse Button to be pressed
    /// </summary>
    public void MouseReleaseMain()
    {
        clicked = false;

        Technique t = GetSelected;
        MartialArt martial = t as MartialArt;
        SpecialArt special = t as SpecialArt;

        if (martial)
        {
            ExitTechCharge(false);
        }
        else if (special)
        {
            if (techChargeTimer >= special.minChargeTime)
            {
                ExitTechCharge(true);
                AnimateAttack(special);
                if (special.type == HitType.Beam)
                {
                    character.SetState<BeamState>();
                    character.GetCurrentState.BroadcastMessage("InitializeBeam", special);
                }
            }
            else
            {
                ExitTechCharge(false);                
            }
        }
        
    }

    /// <summary>
    /// Sets Animator state to the charging Animation Clip of the Technique
    /// </summary>
    private void AnimateCharge(SpecialArt t)
    {
        anim.SetInteger("ChargeAnim", t.chargeAnimation);
        anim.SetBool("ChargingAttack", true);
    }

    /// <summary>
    /// Sets Animator state to the attack Animation Clip of the Technique
    /// </summary>
    private void AnimateAttack(SpecialArt t)
    {
        attackAnimating = t;

        anim.SetInteger("AttackAnim", t.attackAnimation);

        if (anim.GetBool("ChargingAttack"))
            anim.SetBool("ChargingAttack", false);

        anim.SetBool("FiringAttack", true);

        if (t.type == HitType.Beam)
            character.SetState<BeamState>();
    }

    /// <summary>
    /// Called by an Animation Event Trigger and sets Projectile origin to left hand.
    /// </summary>
    public void FireAttackLeft()
    {
        currentProjectileSpot = leftProjectileSpot;
        UseTechnique(attackAnimating);

        attackAnimating = null;
    }

    /// <summary>
    /// Called by an Animation Event Trigger and sets Projectile origin to right hand.
    /// </summary>
    public void FireAttackRight()
    {
        currentProjectileSpot = rightProjectileSpot;
        UseTechnique(attackAnimating);

        attackAnimating = null;
    }

    /// <summary>
    /// Called by an Animation Event Trigger as a part of multiple attacks and sets Projectile origin to left hand.
    /// </summary>
    public void FireAttackPartLeft()
    {
        currentProjectileSpot = leftProjectileSpot;
        UseTechnique(attackAnimating);
    }

    /// <summary>
    /// Called by an Animation Event Trigger as a part of multiple attacks and sets Projectile origin to right hand.
    /// </summary>
    public void FireAttackPartRight()
    {
        currentProjectileSpot = rightProjectileSpot;
        UseTechnique(attackAnimating);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ExitAnimationCheck()
    {
        if (beamHead != null)
            return;

        if (anim.GetBool("FiringAttack"))
        {
            anim.SetBool("FiringAttack", false);
        }
    }

    /// <summary>
    /// Called by an Animation Event Trigger at the start of each melee animation.
    /// </summary>
    public void DoneAnim()
    {
        anim.SetBool("DoneAnim", true);

        currentMove = _nextMove;

        stats.UpdateHealth(-currentMove.healthCost);
        stats.UpdateStamina(-currentMove.staminaCost);
        stats.UpdateEnergy(-currentMove.energyCost);
    }

    public void ResetDoneAnim()
    {
        anim.SetBool("DoneAnim", false);
    }

    public void UpdateUI()
    {
        if (vi.localPlayer == false)
            return;

        if (chargeParent == false)
        {
            Debug.LogWarning("Missing UI element in :: " + this + " :: chargeParent");
            return;
        }

        SpecialArt art = GetSelected as SpecialArt;
        if (art != null)
        {
            chargeParent.SetActive(isChargingTech && art.minChargeTime > 0);

            chargeBar.fillAmount = techChargeTimer / art.minChargeTime;
            overCharge.fillAmount = (techChargeTimer - art.minChargeTime) / (art.fullChargeTime - art.minChargeTime);

            if (minCharge != null)
            {
                float x = art.minChargeTime / art.fullChargeTime * emptyBar.rectTransform.rect.width;
                minCharge.rectTransform.anchoredPosition = new Vector2(x, 0);
            }
        }
    }
}
