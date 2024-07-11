using Controller2k;
using Google.Protobuf.WellKnownTypes;
using Mirror;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Collections;
using static UMA.RaceData;
using UnityEngine.EventSystems;
using GameZero;
using System;
public class battlescripts : NetworkBehaviour
{
    //npc teleporter
        [Header("Npc teleporter")]
        

    public static int partyId = -1; // initialize to -1 to indicate that it is not set yet
    private static Player creator;
    private Vector3 targetPos;
    public static battlescripts singleton;

    [Header("charge")]
    public float stopDistance;
    public Vector3 goal;
    public string buffName;
    public Buff buff;
    public GameObject jumplungeprefab;
    public GameObject whirlwindprefab;
    public GameObject frontflipprefab;
    public GameObject swingforwardprefab;
    public GameObject firegridprefab;
    public GameObject overheadprefab;

    public PlayerCharacterControllerMovement pccm;
    public GameObject trail;
    public float dashSpeed = 5f; // Adjust the speed to make it slower
    public float dashDuration = 0.5f; // Duration of the dash
    public float jumplungeDuration = 0.5f; // Duration of the dash
    public float gravity = -9.81f;
    private Vector3 velocity;
    private Vector3 moveDirection = Vector3.zero;
    public CharacterController2k controller;
    public Equipment equipable;
    public Player player;
    public Animator animator;
    // public string mainHandidleAni;
    // public string offHandidleAni;
    [Header("battlestances")]
    public bool rootmotion;
    public bool twoMainHandswordAIMnoOH;
    public bool twoOffHandswordAIMnoOH;
    public bool twoMainHandspearAIMnoOH;
    public bool twoMainHandmaceAIMnoOH;
    public bool twoOffHandmaceAIMnoOH;
    public bool twoMainHandaxeAIMnoOH;
    public bool twoOffHandaxeAIMnoOH;
    public bool twoMainHandgunAIMnoOH;
    public bool twoOffHandgunAIMnoOH;
    public bool twoMainHandbowAIMnoOH;
    public bool unarmedAIM;

    private Entity entity;

    public aoedamageskillstart aoestart;
    
    public TargetProjectileSkill rapidfire;
    public Entity caster;
    public int skillLevel;
    private Transform casterTransform;
    [Header("mousemanager")]
    [SyncVar] public int currentItemIndex;
    [SyncVar] public int skillIndex;
    [SyncVar] public bool isTargeting;
    [SyncVar] public Vector3 storedMousePosition;

    public Texture2D defaultCursor;
    public Texture2D targetcursor;
    public Texture2D attackCursor;
    private CursorMode cursorMode = CursorMode.ForceSoftware;
    private Vector2 hotSpot = Vector2.zero;
    private Vector2 defualtSpot = new Vector2(18f, 5f);
    private Vector2 targetSpot = new Vector2(30f, 30f);
    public Vector2 attackSpot = new Vector2(30f, 30f);

    public GameObject aoeCanvas;
    public Projector targetIndicator;

    public int layerMask = 9;

    public Vector3 mouseHitPosition;
    public Vector3 posUp;
    public float maxAbility2Distance;

    void Awake()
    {

        // initialize singleton
        if (singleton == null) singleton = this;
    }
    public void FixedUpdate()
    {

        if (player.charging == true)
        {
            DashMovement(buffName, stopDistance);

        }
    }
    public void DashMovement(string buffName, float stopDistance)
    {
        if (player.target != null)
        {
            Vector3 goal = player.target.transform.position;
            int index = player.skills.GetBuffIndexByName(buffName);

            player.charging = true;
            animator.SetBool("chargeattack", true);
            player.transform.position = Vector3.MoveTowards(player.transform.position, goal, player.speed * Time.fixedDeltaTime);
            player.transform.LookAt(goal);

            if (Vector3.Distance(player.transform.position, goal) <= stopDistance)
            {
                if (index != -1)
                {
                    player.skills.buffs.RemoveAt(index);
                }
                player.charging = false;
                animator.SetBool("chargeattack", false);

            }
        }
    }
    public void startswing2()
    {
        StartCoroutine(swing2());
    }
    public void endswing2()
    {
        StopCoroutine(swing2());
    }
    IEnumerator swing2()
    {
        yield return new WaitForSeconds(.5f);
        Debug.Log("attack 1: " + Time.time);
        aoestart.swingcast2(caster, skillLevel);
        yield return new WaitForSeconds(.5f);
        Debug.Log("attack 2: " + Time.time);
        aoestart.swingcast2(caster, skillLevel);
        yield return new WaitForSeconds(.5f);
        Debug.Log("attack 3: " + Time.time);
        aoestart.swingcast2(caster, skillLevel);
        yield return new WaitForSeconds(.75f);
        Debug.Log("attack 3: " + Time.time);
        aoestart.swingcast2(caster, skillLevel);


    }
    public void startswing()
    {
        StartCoroutine(swingAttack());
    }
    public void endswing()
    {
        StopCoroutine(swingAttack());
    }
    IEnumerator swingAttack()
    {
        yield return new WaitForSeconds(.5f);
        Debug.Log("attack 1: " + Time.time);
        aoestart.swingcast(caster, skillLevel);
        yield return new WaitForSeconds(1f);
        Debug.Log("attack 2: " + Time.time);
        aoestart.swingcast(caster, skillLevel);
        yield return new WaitForSeconds(.5f);
        Debug.Log("attack 3: " + Time.time);
        aoestart.swingcast(caster, skillLevel);
        yield return new WaitForSeconds(.75f);
        Debug.Log("attack 3: " + Time.time);
        aoestart.swingcast(caster, skillLevel);
        

    }

    public void startoverhead()
    {
        StartCoroutine(overheadAttack());
    }
    public void endoverhead
        ()
    {
        StopCoroutine(overheadAttack());
    }
    IEnumerator overheadAttack()
    {
        yield return new WaitForSeconds(.25f);
        Debug.Log("attack 1: " + Time.time);
        aoestart.overheadcast(caster, skillLevel);
        yield return new WaitForSeconds(.5f);
        Debug.Log("attack 2: " + Time.time);
        aoestart.overheadcast(caster, skillLevel);
        yield return new WaitForSeconds(1f);
        Debug.Log("attack 3: " + Time.time);
        aoestart.overheadcast(caster, skillLevel);


    }

    IEnumerator rapidfireAttack()
    {
        Debug.Log("attack 1: " + Time.time);
        rapidfire.Apply(caster, skillLevel);
        yield return new WaitForSeconds(.1f);
        Debug.Log("attack 2: " + Time.time);
        rapidfire.Apply(caster, skillLevel);
        yield return new WaitForSeconds(.1f);
        Debug.Log("attack 3: " + Time.time);
        rapidfire.Apply(caster, skillLevel);
        yield return new WaitForSeconds(.1f);
        Debug.Log("attack 4: " + Time.time);
        rapidfire.Apply(caster, skillLevel);
        yield return new WaitForSeconds(.1f);
        Debug.Log("attack 5: " + Time.time);
        rapidfire.Apply(caster, skillLevel);
        yield return new WaitForSeconds(.1f);
        Debug.Log("attack 6: " + Time.time);
        rapidfire.Apply(caster, skillLevel);
        yield return new WaitForSeconds(.1f);

    }

    public void startrapidfire()
    {
        StartCoroutine(rapidfireAttack());
    }
    public void endrapidfire
        ()
    {
        StopCoroutine(rapidfireAttack());
    }


    public IEnumerator aoeAttack()
    {
       
        Debug.Log("attack 1: " + Time.time);
        aoestart.cyclonecast(caster, skillLevel);
        yield return new WaitForSeconds(.2f);
        Debug.Log("attack 2: " + Time.time);
        aoestart.cyclonecast(caster, skillLevel);
        yield return new WaitForSeconds(.2f);
        Debug.Log("attack 3: " + Time.time);
        aoestart.cyclonecast(caster, skillLevel);
    }

    public void startaoe()
    {
        StartCoroutine(aoeAttack());
    }

    public void endaoe()
    {
        StopAllCoroutines();
    }
    public void clearstances()
    {
        foreach (Animator anim in GetComponentsInChildren<Animator>())
        {
            anim.SetBool("twoMainHandswordAIMnoOH", false);
            anim.SetBool("twoOffHandswordAIMnoOH", false);
            anim.SetBool("oneMainHandswordAIMnoOH", false);
            anim.SetBool("oneOffHandswordAIMnoOH", false);
            anim.SetBool("twoMainHandspearAIMnoOH", false);
            anim.SetBool("twoOffHandspearAIMnoOH", false);
            anim.SetBool("twoMainHandmaceAIMnoOH", false);
            anim.SetBool("twoOffHandmaceAIMnoOH", false);
            anim.SetBool("twoMainHandaxeAIMnoOH", false);
            anim.SetBool("twoOffHandaxeAIMnoOH", false);
            anim.SetBool("twoMainHandgunAIMnoOH", false);
            anim.SetBool("twoOffHandgunAIMnoOH", false);
            anim.SetBool("twoMainHandbowAIMnoOH", false);
            anim.SetBool("AIMING", false);
        }
    }

    public void CloseAll()
    {
        foreach (Animator animator in GetComponentsInChildren<Animator>())
        {
            animator.SetBool("closeLeft", true);
            animator.SetBool("closeRight", true);
        }
    }

    public void OpenAll()
    {
        foreach (Animator animator in GetComponentsInChildren<Animator>())
        {
            animator.SetBool("OpenLeft", true);
            animator.SetBool("OpenRight", true);
        }
    }

    public void CloseLeft()
    {
        foreach (Animator animator in GetComponentsInChildren<Animator>())
        {
            animator.SetBool("closeLeft", true);
        }
    }

    public void CloseRight()
    {
        foreach (Animator animator in GetComponentsInChildren<Animator>())
        {
            animator.SetBool("closeRight", true);
        }
    }

    public void OpenLeft()
    {
        foreach (Animator animator in GetComponentsInChildren<Animator>())
        {
            animator.SetBool("OpenLeft", true);
        }
    }

    public void OpenRight()
    {
        foreach (Animator animator in GetComponentsInChildren<Animator>())
        {
            animator.SetBool("OpenRight", true);
        }
    }
    public void Start()
    {

        Cursor.SetCursor(defaultCursor, defualtSpot, cursorMode);
        setDefaultCursor();

        if (player.isLocalPlayer)
        {
            targetIndicator.enabled = false;
        }



        entity = GetComponent<Entity>();
        trail.SetActive(false);

        OpenAll();
    }
    public void Update()
    {
        
        if (Player.localPlayer == null) return;

        if (Player.localPlayer.target != null && Player.localPlayer.target.tag == "Monster")
        {
            monster_selected();
        }
        else if (Player.localPlayer.target != null)
        {
            if (Player.localPlayer.target.tag != "Monster")
            {
                monster_not_selected();
            }
        }
        else
        {
            nothing_selected();
        }

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            if (hit.collider.gameObject != this.gameObject)
            {
                posUp = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                mouseHitPosition = hit.point;
            }
        }

        Quaternion transRot = Quaternion.LookRotation(mouseHitPosition - player.transform.position);

        var hitPosDir = (mouseHitPosition - transform.position).normalized;
        float distance = Vector3.Distance(mouseHitPosition, transform.position);
        distance = Mathf.Min(distance, maxAbility2Distance);

        var newHitPos = transform.position + hitPosDir * distance;
        aoeCanvas.transform.position = newHitPos;
    }
    public void monster_selected()
    {
        //if (player != null)
        //{
            //Debug.Log("player is not null");
            // only call this for server and for local player. not for other
            // players on the client. no need in locally creating their
            // instances too.
            if (player.isLocalPlayer)
            {
                // Debug.Log("monster selected");
                // Debug.Log("Equipped Weapon: " + player.equipment.GetEquippedWeaponType());
                // Debug.Log("Hands: " + player.equipment.GetHands());

                Animator anim = GetComponentInChildren<Animator>();
                anim.SetBool("AIMING", true);

                if (player.equipment.slots[10].amount == 0 && player.equipment.slots[11].amount != 0)
                {

                    if (player.equipment.GetHands() == ScriptableItem.HandsRequired.NONE)
                    {
                        //Debug.Log("No weapons equip");




                    }
                    else if (player.equipment.GetHands() == ScriptableItem.HandsRequired.ONE_HANDED)
                    {
                        // Debug.Log("1h equp");

                        switch (player.equipment.GetEquippedWeaponType())
                        {
                            case ScriptableItem.WeaponType.Fist:
                                {
                                    Debug.Log("Its a 1h fist");
                                    anim.SetBool("oneMainHandfistAIMnoOH", true);
                                    anim.SetBool("oneMainHandfistAIMnoOH", true);
                                    break;
                                }
                            case ScriptableItem.WeaponType.Sword:
                                {
                                    Debug.Log("Its a 1h sword!");
                                    anim.SetBool("oneMainHandswordAIMnoOH", true);
                                    anim.SetBool("oneOffHandswordAIMnoOH", true);
                                    break;
                                }
                            case ScriptableItem.WeaponType.Dagger:
                                {
                                    Debug.Log("Its a 1h dagger!");
                                    anim.SetBool("oneMainHanddaggerAIMnoOH", true);
                                    anim.SetBool("oneOffHanddaggerAIMnoOH", true);
                                    break;
                                }
                            case ScriptableItem.WeaponType.Axe:
                                {
                                    Debug.Log("Its a 1h axe!");
                                    anim.SetBool("oneMainHandaxeAIMnoOH", true);
                                    anim.SetBool("oneOffHandaxeAIMnoOH", true);
                                    break;
                                }
                            case ScriptableItem.WeaponType.Mace:
                                {
                                    Debug.Log("Its a 1h mace!");
                                    anim.SetBool("oneMainHandmaceAIMnoOH", true);
                                    anim.SetBool("oneOffHandmaceAIMnoOH", true);
                                    break;
                                }
                            case ScriptableItem.WeaponType.Shield:
                                {
                                    Debug.Log("Its a shield!");
                                    anim.SetBool("shieldAIMnoOH", true);
                                    anim.SetBool("shieldAIMnoOH", true);
                                    break;
                                }
                        }

                    }

                    else if (player.equipment.GetHands() == ScriptableItem.HandsRequired.TWO_HANDED)
                    {
                        // Debug.Log("2h equip equip");

                        switch (player.equipment.GetEquippedWeaponType())
                        {
                            case ScriptableItem.WeaponType.Spear:
                                {
                                    CloseRight(); CloseLeft();
                                    Debug.Log("Its a 2h spear");
                                    anim.SetBool("twoMainHandspearAIMnoOH", true);
                                    anim.SetBool("twoOffHandspearAIMnoOH", true);
                                    break;

                                }

                            case ScriptableItem.WeaponType.Sword:
                                {
                                    CloseRight();
                                    Debug.Log("Its a 2h sword!");
                                    anim.SetBool("twoMainHandswordAIMnoOH", true);
                                    anim.SetBool("twoOffHandswordAIMnoOH", true);
                                    break;

                                }
                            case ScriptableItem.WeaponType.Gun:
                                {
                                    CloseRight(); CloseLeft();
                                    Debug.Log("Its a 2h Gun!");
                                    anim.SetBool("twoMainHandgunAIMnoOH", true);
                                    anim.SetBool("twoOffHandgunAIMnoOH", true);


                                    break;

                                }
                            case ScriptableItem.WeaponType.Axe:
                                {
                                    CloseRight(); CloseLeft();
                                    Debug.Log("Its a 2h axe!");
                                    anim.SetBool("twoMainHandaxeAIMnoOH", true);
                                    anim.SetBool("twoOffHandaxeAIMnoOH", true);
                                    break;
                                }
                            case ScriptableItem.WeaponType.Mace:
                                {
                                    CloseRight(); CloseLeft();
                                    Debug.Log("Its a 2h mace!");
                                    anim.SetBool("twoMainHandmaceAIMnoOH", true);
                                    anim.SetBool("twoOffHandmaceAIMnoOH", true);
                                    break;
                                }


                            case ScriptableItem.WeaponType.Bow:
                                {
                                    Debug.Log("Its a 2h bow!");
                                    anim.SetBool("twoMainHandbowAIMnoOH", true);
                                    anim.SetBool("twoOffHandbowAIMnoOH", true);
                                    break;
                                }
                        }
                    }


                }

                if (player.equipment.slots[10].amount != 0 && player.equipment.slots[11].amount == 0)
                {

                    if (player.equipment.GetHands() == ScriptableItem.HandsRequired.NONE)
                    {
                        // Debug.Log("No weapons equip");




                    }
                    else if (player.equipment.GetHands() == ScriptableItem.HandsRequired.ONE_HANDED)
                    {
                        // Debug.Log("1h equp");

                    // yeah I'm making a weapon id addon, I though it was available already this is not manageable, should just be 1 bool and a integer pointing to what weapon type.
                    // if dual wield simply have two of them, L_HAND_WEAPONID = 0 R_HAND_WEAPONID = 4 (2H SWORD) etc, then noOH? set to true, done.. then all this code would be 4 lines, can you do that real fast? haha, no I need to setup the animator and things in order for that so not today, I need to go back to work, ok the sheathing should work pretty good though ya?  yeah 100% it's synced and everything 100%. sweet I'll build it and test some stuff out on the network today, cool! now I have to get back to work ^^
                        switch (player.equipment.GetEquippedWeaponType())
                        {
                            case ScriptableItem.WeaponType.Fist:
                                {
                                    // Debug.Log("Its a 1h fist");
                                    anim.SetBool("oneMainHandfistAIMnoOH", true);
                                    anim.SetBool("oneMainHandfistAIMnoOH", true);
                                    break;
                                }
                            case ScriptableItem.WeaponType.Sword:
                                {
                                    //Debug.Log("Its a 1h sword!");
                                    anim.SetBool("oneMainHandswordAIMnoOH", true);
                                    anim.SetBool("oneOffHandswordAIMnoOH", true);
                                    break;
                                }
                            case ScriptableItem.WeaponType.Dagger:
                                {
                                    //Debug.Log("Its a 1h dagger!");
                                    anim.SetBool("oneMainHanddaggerAIMnoOH", true);
                                    anim.SetBool("oneOffHanddaggerAIMnoOH", true);
                                    break;
                                }
                            case ScriptableItem.WeaponType.Axe:
                                {
                                    //Debug.Log("Its a 1h axe!");
                                    anim.SetBool("oneMainHandaxeAIMnoOH", true);
                                    anim.SetBool("oneOffHandaxeAIMnoOH", true);
                                    break;
                                }
                            case ScriptableItem.WeaponType.Mace:
                                {
                                    //Debug.Log("Its a 1h mace!");
                                    anim.SetBool("oneMainHandmaceAIMnoOH", true);
                                    anim.SetBool("oneOffHandmaceAIMnoOH", true);
                                    break;
                                }
                        }

                    }
                    else if (player.equipment.GetHands() == ScriptableItem.HandsRequired.TWO_HANDED)
                    {
                        // Debug.Log("2h equip equip");

                        switch (player.equipment.GetEquippedWeaponType())
                        {
                            case ScriptableItem.WeaponType.Spear:
                                {
                                    CloseRight(); CloseLeft();
                                    // Debug.Log("Its a 2h spear");
                                    anim.SetBool("twoMainHandspearAIMnoOH", true);
                                    anim.SetBool("twoOffHandspearAIMnoOH", true);
                                    break;

                                }
                            case ScriptableItem.WeaponType.Mace:
                                {
                                    CloseRight(); CloseLeft();
                                    // Debug.Log("Its a 2h spear");
                                    anim.SetBool("twoMainHandmaceAIMnoOH", true);
                                    anim.SetBool("twoOffHandmaceAIMnoOH", true);
                                    break;

                                }

                            case ScriptableItem.WeaponType.Sword:
                                {
                                    CloseRight();
                                    //Debug.Log("Its a 2h sword!");
                                    anim.SetBool("twoMainHandswordAIMnoOH", true);
                                    anim.SetBool("twoOffHandswordAIMnoOH", true);

                                    break;

                                }
                            case ScriptableItem.WeaponType.Gun:
                                {
                                    CloseRight(); CloseLeft();
                                    // Debug.Log("Its a 2h Gun!");
                                    anim.SetBool("twoMainHandgunAIMnoOH", true);
                                    anim.SetBool("twoOffHandgunAIMnoOH", true);


                                    break;

                                }
                            case ScriptableItem.WeaponType.Axe:
                                {
                                    CloseRight(); CloseLeft();
                                    // Debug.Log("Its a 2h axe!");
                                    anim.SetBool("twoMainHandaxeAIMnoOH", true);
                                    anim.SetBool("twoOffHandaxeAIMnoOH", true);
                                    break;
                                }
                            case ScriptableItem.WeaponType.Bow:
                                {
                                    // Debug.Log("Its a 2h bow!");
                                    anim.SetBool("twoMainHandbowAIMnoOH", true);
                                    anim.SetBool("twoOffHandbowAIMnoOH", true);
                                    break;
                                }
                        }
                    }


                }

                if (player.equipment.slots[10].amount == 0 && player.equipment.slots[11].amount != 0)
                {
                    foreach (Animator animator in GetComponentsInChildren<Animator>())
                    {
                        // do something
                    }
                }
                else if (player.equipment.slots[10].amount != 0 && player.equipment.slots[11].amount != 0)
                {
                    foreach (Animator animator in GetComponentsInChildren<Animator>())
                    {
                        // do something
                    }
                }
                else if (player.equipment.slots[10].amount == 0 && player.equipment.slots[11].amount == 0)
                {
                    foreach (Animator animator in GetComponentsInChildren<Animator>())
                    {
                        // do something
                    }
                }

                if (player.equipment.slots[10].amount == 0)
                {
                    OpenRight();
                }

                if (player.equipment.slots[11].amount == 0)
                {
                    OpenLeft();
                }
                
                if (Player.localPlayer.state != "MOUNTED" ||
                    Player.localPlayer.state == "MOVING" ||
                    Player.localPlayer.state == "CASTING" ||
                    Player.localPlayer.state == "STUNNED" ||
                    Player.localPlayer.state == "Autoattack" ||
                    Player.localPlayer.state == "Strong Hit" ||
                    Player.localPlayer.state == "aoedamageskill" ||
                    Player.localPlayer.state == "frontflip" ||
                    Player.localPlayer.charging ||
                    Player.localPlayer.state == "Fireball" ||
                    Player.localPlayer.state == "Fireblast")
                {
                    clearstances();
                }

            //}
        }

    }
    public void monster_not_selected()
    {
        clearstances();

    }
    public void nothing_selected()
    {
        clearstances();
    }
    public void applyswingforward()
    {
        startswing();
        animator.applyRootMotion = true;
        StartCoroutine(swingforwardtimer());

    }
    IEnumerator swingforwardtimer()
    {
        yield return new WaitForSeconds(4f);
        animator.applyRootMotion = false;
    }
    IEnumerator swing2timer()
    {
        yield return new WaitForSeconds(3f);
        animator.applyRootMotion = false;
    }
    public void applyswing2()
    {
        startswing2();
        animator.applyRootMotion = true;
        StartCoroutine(swing2timer());

    }
    
    public void applyjumplunge()
    {


        trail.SetActive(true);




        animator.applyRootMotion = true;

        StartCoroutine(jumplungetimer());

    }
    IEnumerator jumplungetimer()
    {
        yield return new WaitForSeconds(.5f);
        float elapsedTime = 0f;
        Vector3 dashDirection = transform.forward; // Capture the current forward direction

        Debug.Log("Dash Direction: " + dashDirection); // Log the dash direction to check if it's correct

        while (elapsedTime < jumplungeDuration)
        {
            // Calculate dash movement
            Vector3 dashMove = dashDirection * dashSpeed * Time.deltaTime;

            // Apply gravity
            if (!controller.isGrounded)
            {
                velocity.y += gravity * Time.deltaTime;
            }

            // Move the character
            controller.Move(dashMove + velocity * Time.deltaTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        animator.applyRootMotion = false;
        trail.SetActive(false);
    }

    public void applyoverhead()
    {


        //trail.SetActive(true);




        //animator.applyRootMotion = true;
        startoverhead();
        StartCoroutine(overheadtimer());

    }
    IEnumerator overheadtimer()
    {
        yield return new WaitForSeconds(.5f);
        float elapsedTime = 0f;
        Vector3 dashDirection = transform.forward; // Capture the current forward direction

        Debug.Log("Dash Direction: " + dashDirection); // Log the dash direction to check if it's correct

        while (elapsedTime < jumplungeDuration)
        {
            // Calculate dash movement
            Vector3 dashMove = dashDirection * dashSpeed * Time.deltaTime;

            // Apply gravity
            if (!controller.isGrounded)
            {
                velocity.y += gravity * Time.deltaTime;
            }

            // Move the character
            //controller.Move(dashMove + velocity * Time.deltaTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        //animator.applyRootMotion = false;
        //trail.SetActive(false);
    }
    public void applydashforward()
    {


        trail.SetActive(true);




        animator.applyRootMotion = true;
        
        StartCoroutine(dashforwardtimer());

    }
    IEnumerator dashforwardtimer()
    {
        float elapsedTime = 0f;
        Vector3 dashDirection = transform.forward; // Capture the current forward direction

        Debug.Log("Dash Direction: " + dashDirection); // Log the dash direction to check if it's correct

        while (elapsedTime < dashDuration)
        {
            // Calculate dash movement
            Vector3 dashMove = dashDirection * dashSpeed * Time.deltaTime;

            // Apply gravity
            if (!controller.isGrounded)
            {
                velocity.y += gravity * Time.deltaTime;
            }

            // Move the character
            controller.Move(dashMove + velocity * Time.deltaTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        animator.applyRootMotion = false;
        trail.SetActive(false);
    }
    public void applyinvisibility()
    {

        pccm.state = MoveState.SNEAKING;


        StartCoroutine(invisibilitytimer());

    }
    IEnumerator invisibilitytimer()
    {
        yield return new WaitForSeconds(30f);
        pccm.state = MoveState.IDLE;

    }

    [Command(requiresAuthority = false)]
    public void CmdLobbyCreator()
    {
        Debug.Log("Player assigned: " + player.name);
        if (player != null)
        {
            PartySystem.FormSoloParty(player.name);
            partyId = player.party.party.partyId;
            Debug.Log($"partyId = {partyId}");
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdLobbyJoiner()
    {
        if (player != null)
        {
            PartySystem.AddToParty(partyId, player.name);
        }
    }





    [Command(requiresAuthority = false)]
    public void CmdPartySetup()
    {
        //ArenaManager am = GameObject.FindWithTag("GameManager").GetComponent<ArenaManager>();
        Instance instanceTemplate = ArenaManager.singleton.GetInstanceTemplate(ArenaManager.singleton.instanceId);

        Debug.Log("Cmdpartysetup called");
        Debug.Log("instanceTemplate = " + instanceTemplate);
        Player[] playersInParty = PartySystem.GetPlayersInParty(partyId);
        Debug.Log("Number of players in party: " + playersInParty.Length);
        foreach (Player player in playersInParty)
        {
            Debug.Log("player name = " + player.name);


            // loop through each player and do something with them
            Debug.Log("player isServer: " + player.isServer + " hasAuthority: " + player.isOwned + " isClient: " + player.isClient + " isLocalPlayer: " + player.isLocalPlayer);


            // collider might be in player's bone structure. look in parents.

            if (player != null)
            {
                Debug.Log("player is not null");
                // only call this for server and for local player. not for other
                // players on the client. no need in locally creating their
                // instances too.
                if (player.isServer || player.isLocalPlayer)
                {
                    Debug.Log("player is server or local player");
                    // required level?
                    if (player.level.current >= instanceTemplate.requiredLevel)
                    {
                        Debug.Log("player met level requirement");
                        // can only enter with a party
                        if (player.party.InParty())
                        {
                            Debug.Log("player is in a party");
                            // is there an instance for the player's party yet?
                            if (instanceTemplate.instances.TryGetValue(partyId, out Instance existingInstance))
                            {
                                // teleport player to instance entry
                                if (player.isServer)
                                {
                                    Debug.Log("player has authority!!");
                                    Vector3 entry1Pos = existingInstance.entry1?.position ?? Vector3.zero;
                                    Vector3 entry2Pos = existingInstance.entry2?.position ?? Vector3.zero;

                                    // Determine the target position for the player based on the player index

                                    int playerIndex = Array.IndexOf(playersInParty, player);
                                    Vector3 targetPos = (playerIndex % 2 == 0) ? entry2Pos : entry1Pos;

                                    // Call the CmdWarpToEntry() method to move the player to the target position
                                    if (player.isServer || (player.isClient && player.isOwned))
                                    {
                                        // Call the CmdWarpDrive() method only if the player is owned by a client
                                        player.movement.Warp(targetPos);
                                    }
                                    Debug.Log("Teleporting " + player.name + " to existing instance=" + existingInstance.name + " with partyId=" + partyId);
                                }

                            }
                            // otherwise create a new one
                            else
                            {
                                Instance instance = Instance.CreateInstance(instanceTemplate, player.party.party.partyId);
                                NetworkServer.Spawn(instance.gameObject);
                                if (instance != null)
                                {
                                    Debug.Log("instance is not null");
                                    // teleport player to instance entry
                                    Debug.Log("player isServer: " + player.isServer + " hasAuthority: " + player.isOwned + " isClient: " + player.isClient + " isLocalPlayer: " + player.isLocalPlayer + "isowned" + player.isOwned);

                                    if (player.isServer)
                                    {
                                        Debug.Log("has authority");
                                        // Get the entry positions for the instance
                                        Vector3 entry1Pos = instance.entry1?.position ?? Vector3.zero;
                                        Vector3 entry2Pos = instance.entry2?.position ?? Vector3.zero;

                                        // Determine the target position for the player based on the player index

                                        int playerIndex = Array.IndexOf(playersInParty, player);
                                        Vector3 targetPos = (playerIndex % 2 == 0) ? entry2Pos : entry1Pos;

                                        // Call the CmdWarpToEntry() method to move the player to the target position
                                        if (player.isServer || (player.isClient && player.isOwned))
                                        {
                                            if (player.GetComponent<NetworkIdentity>().isOwned)
                                            {
                                                Debug.Log("client has authority for cmdwarpdrive");
                                                // Check if client is still connected
                                                if (!NetworkServer.connections.ContainsKey(player.GetComponent<NetworkIdentity>().connectionToClient.connectionId))
                                                {
                                                    // Client has disconnected, do something
                                                    Debug.Log("client has disconnected, do something!");
                                                }
                                                else
                                                {
                                                    // Call the CmdWarpDrive() method only if the player is owned by a client
                                                    player.movement.Warp(targetPos);
                                                    Debug.Log("player passed all checks, warping successfully");
                                                }
                                            }
                                            Debug.Log("player didn't have authority, still warping just in case");
                                            player.movement.Warp(targetPos);
                                        }
                                        Debug.Log("Teleporting " + player.name + " to new instance=" + instance.name + " with partyId=" + player.party.party.partyId);
                                    }
                                    else { Debug.Log("player is not server!!"); }

                                }
                                else if (player.isServer) player.chat.TargetMsgInfo("There are already too many " + instance.name + " instances. Please try again later.");
                            }
                        }

                        else
                        {
                            Debug.LogError("No existing instance found!");
                        }
                    }
                }

            }
        }

    }
    //mouse manger
    public bool isMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
    

    

    public void setTargetCursor()
    {
        Cursor.SetCursor(targetcursor, targetSpot, cursorMode);
        isTargeting = true;
    }

    public void setAttackCursor()
    {
        Cursor.SetCursor(attackCursor, attackSpot, cursorMode);
    }
    public void setLockpickTargetCursor()
    {
        Cursor.SetCursor(targetcursor, targetSpot, cursorMode);
        isTargeting = true;

    }
    [Command]
    public void CmdSetDefaultCursor()
    {
        Cursor.SetCursor(defaultCursor, defualtSpot, cursorMode);

        isTargeting = false;
        currentItemIndex = -1;
        skillIndex = -1;
        targetIndicator.enabled = false;

    }
    [Server]
    public void setDefaultCursor()
    {
        Cursor.SetCursor(defaultCursor, defualtSpot, cursorMode);

        isTargeting = false;
        currentItemIndex = -1;
        skillIndex = -1;
        targetIndicator.enabled = false;
    }
    [Command]
    public void CmdSetTargeting(bool value)
    {
        isTargeting = value;
    }

    [Command]
    public void CmdSetSkillIndex(int index)
    {
        skillIndex = index;
    }

    [Command]
    public void CmdSetCurrentItemIndex(int index)
    {
        currentItemIndex = index;
    }
    private void SetCustomCursor(Texture2D curText)
    {
        Cursor.SetCursor(curText, hotSpot, cursorMode);
    }

    [Command]
    public void CmdSetVector3(Vector3 mousePos)
    {

        storedMousePosition = mousePos;
    }
}


