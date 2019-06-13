using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Instantiated")]
    public GameObject chargeObject;
    public GameObject beamHead;
    public float beamStartTime;

    [Header("Particle Systems")]
    public ParticleSystem fullCharge;
    public bool playedFullCharge;

    [Header("Melee")]
    public bool canClick = true;
    public int punchNum;
    public Move currentMove;

    [Header("Techniques")]
    public List<Technique> techniques;
    public int activeSlot = 1;

    public float techChargeTimer;
    public float lastTechChargeTimer;

    private int healthHolder;
    private int energyHolder;
    private int staminaHolder;

    public Technique GetSelected
    {
        get
        {
            //if (this.techniques[activeSlot - 1] is MartialArt)
            //    return GetCurrentMove;
            //else
                return this.techniques[activeSlot - 1];
        }
    }

    private Move GetCurrentMove { get { return (this.techniques[activeSlot - 1] as MartialArt).moveArray[punchNum]; } }

    [Header("Animation")]
    public Technique attackAnimating;

    [Header("Conditions")]
    public bool isChargingTech;
    public bool isFiringBeam;


    public bool clicked;

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

    }

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

        switch (t.type)
        {
            case HitType.Projectile:

                if (chargeObject != null)
                    Destroy(chargeObject);

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
                    po.damage = t.damage;
                    po.speed = t.speed;
                    po.blowBack = t.blowBackForce;
                    po.owner = character;
                    po.target = character.ts.lockOn ? character.ts.lockOn : null;
                }

                break;
            case HitType.Beam:

                if (chargeObject != null)
                    Destroy(chargeObject);

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
                    po.owner = character;
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
            currentMove = m.dashAttack;
            character.SetState<ChargeAttackState>();
            Debug.Log("true");
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

                currentMove = m.moveArray[punchNum - 1];

                stats.UpdateHealth(-currentMove.healthCost);
                stats.UpdateStamina(-currentMove.staminaCost);
                stats.UpdateEnergy(-currentMove.energyCost);

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

    #region Melee

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
        //Debug.Log("Called reset");
        punchNum = 0;
        anim.SetInteger("Combo", punchNum);
        EnableClick();
    }

    #endregion


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
                if (camScript && camScript.view != ThirdPersonCam.camView.RightZoomBeam)
                    camScript.TransitionView(ThirdPersonCam.camView.RightZoomBeam);

            if (anim.GetBool("ChargingAttack") == false)
                AnimateCharge(t as SpecialArt);
        }

        #endregion
    }

    public void ExitTechCharge(bool executingAttack)
    {
        isChargingTech = false;

        UpdateUI();

        #region Visual

        anim.SetBool("ChargingAttack", false);
        anim.SetBool("ChargePunch", false);
        anim.SetBool("ChargeKick", false);

        if (camScript && camScript.view != ThirdPersonCam.camView.InstantFront)
            camScript.TransitionView(ThirdPersonCam.camView.TransitionFront);

        #endregion

        lastTechChargeTimer = techChargeTimer;
        techChargeTimer = 0f;
        playedFullCharge = false;

        if (!executingAttack && chargeObject)
            Destroy(chargeObject);

        healthHolder = staminaHolder = energyHolder = 0;
    }

    public void ExitBeamMode()
    {
        if(beamHead)
        {
            TrailRenderer tr = beamHead.GetComponent<TrailRenderer>();
            tr.time = Time.time - beamStartTime;
            tr.endWidth = 0f;
            beamHead = null;
        }
        ExitAnimationCheck();
    }

    public void MousePressMain()
    {
        Technique t = GetSelected;
        MartialArt martial = t as MartialArt;
        SpecialArt special = t as SpecialArt;

        if (martial != null)
        {
            //if ((t as MartialArt).moveArray[(punchNum + 1) % (t as MartialArt).moveArray.Length].chargable)
            //    TechCharge();
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
            {
                AnimateAttack(special);
            }
        }

        clicked = true;
    }

    public void MouseReleaseMain()
    {
        clicked = false;

        Technique t = GetSelected;

        if (t is MartialArt)
        {
            ExitTechCharge(false);
            //UseMartialArt(t as MartialArt);
        }
        else if (t is SpecialArt)
        {
            if (techChargeTimer >= (t as SpecialArt).minChargeTime)
            {
                ExitTechCharge(true);
                AnimateAttack(t as SpecialArt);
                if ((t as SpecialArt).type == HitType.Beam)
                    character.SetState<BeamState>();
            }
            else
            {
                stats.UpdateHealth(healthHolder);
                stats.UpdateEnergy(energyHolder);
                stats.UpdateStamina(staminaHolder);

                ExitTechCharge(false);                
            }
        }
        
    }

    void AnimateCharge(SpecialArt t)
    {
        anim.SetInteger("ChargeAnim", t.chargeAnimation);
        anim.SetBool("ChargingAttack", true);
    }

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

    public void FireAttackLeft()
    {
        currentProjectileSpot = leftProjectileSpot;
        UseTechnique(attackAnimating);

        //attackStandby = false;
        attackAnimating = null;
    }

    public void FireAttackRight()
    {
        currentProjectileSpot = rightProjectileSpot;
        UseTechnique(attackAnimating);

        //attackStandby = false;
        attackAnimating = null;
    }

    public void FireAttackPartLeft()
    {
        currentProjectileSpot = leftProjectileSpot;
        UseTechnique(attackAnimating);
    }

    public void FireAttackPartRight()
    {
        currentProjectileSpot = rightProjectileSpot;
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

    public void DoneAnim()
    {
        anim.SetBool("DoneAnim", true);
    }

    public void ResetDoneAnim()
    {
        anim.SetBool("DoneAnim", false);
    }

    public void UpdateUI()
    {
        if (!vi.localPlayer)
            return;

        if (!chargeParent)
            return;

        SpecialArt art = GetSelected as SpecialArt;
        if (art != null)
        {
            chargeParent.SetActive(isChargingTech && art.minChargeTime > 0);

            chargeBar.fillAmount = techChargeTimer / art.fullChargeTime;
            float x = art.minChargeTime / art.fullChargeTime * emptyBar.rectTransform.rect.width;
            minCharge.rectTransform.anchoredPosition = new Vector2(x, 0);
        }
    }
}
