//lots of scripts fo handeling features in ummorpg.  this is still a work in progress.  would like to thank trugord for implementing many of these features already into his arsenal.  
//hopefully someone will find use for these ideas.
using Controller2k;
using Mirror;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Controller2k;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.UI;
using System.CodeDom;

namespace GameZero
{
    public enum HolsterLocation : byte { ONE_HANDED_MAIN, ONE_HANDED_OFF, TWO_HANDED_MAIN, TWO_HANDED_OFF, UPPER_BACK, RANGED }
    public enum HOLSTER_STATE : byte
    {
        SHEATHED,
        DRAWN
    }
    public enum EquipmentSlotLocations
    {
        Helmet = 0,
        Glasses = 1,
        Cigar = 2,
        Mask = 3,
        Necklace = 4,
        Wings = 5,
        Cape = 6,
        Shoulders = 7,
        Tabard = 8,
        Chest = 9,
        Tie = 10,
        Shirt = 11,
        Wrist = 12,
        Hands = 13,
        Waist = 14,
        Tail = 15,
        Legs = 16,
        Feet = 17,
        Ring1 = 18,
        Ring2 = 19,
        Trinket1 = 20,
        Trinket2 = 21,
        Mainhand = 22,
        Offhand = 23,
        Artifact = 24,
        Ammo = 25

    }

    public enum DrawHolsterAnimationTriggers : int
    {
        Unarmed,
        MainHand1,
        MainHand2,
        OffHand1,
        OffHand2,
        Shield,
        MainHandGun1,
        MainHandGun2,
        OffHandGun1,
        OffHandGun2,
        MixHilts,
        MixBackBack,
        MixHiltBack,
        MixBackHilt,
        MixHiltShield,
        MixBackShield,
        NONE
    }
    public class SyncHashSetInt : SyncHashSet<int> { }
    [RequireComponent(typeof(PlayerInventory))]
    [DisallowMultipleComponent]


    public class battlescripts : NetworkBehaviour
    {
        public GameObject capeObject;
        public NpcCostume costume;
        public int instanceId;
        [Header("Physics")]
        [Tooltip("Apply a small default downward force while grounded in order to stick on the ground and on rounded surfaces. Otherwise walking on rounded surfaces would be detected as falls, preventing the player from jumping.")]
        public float gravityMultiplier = 2;
        public PlayerMountControl mountControl;
        static int nextPartyId = 1;
        private Dictionary<int, Party> parties;
        public PlayerEquipment pe;
        public bool charging;
        private float sitting = 0f;
        private bool running;
        //holster stuff
        public List<Transform> HolsterLocations;
        public HolsterLocation holsterLocation;
        public HandsRequired hands; // stores the handedness of THIS weapon
        public WeaponType weaponType; // stores the type of THIS weapon
        [HideInInspector] public Vector3 moveDir;

        private bool _hasMainHandWeapon => pe.slots[(int)EquipmentSlotLocations.Mainhand].amount > 0;
        private bool _hasOffHandWeapon => pe.slots.Count > (int)EquipmentSlotLocations.Offhand ? pe.slots[(int)EquipmentSlotLocations.Offhand].amount > 0 : false;
        private Transform GetMainHandWeaponLocation => pe.slotInfo[(int)EquipmentSlotLocations.Mainhand].location;
        private Transform GetOffHandWeaponLocation => pe.slotInfo[(int)EquipmentSlotLocations.Offhand].location;
        public enum HandsRequired { ONE_HANDED, TWO_HANDED, NONE } // declares the possible handednesses of weapons
        public enum WeaponType { Unarmed, Sword, Axe, Mace, Fist, Spear, Dagger, Shield, Staff, Bow, Wand, Gun, Tome, None } // declares the possible weapon types
        [SyncVar(hook = nameof(OnHolsterChanged))] public HOLSTER_STATE CURRENT_HOLSTER_STATE = HOLSTER_STATE.SHEATHED;
        //npc teleporter
        [Header("Npc teleporter")]

        public BuffSkill buffskill;
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
        public GameObject onemilpunchesprefab;
        //buff prefabs
        public GameObject critbusterprefab;
        public GameObject critvengprefab;
        public GameObject berserkprefab;
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

        public GameObject mainhand;
        public GameObject offhand;
        public GameObject twohandmainhandsheath;
        public GameObject onehandmainhandsheath;
        public GameObject twohandoffhandsheath;
        public GameObject onehandoffhandsheath;

        public GameObject twohandmainhandgunsheath;
        public GameObject onehandmainhandgunsheath;
        public GameObject twohandoffhandgunsheath;
        public GameObject onehandoffhandgunsheath;
        public GameObject staffsheath;
        public GameObject bowsheath;

        public GameObject shieldsheath;
        private MethodInfo eventDiedMethod;
        private MethodInfo eventUnderWaterMethod;

        private Energy energy;


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
        public PlayerEquipment equipment;
        public GameObject aoeCanvas;
        public Projector targetIndicator;

        public LayerMask layerMask = 1 << 16;

        public Vector3 mouseHitPosition;
        public Vector3 posUp;
        public float maxAbility2Distance;
        [Header("skillz")]
        public aoedamageskillstart aoestart;
        // public TargetDamageSkill dmgskill;
        public TargetProjectileSkill rapidfire;
        //buffs
        public RotatingStuff rotatingstuff;
        [Header("Playernpccostume")]

        public PlayerInventory inventory;

        public SyncHashSetInt unlockedCostume = new SyncHashSetInt();
        public List<int> sortedCostume;
        public float HorizontalSpeed => horizontalSpeed;



        
        private FieldInfo moveDirField;
        private FieldInfo desiredDirField;
        private FieldInfo inputDirField;
        private FieldInfo waterColliderField;

        private float swimSpeed = 5f;  // Adjust accordingly
        private float horizontalSpeed = 3f;
        private float swimSurfaceOffset = 0.3f;
        [SerializeField] private GameObject playerObject;
        [SerializeField] private Breath breath;

       



        public override void OnStartServer()
        {
            foreach (ItemSlot its in inventory.slots)
                if (its.amount > 0)
                    AddCostumeHash(its.item.name.GetStableHashCode());

            foreach (ItemSlot its in player.equipment.slots)
                if (its.amount > 0)
                    AddCostumeHash(its.item.name.GetStableHashCode());

            inventory.slots.Callback += (q, w, e, r) =>
            {
                if (r.amount > 0)
                {
                    int hashCode = r.item.data.name.GetStableHashCode();
                    AddCostumeHash(hashCode);
                }
            };



            // TEST ONLY
            //foreach (ScriptableItem item in ScriptableItem.All.Values)
            //    if (item is EquipmentItem)
            //        unlockedCostume.Add(item.name.GetStableHashCode());
        }
        public bool AllowedWeaponDrawState => pccm.state != MoveState.DEAD && pccm.state != MoveState.MOUNTED; // add other states if needed.
        public WeaponItem GetMainHandWeapon()
        {
            // Check if there is a main hand weapon equipped and return it as WeaponItem, or null if not.
            return (_hasMainHandWeapon ? pe.slots[(int)EquipmentSlotLocations.Mainhand].item.data as WeaponItem : null);
        }

        public EquipmentItem GetOffHandWeapon()
        {
            // Check if there is an offhand weapon equipped and return it as EquipmentItem, or null if not.
            return (_hasOffHandWeapon ? pe.slots[(int)EquipmentSlotLocations.Offhand].item.data as EquipmentItem : null);
        }

        //npccostume
        void AddCostumeHash(int h)
        {
            ScriptableItem itemData = ScriptableItem.All[h];
            if (!(itemData is EquipmentItem))
                return;

            if (!unlockedCostume.Contains(h))
                unlockedCostume.Add(h);

        }

        void UnlockedCallback(SyncHashSet<int>.Operation op, int q)
        {
            sortedCostume = unlockedCostume.ToList();
        }

        void UpdateCameraWithCustomLogic()
        {
            // Override or add logic here, use pccm to access the base class
            if (pccm.velocity != Vector3.zero)
            {
                if (pccm.GetComponent<Camera>().transform.parent != pccm.transform)
                    pccm.InitializeForcedLook();  // Your custom call
            }
        }



        // trading /////////////////////////////////////////////////////////////////


        [Command]
        public void CmdCustomizeEquip(int index, int costumeIndex)
        {
            // validate: close enough, npc alive and valid index?
            // use collider point(s) to also work with big entities
            int costumeHash = sortedCostume[costumeIndex];

            if (player.state == "IDLE" &&
                player.target != null &&
                player.target.health.current > 0 &&
                player.target is Npc npc &&
                costume != null && // only if Npc offers trading
                Utils.ClosestDistance(player, npc) <= player.interactionRange &&
                0 <= index && index < player.inventory.slots.Count &&
                unlockedCostume.Contains(costumeHash))
            {
                ScriptableItem costumeItem = ScriptableItem.All[costumeHash];

                ItemSlot slot = player.inventory.slots[index];
                slot.item.hash = costumeHash;
                player.inventory.slots[index] = slot;

                long cost = costumeItem.buyPrice + slot.item.data.buyPrice;

                player.gold -= cost;

                Debug.Log(costumeHash + " : " + player.inventory.slots[index].item.hash + " : " + cost);

            }
        }

        // drag & drop /////////////////////////////////////////////////////////////
        void OnDragAndDrop_InventorySlot_NpcCostumeSlot(int[] slotIndices)
        {
            // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
            ItemSlot slot = inventory.slots[slotIndices[0]];

            if (slot.item.data is EquipmentItem eq)
            {
                UINpcCostume.singleton.inventoryIndex = slotIndices[0];
                UINpcCostume.singleton.costumeIndex = -1;

            }
        }

        void OnDragAndClear_NpcCostumeSlot(int slotIndex)
        {
            UINpcCostume.singleton.inventoryIndex = -1;
        }


        //end npccostume
        void Start()
        {

            HandleCapeAnimation();
            Debug.Log("handlecapeanimations started");
            parties = GetParties();
            unlockedCostume.Callback += UnlockedCallback;
            UnlockedCallback(SyncSet<int>.Operation.OP_ADD, 1);
            Cursor.SetCursor(defaultCursor, defualtSpot, cursorMode);
            setDefaultCursor();

            if (player.isLocalPlayer)
            {
                targetIndicator.enabled = false;
            }




            trail.SetActive(false);

            OpenAll();


        }




        void Awake()
        {

            // initialize singleton
            if (singleton == null) singleton = this;
        }

        float ApplyGravity(float moveDirY)
        {
            // apply full gravity while falling
            if (!controller.isGrounded)
                // gravity needs to be * Time.fixedDeltaTime even though we multiply
                // the final controller.Move * Time.fixedDeltaTime too, because the
                // unit is 9.81m/s²
                return moveDirY + Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
            // if grounded then apply no force. the new OpenCharacterController
            // doesn't need a ground stick force. it would only make the character
            // slide on all uneven surfaces.
            return 0;
        }
        public void FixedUpdate()
        {
            

            if (charging)
            {
                DashMovement(buffName, stopDistance); // Handle dash movement if charging
            }
        }

        private void UpdateAnimationParameters(Vector3 moveDir)
        {
            // Calculate normalized direction
            Vector2 direction = new Vector2(moveDir.x, moveDir.z).normalized;

            // Update Blend Tree parameters
            animator.SetFloat("DirX", direction.x);
            animator.SetFloat("DirZ", direction.y);
        }

        public void DashMovement(string buffName, float stopDistance)
        {
            if (player.target != null)
            {
                Vector3 goal = player.target.transform.position;
                int index = player.skills.GetBuffIndexByName(buffName);

                charging = true;
                animator.SetBool("chargeattack", true);
                player.transform.position = Vector3.MoveTowards(player.transform.position, goal, player.speed * Time.fixedDeltaTime);
                player.transform.LookAt(goal);

                if (Vector3.Distance(player.transform.position, goal) <= stopDistance)
                {
                    if (index != -1)
                    {
                        player.skills.buffs.RemoveAt(index);
                    }
                    charging = false;
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
        public IEnumerator Berserk()
        {
            yield return new WaitForSeconds(15f);
            Debug.Log("attack 1: " + Time.time);
            rotatingstuff.redOrb.SetActive(false);
        }

        public void StartBerserk()
        {
            rotatingstuff.redOrb.SetActive(true);
            StartCoroutine(Berserk());
        }

        public void EndBerserk()
        {
            StopAllCoroutines(); // This stops all coroutines, you might want to handle this more specifically
        }

        public void StartCritBust()
        {
            rotatingstuff.purpleOrb.SetActive(true);
            StartCoroutine(CritBust());
        }

        public void EndCritBust()
        {
            StopAllCoroutines();
        }

        public IEnumerator CritBust()
        {
            yield return new WaitForSeconds(15f);
            Debug.Log("attack 1: " + Time.time);
            rotatingstuff.purpleOrb.SetActive(false);

        }

        public void StartCritVeng()
        {
            rotatingstuff.blueOrb.SetActive(true);
            StartCoroutine(CritVeng());
        }

        public void EndCritVeng()
        {
            StopAllCoroutines();
        }

        public IEnumerator CritVeng()
        {
            yield return new WaitForSeconds(15f);
            Debug.Log("attack 1: " + Time.time);
            rotatingstuff.blueOrb.SetActive(false);
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

        IEnumerator onemilAttack()
        {
            yield return new WaitForSeconds(.25f);
            Debug.Log("attack 1: " + Time.time);
            aoestart.onemil(caster, skillLevel);
            yield return new WaitForSeconds(.25f);
            Debug.Log("attack 2: " + Time.time);
            aoestart.onemil(caster, skillLevel);
            yield return new WaitForSeconds(.25f);
            Debug.Log("attack 3: " + Time.time);
            aoestart.onemil(caster, skillLevel);
            yield return new WaitForSeconds(.25f);
            Debug.Log("attack 4: " + Time.time);
            aoestart.onemil(caster, skillLevel);
            yield return new WaitForSeconds(.25f);
            Debug.Log("attack 5: " + Time.time);
            aoestart.onemil(caster, skillLevel);
            yield return new WaitForSeconds(.25f);
            Debug.Log("attack 6: " + Time.time);
            aoestart.onemil(caster, skillLevel);


        }

        public void startonemil()
        {
            StartCoroutine(onemilAttack());
        }
        public void endonemil
            ()
        {
            StopCoroutine(onemilAttack());
        }
        public IEnumerator dashforward()
        {

            Debug.Log("attack 1: " + Time.time);
            yield return new WaitForSeconds(.5f);
            aoestart.dashforward(caster, skillLevel);

        }

        public void startdashforward()
        {
            StartCoroutine(dashforward());
        }

        public void enddashforward()
        {
            StopAllCoroutines();
        }
        public IEnumerator jumplunge()
        {

            Debug.Log("attack 1: " + Time.time);
            yield return new WaitForSeconds(1f);
            aoestart.jumplunge(caster, skillLevel);

        }

        public void startjumplunge()
        {
            StartCoroutine(jumplunge());
        }

        public void endjumplunge()
        {
            StopAllCoroutines();
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
            onehandmainhandsheath.SetActive(false);////somewhere the object assigned to this, is getting deleted refesh location?//it might have the wrong pobject assigned to it
            onehandoffhandsheath.SetActive(false);
            twohandmainhandsheath.SetActive(false);//refresh location doesnt access this directly all references below it's a 2h and it's throwing error on 1h
            twohandoffhandsheath.SetActive(false);//im just talking here, still talkign about 1840


            twohandmainhandgunsheath.SetActive(false);
            onehandmainhandgunsheath.SetActive(false);
            twohandoffhandgunsheath.SetActive(false);
            onehandoffhandgunsheath.SetActive(false);
            staffsheath.SetActive(false);
            bowsheath.SetActive(false);

            shieldsheath.SetActive(false);
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

        public void MonsterSelected(Entity monster)
        {
            // Use the monster's attributes to determine its category
            string category = GetMonsterCategory(monster);

            if (category != "Mainhand" && category != "Offhand")
            {
                Debug.Log($"Upgrade length = 0");
                return;
            }

            // Implement logic based on the selected monster's category
            HandleMonsterBasedOnCategory(category);
        }

        private string GetMonsterCategory(Entity monster)
        {
            // Determine the category based on monster attributes
            if (monster.isBoss)
            {
                return "Boss";
            }
            else if (monster.isElite)
            {
                return "Elite";
            }
            else
            {
                return "Normal"; // Default category
            }
        }

        private void HandleMonsterBasedOnCategory(string category)
        {
            // Implement your logic based on the monster's category
            // For example:
            switch (category)
            {
                case "Boss":
                    // Handle Boss logic
                    Debug.Log("Handling Boss Monster");
                    break;
                case "Elite":
                    // Handle Elite logic
                    Debug.Log("Handling Elite Monster");
                    break;
                case "Normal":
                    // Handle Normal logic
                    Debug.Log("Handling Normal Monster");
                    break;
                default:
                    Debug.LogWarning("Unknown monster category");
                    break;
            }
        }

        private bool EventFalling()
        {
            // Implement your logic for detecting if the player is falling
            return false;
        }
        private Collider GetWaterCollider()
        {
            // You might need to retrieve water collider using reflection or other methods depending on how it's structured
            // If the waterCollider is private in PlayerCharacterControllerMovement, you can reflect or manage it here
            return null;
        }

       
        

        public void Update()
        {


            if (isLocalPlayer)
            {
                if (Input.GetKeyDown(KeyCode.Z) && !mountControl.activeMount)
                {
                    Debug.Log("Z key pressed, trying to toggle combat.");
                    TryChangeHolsterState();
                    CmdToggleCombat(!combatMode, true);
                }

                if (Input.GetKeyDown("x"))
                {
                    sitting = 1f - sitting; // Toggle sitting state
                    animator.SetFloat("sitting", sitting);
                }

              
            }

            if (Player.localPlayer == null) return;

            if (Player.localPlayer.target != null && Player.localPlayer.target.tag == "Monster")
            {
                Entity selectedMonster = Player.localPlayer.target;

                // Call the method with the selected monster
                MonsterSelected(selectedMonster);
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
                    mouseHitPosition = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                }
            }

            Quaternion transRot = Quaternion.LookRotation(mouseHitPosition - player.transform.position);

            var hitPosDir = (mouseHitPosition - transform.position).normalized;
            float distance = Vector3.Distance(mouseHitPosition, transform.position);
            distance = Mathf.Min(distance, maxAbility2Distance);

            var newHitPos = transform.position + hitPosDir * distance;
            aoeCanvas.transform.position = newHitPos;
        }



        //SELECTION
 //###################################################################################################################################################################
        public void MonsterSelected(int index)
        {
            // Retrieve the item slot and equipment info for the specified index
            ItemSlot slot = equipment.slots[index];
            EquipmentInfo info = equipment.slotInfo[index];

            // Check if the category is Mainhand or Offhand
            if (info.requiredCategory != "Mainhand" && info.requiredCategory != "Offhand")
            {
                Debug.Log($"Upgrade length = 0");
                // Handle the case when the category is not Mainhand or Offhand
                return;
            }

            // Ensure player is local before proceeding
            if (!player.isLocalPlayer) return;

            Animator anim = GetComponentInChildren<Animator>();
            anim.SetBool("AIMING", true);

            var handsRequired = GetHands();
            var weaponType = (WeaponType)player.equipment.GetEquippedWeaponType();
            bool isTwoHanded = handsRequired == HandsRequired.TWO_HANDED;

            if (player.equipment.slots[24].amount == 0 && player.equipment.slots[25].amount != 0)
            {
                HandleOneHandedWeapon(anim, weaponType);
            }
            else if (player.equipment.slots[24].amount != 0 && player.equipment.slots[25].amount == 0)
            {
                HandleOneHandedWeapon(anim, weaponType);
            }
            else if (player.equipment.slots[24].amount == 0 && player.equipment.slots[25].amount == 0)
            {
                HandleOneHandedWeapon(anim, weaponType);
            }

            if (isTwoHanded)
            {
                HandleTwoHandedWeapon(anim, weaponType);
            }

            if (player.equipment.slots[24].amount == 0)
            {
                OpenRight();
            }

            if (player.equipment.slots[25].amount == 0)
            {
                OpenLeft();
            }

            CheckPlayerState();
        }

        public void monster_not_selected()
        {
            clearstances();

        }
        public void nothing_selected()
        {
            clearstances();
        }

        //STANCES
        //###################################################################################################################################################################
        private void HandleOneHandedWeapon(Animator anim, WeaponType weaponType)
        {
            switch (weaponType)
            {
                case WeaponType.Fist:
                    anim.SetBool("oneMainHandfistAIMnoOH", true);
                    break;
                case WeaponType.Sword:
                    anim.SetBool("oneMainHandswordAIMnoOH", true);
                    anim.SetBool("oneOffHandswordAIMnoOH", true);
                    break;
                case WeaponType.Dagger:
                    anim.SetBool("oneMainHanddaggerAIMnoOH", true);
                    anim.SetBool("oneOffHanddaggerAIMnoOH", true);
                    break;
                case WeaponType.Axe:
                    anim.SetBool("oneMainHandaxeAIMnoOH", true);
                    anim.SetBool("oneOffHandaxeAIMnoOH", true);
                    break;
                case WeaponType.Mace:
                    anim.SetBool("oneMainHandmaceAIMnoOH", true);
                    anim.SetBool("oneOffHandmaceAIMnoOH", true);
                    break;
                case WeaponType.Shield:
                    anim.SetBool("shieldAIMnoOH", true);
                    break;
                default:
                    Debug.LogWarning("Unknown one-handed weapon type");
                    break;
            }
        }

        private void HandleTwoHandedWeapon(Animator anim, WeaponType weaponType)
        {
            switch (weaponType)
            {
                case WeaponType.Spear:
                    CloseRight();
                    CloseLeft();
                    anim.SetBool("twoMainHandspearAIMnoOH", true);
                    anim.SetBool("twoOffHandspearAIMnoOH", true);
                    break;
                case WeaponType.Sword:
                    CloseRight();
                    anim.SetBool("twoMainHandswordAIMnoOH", true);
                    anim.SetBool("twoOffHandswordAIMnoOH", true);
                    break;
                case WeaponType.Gun:
                    CloseRight();
                    CloseLeft();
                    anim.SetBool("twoMainHandgunAIMnoOH", true);
                    anim.SetBool("twoOffHandgunAIMnoOH", true);
                    break;
                case WeaponType.Axe:
                    CloseRight();
                    CloseLeft();
                    anim.SetBool("twoMainHandaxeAIMnoOH", true);
                    anim.SetBool("twoOffHandaxeAIMnoOH", true);
                    break;
                case WeaponType.Mace:
                    CloseRight();
                    CloseLeft();
                    anim.SetBool("twoMainHandmaceAIMnoOH", true);
                    anim.SetBool("twoOffHandmaceAIMnoOH", true);
                    break;
                case WeaponType.Bow:
                    anim.SetBool("twoMainHandbowAIMnoOH", true);
                    anim.SetBool("twoOffHandbowAIMnoOH", true);
                    break;
                default:
                    Debug.LogWarning("Unknown two-handed weapon type");
                    break;
            }
        }

        private void CheckPlayerState()
        {
            if (Player.localPlayer.state == "MOUNTED" ||
                Player.localPlayer.state == "MOVING" ||
                Player.localPlayer.state == "CASTING" ||
                Player.localPlayer.state == "STUNNED" ||
                Player.localPlayer.state == "Autoattack" ||
                Player.localPlayer.state == "Strong Hit" ||
                Player.localPlayer.state == "aoedamageskill" ||
                Player.localPlayer.state == "frontflip" ||
                charging ||
                Player.localPlayer.state == "Fireball" ||
                Player.localPlayer.state == "Fireblast")
            {
                clearstances();
            }
        }
        


//###################################################################################################################################################################
//Attack timers


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
        IEnumerator critbusttimer()
        {
            yield return new WaitForSeconds(3f);
            //animator.applyRootMotion = false;
        }
        public void applycritbust()
        {
            StartCritBust();
            //animator.applyRootMotion = true;
            StartCoroutine(critbusttimer());

        }
        IEnumerator critvengtimer()
        {
            yield return new WaitForSeconds(3f);
            //animator.applyRootMotion = false;
        }
        public void applycritveng()
        {
            StartCritVeng();
            //animator.applyRootMotion = true;
            StartCoroutine(critvengtimer());

        }
        IEnumerator berserktimer()
        {
            yield return new WaitForSeconds(3f);
            //animator.applyRootMotion = false;
        }
        public void applyberserk()
        {
            StartBerserk();
            //animator.applyRootMotion = true;
            StartCoroutine(berserktimer());

        }

        public void applyjumplunge()
        {


            trail.SetActive(true);




            animator.applyRootMotion = true;
            startjumplunge();
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
            startdashforward();
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

            //pccm.state = MoveState.SNEAKING;


            StartCoroutine(invisibilitytimer());

        }
        IEnumerator invisibilitytimer()
        {
            yield return new WaitForSeconds(30f);
            pccm.state = MoveState.IDLE;

        }


        //###################################################################################################################################################################

        //ARENA stuff

        [Command(requiresAuthority = false)]
        public void CmdLobbyCreator()
        {
            Debug.Log("Player assigned: " + player.name);
            if (player != null)
            {
                FormSoloParty(player.name, player.name);
                partyId = player.party.party.partyId;
                Debug.Log($"partyId = {partyId}");
            }
        }


        [Command(requiresAuthority = false)]
        public void CmdLobbyJoiner()
        {
            if (player != null)
            {
                AddToParty(partyId, player.name);
            }
        }

        

        [Command(requiresAuthority = false)]
        public void CmdPartySetup()
        {
            ArenaManager am = GameObject.FindWithTag("GameManager").GetComponent<ArenaManager>();
            Instance instanceTemplate = am.GetInstanceTemplate(instanceId);

            Debug.Log("Cmdpartysetup called");
            Debug.Log("instanceTemplate = " + instanceTemplate);
            Player[] playersInParty = GetPlayersInParty(partyId);
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
                                        Vector3 entry1Pos = existingInstance.entry?.position ?? Vector3.zero;
                                        Vector3 entry2Pos = existingInstance.entry?.position ?? Vector3.zero;

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
                                            Vector3 entry1Pos = instance.entry?.position ?? Vector3.zero;
                                            Vector3 entry2Pos = instance.entry?.position ?? Vector3.zero;

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

 //###################################################################################################################################################################
        //AOE ATTACKS 


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

        //holsterstuff
        private DrawHolsterAnimationTriggers StandardLogic(bool MainHand)
        {
            if (MainHand)
            {
                return hands == HandsRequired.ONE_HANDED ? DrawHolsterAnimationTriggers.MainHand1 : DrawHolsterAnimationTriggers.MainHand2;
            }
            else
            {
                return hands == HandsRequired.ONE_HANDED ? DrawHolsterAnimationTriggers.OffHand1 : DrawHolsterAnimationTriggers.OffHand2;
            }
        }

        private DrawHolsterAnimationTriggers GunLogic(bool MainHand)
        {
            if (MainHand)
            {
                return hands == HandsRequired.ONE_HANDED ? DrawHolsterAnimationTriggers.MainHandGun1 : DrawHolsterAnimationTriggers.MainHandGun2;
            }
            else
            {
                return hands == HandsRequired.ONE_HANDED ? DrawHolsterAnimationTriggers.OffHandGun1 : DrawHolsterAnimationTriggers.OffHandGun2;
            }
        }

        public DrawHolsterAnimationTriggers GetEquipmentTrigger(bool MainHand)
        {
            switch (weaponType)
            {
                case WeaponType.Sword:
                    return StandardLogic(MainHand);

                case WeaponType.Axe:
                    return StandardLogic(MainHand);

                case WeaponType.Mace:
                    return StandardLogic(MainHand);

                case WeaponType.Fist:
                    return MainHand ? DrawHolsterAnimationTriggers.MainHand1 : DrawHolsterAnimationTriggers.OffHand1;

                case WeaponType.Spear:
                    return StandardLogic(MainHand);

                case WeaponType.Dagger:
                    return MainHand ? DrawHolsterAnimationTriggers.MainHand1 : DrawHolsterAnimationTriggers.OffHand1;

                case WeaponType.Shield:
                    return DrawHolsterAnimationTriggers.Shield;

                case WeaponType.Staff:
                    return StandardLogic(MainHand); // assuming there are 1h staffs?  there are wands

                case WeaponType.Bow:
                    return DrawHolsterAnimationTriggers.OffHand2;

                case WeaponType.Wand:
                    return MainHand ? DrawHolsterAnimationTriggers.MainHand1 : DrawHolsterAnimationTriggers.OffHand1;

                case WeaponType.Gun:
                    return GunLogic(MainHand);

                case WeaponType.Tome:
                    return MainHand ? DrawHolsterAnimationTriggers.MainHand1 : DrawHolsterAnimationTriggers.OffHand1;

                case WeaponType.None:
                    return DrawHolsterAnimationTriggers.NONE;

                default: return DrawHolsterAnimationTriggers.NONE;
            }
        }


        public HandsRequired GetHands()
        {
            int index = player.equipment.GetEquippedWeaponIndex();
            // Ensure the item in the slot is of type WeaponItem before accessing 'hands'
            if (pe.slots[index].item.data is WeaponItem weaponItem)
            {
                return hands;
            }

            // If the item is not a WeaponItem, return a default value or throw an exception
            Debug.LogWarning("Item in slot is not a WeaponItem");
            return HandsRequired.NONE; // Use an appropriate default value
        }


        public bool combatMode = true;//is being deleted by refreshlocation or even somewhere else
        public string mainHandAni;
        public string offHandAni;


        [Server]
        public void ServerToggleCombat(bool combatActive, bool playAni)//cant call a command from server, only client so server for when server calls this
        {
            RpcToggleCombat(combatActive, playAni);
        }
        [Command]
        public void CmdToggleCombat(bool combatActive, bool playAni)//i realized that refreshlocation is a client rpc, but not when we toggle ourselves so we make an rpc for that case
        {
            RpcToggleCombat(combatActive, playAni);
        }
        [ClientRpc]
        public void RpcToggleCombat(bool combatActive, bool playAni)
        {
            combatMode = combatActive;
            if (combatMode)
            {
                Debug.Log("combatMode active");
                if (player.equipment.slots[22].amount != 0)
                {
                    foreach (Animator animator in GetComponentsInChildren<Animator>())
                    {
                        animator.SetBool("closeRight", true);
                    }

                }
                else
                {
                    foreach (Animator animator in GetComponentsInChildren<Animator>())
                    {
                        animator.SetBool("closeRight", false);
                    }
                }
                if (player.equipment.slots[23].amount != 0)
                {
                    foreach (Animator animator in GetComponentsInChildren<Animator>())
                    {
                        animator.SetBool("closeLeft", true);
                    }
                }
                else
                {
                    foreach (Animator animator in GetComponentsInChildren<Animator>())
                    {
                        animator.SetBool("closeLeft", false);
                    }
                }
            }
            else
            {
                foreach (Animator animator in GetComponentsInChildren<Animator>())
                {
                    animator.SetBool("closeRight", false);
                    animator.SetBool("closeLeft", false);
                }
            }
            if (playAni)
            {
                PlayAnimation();
            }
            StartCoroutine(DelayedToggle(combatActive));
        }


        public void ToggleCombat(bool combatActive, bool playAni)///okay all cases solved, it should be networked 100% lets ctext animation issue
        {
            combatMode = combatActive;

            if (combatMode)
            {
                if (player.equipment.slots[22].amount != 0)
                {
                    foreach (Animator animator in GetComponentsInChildren<Animator>())
                    {
                        animator.SetBool("closeRight", true);
                    }

                }
                else
                {
                    foreach (Animator animator in GetComponentsInChildren<Animator>())
                    {
                        animator.SetBool("closeRight", false);
                    }
                }
                if (player.equipment.slots[23].amount != 0)
                {
                    foreach (Animator animator in GetComponentsInChildren<Animator>())
                    {
                        animator.SetBool("closeLeft", true);
                    }
                }
                else
                {
                    foreach (Animator animator in GetComponentsInChildren<Animator>())
                    {
                        animator.SetBool("closeLeft", false);
                    }
                }
            }
            else
            {
                foreach (Animator animator in GetComponentsInChildren<Animator>())
                {
                    animator.SetBool("closeRight", false);
                    animator.SetBool("closeLeft", false);
                }
            }
            if (playAni)
            {
                PlayAnimation();
            }
            StartCoroutine(DelayedToggle(combatActive));
        }
        public void PlayAnimation() //play animation before actual swap
        {
            mainHandAni = "";
            offHandAni = "";

            //mainhand
            if (player.equipment.slots[22].amount != 0)
            {// when we check 1 or 2 hand, its not gonna care what hand we want
                if (GetHands() == HandsRequired.ONE_HANDED)
                {

                    switch ((WeaponType)player.equipment.GetEquippedWeaponType())
                    {
                        case WeaponType.Gun:
                            {
                                mainHandAni = "1MainHandGun";
                                break;
                            }

                        default:
                            {
                                mainHandAni = "1MainHand";
                                break;
                            }

                    }
                }
                if (GetHands() == HandsRequired.TWO_HANDED)
                {
                    switch ((WeaponType)player.equipment.GetEquippedWeaponType())
                    {
                        case WeaponType.Gun:
                            {
                                mainHandAni = "2MainHandGun";
                                break;
                            }
                        default:
                            {
                                mainHandAni = "2MainHand";
                                break;
                            }

                    }
                }
            }
            if (player.equipment.slots[23].amount != 0)
            {// when we check 1 or 2 hand, its not gonna care what hand we want
                if (GetHands() == HandsRequired.ONE_HANDED)
                {
                    switch ((WeaponType)player.equipment.GetEquippedWeaponType())
                    {
                        case WeaponType.Gun:
                            {
                                offHandAni = "1OffHandGun";
                                break;
                            }
                        case WeaponType.Shield:
                            {
                                offHandAni = "Shield";
                                break;
                            }
                        default:
                            {
                                offHandAni = "1OffHand";
                                break;
                            }

                    }
                }
                if (GetHands() == HandsRequired.TWO_HANDED)
                {
                    switch ((WeaponType)player.equipment.GetEquippedWeaponType())
                    {
                        case WeaponType.Gun:
                            {
                                offHandAni = "2OffHandGun";
                                break;
                            }
                        default:
                            {
                                offHandAni = "2OffHand";
                                break;
                            }

                    }
                }
            }



            Debug.Log(mainHandAni + ":main    " + offHandAni + ":offhand      before changing into mix");

            if (mainHandAni != "" && offHandAni != "")
            {
                if (mainHandAni == "1MainHand" && offHandAni == "1OffHand") { mainHandAni = "MixHilts"; offHandAni = ""; }//1
                else if (mainHandAni == "2MainHand" && offHandAni == "2OffHand") { mainHandAni = "MixBackBack"; offHandAni = ""; }

                else if (mainHandAni == "1MainHand" && offHandAni == "2OffHand") { mainHandAni = "MixHiltBack"; offHandAni = ""; }//2
                else if (mainHandAni == "2MainHand" && offHandAni == "1Offhand") { mainHandAni = "MixBackHilt"; offHandAni = ""; }//3

                else if (mainHandAni == "1MainHand" && offHandAni == "Shield") { mainHandAni = "MixHiltShield"; offHandAni = ""; }
                else if (mainHandAni == "2MainHand" && offHandAni == "Shield") { mainHandAni = "MixBackShield"; offHandAni = ""; }

                if (mainHandAni == "1MainHandGun" && offHandAni == "1OffHandGun") { mainHandAni = "MixHilts"; offHandAni = ""; }//1
                else if (mainHandAni == "2MainHandGun" && offHandAni == "2OffHandGun") { mainHandAni = "MixBackBack"; offHandAni = ""; }

                else if (mainHandAni == "1MainHandGun" && offHandAni == "2OffHandGun") { mainHandAni = "MixHiltBack"; offHandAni = ""; }//2
                else if (mainHandAni == "2MainHandGun" && offHandAni == "1OffHandGun") { mainHandAni = "MixBackHilt"; offHandAni = ""; }//3

                else if (mainHandAni == "1MainHandGun" && offHandAni == "Shield") { mainHandAni = "MixHiltShield"; offHandAni = ""; }
                else if (mainHandAni == "2MainHandGun" && offHandAni == "Shield") { mainHandAni = "MixBackShield"; offHandAni = ""; }

                offHandAni = "";
            }
            Debug.Log(mainHandAni);
            foreach (Animator animator in GetComponentsInChildren<Animator>())
            {
                if (mainHandAni != "")
                {
                    animator.SetTrigger(mainHandAni);
                }
                if (offHandAni != "")
                {
                    animator.SetTrigger(offHandAni);//use triggers
                }
            }//this might be the way lmao
        }
        IEnumerator<WaitForSeconds> DelayedToggle(bool combatActive)
        {
            yield return new WaitForSeconds(0.6f);//give time for animation to play ^^
            combatMode = combatActive;
            if (combatMode)
            {
                mainhand.SetActive(true);
                offhand.SetActive(true);



                CloseAll(); //just close all 
            }
            else
            {
                mainhand.SetActive(false);
                offhand.SetActive(false);
                CloseAll(); //always close all, so we can only set true what we do have   //its checking slots before theyreefreshlocation



                //mainhand
                if (player.equipment.slots[22].amount != 0)
                {
                    Debug.Log("mainhand is not empty");
                    //close right hand

                    if (GetHands() == HandsRequired.ONE_HANDED)
                    {
                        switch ((WeaponType)player.equipment.GetEquippedWeaponType())
                        {
                            case WeaponType.Gun:
                                {
                                    onehandmainhandgunsheath.SetActive(true);
                                    break;
                                }
                            case WeaponType.Shield:
                                {
                                    Debug.Log("shield sheath activated");
                                    shieldsheath.SetActive(true);
                                    break;
                                }
                            default:
                                {
                                    onehandmainhandsheath.SetActive(true);
                                    break;
                                }

                        }
                    }
                    if (GetHands() == HandsRequired.TWO_HANDED)
                    {
                        switch ((WeaponType)player.equipment.GetEquippedWeaponType())
                        {
                            case WeaponType.Gun:
                                {
                                    twohandmainhandgunsheath.SetActive(true);
                                    break;
                                }
                            case WeaponType.Staff:
                                {
                                    staffsheath.SetActive(true);
                                    break;
                                }
                            default:
                                {
                                    twohandmainhandsheath.SetActive(true);
                                    break;
                                }


                        }
                    }

                }
                //offhand
                if (player.equipment.slots[23].amount != 0)
                {
                    Debug.Log("offhand is not empty");
                    if (GetHands() == HandsRequired.ONE_HANDED)
                    {
                        switch ((WeaponType)player.equipment.GetEquippedWeaponType())
                        {
                            case WeaponType.Gun:
                                {
                                    onehandoffhandgunsheath.SetActive(true);
                                    break;
                                }
                            case WeaponType.Shield:
                                {
                                    Debug.Log("shield sheath activated");
                                    shieldsheath.SetActive(true);
                                    break;
                                }
                            default:
                                {
                                    onehandoffhandsheath.SetActive(true);
                                    break;
                                }

                        }
                    }
                    if (GetHands() == HandsRequired.TWO_HANDED)
                    {
                        switch ((WeaponType)player.equipment.GetEquippedWeaponType())
                        {
                            case WeaponType.Gun:
                                {
                                    twohandoffhandgunsheath.SetActive(true);
                                    break;
                                }
                            default:
                                {
                                    twohandoffhandsheath.SetActive(true);
                                    break;
                                }

                        }
                    }


                }

            }
        }



        public bool HasShield() => _hasOffHandWeapon ? GetOffHandWeapon() is not WeaponItem : false;
        public DrawHolsterAnimationTriggers EvaluateHolsterAnimation() // this was made very specifically for you previous setup.
        {
            DrawHolsterAnimationTriggers main = GetMainHandWeapon() ? GetEquipmentTrigger(true) : DrawHolsterAnimationTriggers.NONE;
            DrawHolsterAnimationTriggers off = GetOffHandWeapon() ?
                // do we have a shield? if so just return it's a shield.
                (HasShield() ? DrawHolsterAnimationTriggers.Shield :
                // otherwise if we have something equipped here it must be a weapon, get the trigger name:
                GetEquipmentTrigger(false))
                // we had no item.
                : DrawHolsterAnimationTriggers.NONE;

            // this means we have only a main hand weapon, no shield no off hand weapon.
            if (_hasMainHandWeapon && !_hasOffHandWeapon) return main;

            // this means we have dual wield.
            else if (_hasMainHandWeapon && _hasOffHandWeapon)
            {
                // shield specific:
                if (off == DrawHolsterAnimationTriggers.Shield)
                {
                    switch (hands)
                    {
                        case HandsRequired.ONE_HANDED:
                            {
                                return DrawHolsterAnimationTriggers.MixHiltShield;
                            }

                        // if main hand weapon is two handed:
                        case HandsRequired.TWO_HANDED:
                            {
                                return DrawHolsterAnimationTriggers.MixBackShield;
                            }
                    }
                }

                switch (hands)
                {
                    case HandsRequired.ONE_HANDED:
                        {
                            switch (hands)
                            {
                                case HandsRequired.ONE_HANDED:
                                    return DrawHolsterAnimationTriggers.MixHilts;

                                case HandsRequired.TWO_HANDED:
                                    return DrawHolsterAnimationTriggers.MixHiltBack;

                                default:
                                    return DrawHolsterAnimationTriggers.MainHand1;
                            }
                        }

                    // if main hand weapon is two handed:
                    case HandsRequired.TWO_HANDED:
                        {
                            // if off hand is:
                            switch (hands)
                            {
                                case HandsRequired.ONE_HANDED:
                                    return DrawHolsterAnimationTriggers.MixBackHilt;

                                case HandsRequired.TWO_HANDED:
                                    return DrawHolsterAnimationTriggers.MixBackBack;

                                default:
                                    return DrawHolsterAnimationTriggers.MainHand2;
                            }
                        }

                    // default to send back the same as main.
                    default:
                        return GetEquipmentTrigger(true);

                }

            }

            // this means we only have a offhand weapon.
            else
            {
                return off;
            }

            // can shields only be wielded in offhand? I think it's only offhand for shield, but are shield weapon items?  ya u sure?
            // sec replying to some messages.  can we test? no not yet ^^
            #region Old Logic
            /*
            switch (main)
            {
                case DrawHolsterAnimationTriggers.MainHand1:
                    switch (off)
                    {
                        case DrawHolsterAnimationTriggers.OffHand1:
                            return DrawHolsterAnimationTriggers.MixHilts;

                        case DrawHolsterAnimationTriggers.OffHand2:
                            return DrawHolsterAnimationTriggers.MixHiltBack;

                        case DrawHolsterAnimationTriggers.Shield:
                            return DrawHolsterAnimationTriggers.MixHiltShield;

                        default: return DrawHolsterAnimationTriggers.MainHand1;
                    }

                case DrawHolsterAnimationTriggers.MainHand2:
                    {
                        switch (off)
                        {
                            case DrawHolsterAnimationTriggers.OffHandGun2:
                                return DrawHolsterAnimationTriggers.MixBackBack;

                            case DrawHolsterAnimationTriggers.OffHand1:
                                return DrawHolsterAnimationTriggers.MixBackHilt;

                            case DrawHolsterAnimationTriggers.Shield:
                                return DrawHolsterAnimationTriggers.MixBackShield;

                            default: return DrawHolsterAnimationTriggers.MainHand2;
                        }

                    }

                case DrawHolsterAnimationTriggers.MainHandGun1:
                    switch (off)
                    {
                        case DrawHolsterAnimationTriggers.OffHandGun1:
                            return DrawHolsterAnimationTriggers.MixHilts;

                        case DrawHolsterAnimationTriggers.OffHandGun2:
                            return DrawHolsterAnimationTriggers.MixBackHilt;

                        case DrawHolsterAnimationTriggers.Shield:
                            return DrawHolsterAnimationTriggers.MixBackShield;

                        default: return DrawHolsterAnimationTriggers.MainHand2;
                    }

                case DrawHolsterAnimationTriggers.MainHandGun2:
                    break;
                case DrawHolsterAnimationTriggers.OffHandGun1:
                    break;
                case DrawHolsterAnimationTriggers.OffHandGun2:
                    break;

                default:
                    return DrawHolsterAnimationTriggers.NONE;
            }

            if (main == DrawHolsterAnimationTriggers.MainHand1)
            {
                switch (off)
                {
                    case DrawHolsterAnimationTriggers.MainHand1:
                        break;
                    case DrawHolsterAnimationTriggers.MainHand2:
                        break;
                    case DrawHolsterAnimationTriggers.OffHand1:
                        return DrawHolsterAnimationTriggers.MixHilts;

                    case DrawHolsterAnimationTriggers.OffHand2:
                        return DrawHolsterAnimationTriggers.MixHiltBack;
                        break;
                    case DrawHolsterAnimationTriggers.Shield:
                        break;
                    case DrawHolsterAnimationTriggers.MainHandGun1:
                        break;
                    case DrawHolsterAnimationTriggers.MainHandGun2:
                        break;
                    case DrawHolsterAnimationTriggers.OffHandGun1:
                        break;
                    case DrawHolsterAnimationTriggers.OffHandGun2:
                        break;



                    case DrawHolsterAnimationTriggers.MixHilts:
                        break;
                    case DrawHolsterAnimationTriggers.MixBackBack:
                        break;
                    case DrawHolsterAnimationTriggers.MixHiltBack:
                        break;
                    case DrawHolsterAnimationTriggers.MixBackHilt:
                        break;
                    case DrawHolsterAnimationTriggers.MixHiltShield:
                        break;
                    case DrawHolsterAnimationTriggers.MixBackShield:
                        break;
                    case DrawHolsterAnimationTriggers.NONE:
                        break;
                    default:
                        break;
                }
            }
            */
            #endregion

        }

        public bool CanDrawWeapon()
        {
            return player.state != "DEAD" && AllowedWeaponDrawState && !IsSwappingWeapon;
        }

        // simple "toggle with 'Z' button.
        [ClientCallback]
        public void TryChangeHolsterState()
        {
            if (isLocalPlayer)
            {
                if (CURRENT_HOLSTER_STATE == HOLSTER_STATE.SHEATHED && !CanDrawWeapon()) return;

                bool isDrawn = CURRENT_HOLSTER_STATE == HOLSTER_STATE.DRAWN;

                // simply swap the state
                CmdUpdateHolster(isDrawn ? HOLSTER_STATE.SHEATHED : HOLSTER_STATE.DRAWN);
            }
        }

        // useful for example if we go into climbing, we can just force holster.
        [ClientCallback]
        public void ForceHolsterState(HOLSTER_STATE _state)
        {
            if (isLocalPlayer)
            {
                if (CURRENT_HOLSTER_STATE == HOLSTER_STATE.SHEATHED && !CanDrawWeapon()) return;

                CmdUpdateHolster(_state);
            }
        }

        // this code will override anything else, set on the server side.
        [ServerCallback]
        public void ForceHolsterFromServer(HOLSTER_STATE _state)
        {
            CURRENT_HOLSTER_STATE = _state;
        }

        [Command]
        // this will trigger the hook on all clients, no need for RPC's or anything.
        public void CmdUpdateHolster(HOLSTER_STATE _state)
        {
            // simply set the new state.
            CURRENT_HOLSTER_STATE = _state;
        }

        private IEnumerator ChangeWeaponRoutine()
        {
            IsSwappingWeapon = true;
            // 1) run animation.

            // everytime holster state changes, we run animation.
            foreach (Animator anim in GetComponentsInChildren<Animator>())
            {
                anim.SetTrigger(EvaluateHolsterAnimation().ToString());
            }
            // if you want to delay more or less just change the 0.5f.
            yield return new WaitForSeconds(0.5f);

            // 2) do relevant disabling.

            // always disable all the holster locations to then enable them later, prevents some locations being left behind.
            foreach (Transform location in HolsterLocations)
            {
                location.gameObject.SetActive(false);
            }

            // disable the weapons main hand either way, we handle logic later.
            GetMainHandWeaponLocation.gameObject.SetActive(false);
            GetOffHandWeaponLocation.gameObject.SetActive(false);

            // 3) do relevant enabling.

            // check if the new state is drawn or not.
            bool WeaponIsDrawn = CURRENT_HOLSTER_STATE == HOLSTER_STATE.DRAWN;

            // if we have a main hand weapon.
            if (_hasMainHandWeapon)
            {
                // cache the main hand weapon's data.
                WeaponItem Mainhand = GetMainHandWeapon();

                // enable if the new holster state is drawn otherwise disable it.

                int mainHolsterIndex = GetHolsterIndex(Mainhand, true);

                GetMainHandWeaponLocation.gameObject.SetActive(WeaponIsDrawn);
                HolsterLocations[mainHolsterIndex].gameObject.SetActive(!WeaponIsDrawn);
            }

            // if we have a off hand weapon. this will be the stranges thing in the world for ever.
            if (_hasOffHandWeapon)
            {
                // cache the off hand weapon's data.
                EquipmentItem Offhand = GetOffHandWeapon();

                int offHolsterIndex = GetHolsterIndex(Offhand, false);

                GetOffHandWeaponLocation.gameObject.SetActive(WeaponIsDrawn);
                // enable if the new holster state is drawn, otherwise disable it.
                HolsterLocations[offHolsterIndex].gameObject.SetActive(!WeaponIsDrawn);
            }

            IsSwappingWeapon = false;
        }


        public int GetHolsterIndex(EquipmentItem data, bool isMainHand)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "EquipmentItem data cannot be null.");
            }

            switch (weaponType)
            {
                case WeaponType.Bow:
                    return (int)HolsterLocation.RANGED;

                case WeaponType.Shield:
                    return (int)HolsterLocation.UPPER_BACK;

                default:
                    if (isMainHand)
                    {
                        return hands == HandsRequired.ONE_HANDED ?
                            (int)HolsterLocation.ONE_HANDED_MAIN :
                            (int)HolsterLocation.TWO_HANDED_MAIN;
                    }
                    else
                    {
                        return hands == HandsRequired.ONE_HANDED ?
                            (int)HolsterLocation.ONE_HANDED_OFF :
                            (int)HolsterLocation.TWO_HANDED_OFF;
                    }
            }
        }


        private bool IsSwappingWeapon = false;

        void OnHolsterChanged(HOLSTER_STATE oldState, HOLSTER_STATE newState)
        {

            StartCoroutine(ChangeWeaponRoutine());

            // everytime holster state changes, we run animation.
            //foreach (Animator anim in GetComponentsInChildren<Animator>())
            //{
            //    anim.SetTrigger(EvaluateHolsterAnimation().ToString());
            //}

        }


        private static Dictionary<int, Party> GetParties()
        {
            // Get the type of the PartySystem class
            Type partySystemType = typeof(PartySystem);

            // Get the private static field "parties"
            FieldInfo partiesField = partySystemType.GetField("parties", BindingFlags.NonPublic | BindingFlags.Static);

            // Get the value of the "parties" field
            return (Dictionary<int, Party>)partiesField.GetValue(null);
        }

        // party stuff
        public static void FormSoloParty(string creator, string firstMember)
        {
            // Access the private static "parties" dictionary
            Dictionary<int, Party> parties = GetParties();

            int partyId = nextPartyId++;
            Party party = new Party(partyId, creator, firstMember);

            // Add the party to the dictionary
            parties.Add(partyId, party);

            BroadcastChanges(party);
            Debug.Log(creator + " formed a new party");
        }

        public static Player[] GetPlayersInParty(int partyId)
        {
            Dictionary<int, Party> parties = GetParties();

            if (parties.TryGetValue(partyId, out Party party))
            {
                string[] memberNames = party.members;
                List<Player> players = new List<Player>();
                foreach (string memberName in memberNames)
                {
                    Player player = GetPlayerByName(memberName);
                    if (player != null)
                        players.Add(player);
                }
                return players.ToArray();
            }
            return new Player[0];
        }

        public static Player GetPlayerByName(string playerName)
        {
            Player[] players = GameObject.FindObjectsOfType<Player>();
            foreach (Player player in players)
            {
                if (player.name == playerName)
                    return player;
            }
            return null;
        }

        static void BroadcastChanges(Party party)
        {
            Dictionary<int, Party> parties = GetParties(); // Make sure we access parties statically
            foreach (string member in party.members)
                BroadcastTo(member, party);

            parties[party.partyId] = party; // Save the changes in the static dictionary
        }

        static void BroadcastTo(string member, Party party)
        {
            if (Player.onlinePlayers.TryGetValue(member, out Player player))
                player.party.party = party;
        }

        public static void AddToParty(int partyId, string member)
        {
            Dictionary<int, Party> parties = GetParties(); // Use GetParties() to access parties

            // party exists and not full?
            if (parties.TryGetValue(partyId, out Party party) && !party.IsFull())
            {
                // Add to members
                Array.Resize(ref party.members, party.members.Length + 1);
                party.members[party.members.Length - 1] = member;

                // Broadcast and save in dict
                BroadcastChanges(party);
                Debug.Log(member + " was added to party " + partyId);
            }
        }







        //###################################################################################################################################################################


        //CAPE modifier
        public void HandleCapeAnimation()
        {
            Animator capeAnimator = capeObject.GetComponentInChildren<Animator>();
            if (capeAnimator != null)
            {
                RuntimeAnimatorController testController = Resources.Load<RuntimeAnimatorController>("Controllers/fattestcapecontroller");
                if (testController != null)
                {
                    capeAnimator.runtimeAnimatorController = testController;
                    Debug.Log("Test Animator Controller Assigned");
                }
                else
                {
                    Debug.LogError("Test Animator Controller not found");
                }
            }
            else
            {
                Debug.LogError("Cape Animator not found");
            }
        }



    }
}






