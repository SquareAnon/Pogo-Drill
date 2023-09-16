using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pogo : MonoBehaviour
{
    public bool hasWon;
    public LayerMask groundMask;
    public float jumpHeight;
    public Vector3 velocity;
    public Animator anim;
    public float gravity = 10;
    [SerializeField]float g;
    [SerializeField] float fallDuration;
    public Transform pointer;
    GoalArea goal;

    [Header("Rotation")]
    public float rotationSpeed = 50;
    public float rotationValue;
   [SerializeField] float rot;
   
    [Header("Drill collision")]
    public float rayLength = .2f;
    public float rayOffset = .3f;
    public float rayRadius = .1f;

    [Header("Drill fast")]
    public bool charging;
    public float drillSpeed = 10;
    public float fuelConsumptionPerFrame = 10;
    public float highJumpThreshold =.1f;
    public float drillFastEndTime;
    
    Vector3 prevPos;
    public float yVelocityMultiplier = 50;

    [Header("Drill visuals")]
    public Transform drill;
    public float drillRotSpeed = 5;
    public float drillRotSpeedMult = 15;
    public AnimationCurve drillScaleCurve;
    public GameObject speedLines;
    public float drillScaleLerp;
    float posDelta;
    public GameObject drillFX;


    [Header("Player Stats")]
    public bool isDead;
    public int hp;
    public int maxHP = 100;
    public System.Action<int, int> OnHPChange;
    public float iFrameDuration = .1f;
    float lastHitTime;
    public Material white;
    public Renderer body;
    Material mat;
    public GameObject hitFX;
    public int drillPower;

    public float depth, depthOffset;
    public System.Action<float> OnDepthChange;
    public float maxDepth;

    public bool gotDiamond;
    public bool returned;

    public int gold;
    public System.Action<int, int> OnGoldChange;
    public float fuel, maxFuel = 100;
    public System.Action<float, float> OnFuelChange;

    public float air, maxAir= 100;
    public float airConsumptionPerFrame = 1;
    public float airDamageThreshold;
    public int airDamage = 5;
    float lastAirDamageTime;
    public System.Action<float, float> OnAirChange;

    [Header("Explosion")]
    public float explosionRange;
    public float explosionDuration;
    public AnimationCurve explosionCurve;
    public float explosionCooldown;
    public SpriteRenderer explosionSprite;
    public Gradient explosiongradient;
    public bool exploding;
    public float fuelConsumptionPerExplosion = 30;


    [Header("Death")]
    public GameObject ragdoll;
    public Rigidbody rb_drill, rb_pogo;
   
    public float explosionForce = 5;
    public float explosionRadius = 10;
    public GameObject deathFX;
    public System.Action OnGameOver;

    public static Pogo _;

    [Header("Jetpack")]
    public GameObject jetpackExhaustFX;
    public float jetpackSpeed = 5;
    public float jetpackFuelConsumptionPerFrame = 2;
    public float jetpackFuel, maxJetpackFuel = 100;
    bool boosting;
    public float jetPackAccel = 4, jetPackDecel = 12;
    public Vector3 jetPackVelocity;
    public System.Action<float, float> OnJetpackChange;

    public float maxY;

    private void Awake()
    {
        _ = this;
       
    }

    // Start is called before the first frame update
    void Start()
    {
        depthOffset = transform.position.y;
        jetpackFuel = maxJetpackFuel;
        ragdoll.gameObject.SetActive(false);
        rb_drill.gameObject.SetActive(false);
        rb_pogo.gameObject.SetActive(false);
        deathFX.SetActive(false);
        jetpackExhaustFX.SetActive(false);
        explosionSprite.gameObject.SetActive(false);
        speedLines.SetActive(false);
        mat = body.sharedMaterial;
        hp = maxHP;
        fuel = maxFuel;
        air = maxAir;
        if (OnAirChange != null) OnAirChange(air, maxAir);
        if (OnHPChange != null) OnHPChange(hp, maxHP);
        goal = FindObjectOfType<GoalArea>();
    }

    // Update is called once per frame
    void Update()
    {
        if (returned) return;
        if (exploding) return;
        if (isDead) return;
        if (Time.timeScale < 1) return;
        float dt = Time.deltaTime;

        //if (Input.GetKeyDown(KeyCode.F))
        //    Die();
        depth = transform.position.y - depthOffset;

        if (depth <= maxDepth) maxDepth = depth;
        if (OnDepthChange != null) OnDepthChange(depth);

       
            if (Input.GetKey(KeyCode.A))
                Rotate(-1);
            else if (Input.GetKey(KeyCode.D))
                Rotate(1);
       
        else rotationValue = 0;

        if (Input.GetKeyDown(KeyCode.Space))
           StartCoroutine( Explode());

        UpdateAir();
        if (Input.GetKey(KeyCode.W)) Jetpack();
        else
        {
            boosting = false;
           
        }
        if(boosting)
        {
            jetPackVelocity = Vector3.MoveTowards(jetPackVelocity, Vector3.Lerp(Vector3.up, transform.up.normalized, .7f) * jetpackSpeed, dt * jetPackAccel * 100) * dt;
        }
        else
        {
            jetPackVelocity = Vector3.MoveTowards(jetPackVelocity, Vector3.zero, dt * jetPackDecel * 100) * dt;
            if (Input.GetKey(KeyCode.S)) Drill();
            else charging = false;
            if(Input.GetKeyUp(KeyCode.S))
            {
                drillFastEndTime = Time.time;
            }
        }

        g = Mathf.MoveTowards(g, gravity, dt * 12);
        rot = Mathf.MoveTowards(rot, boosting ? rotationValue/2 : rotationValue,  dt * rotationSpeed);
        //print("angle " + Vector3.Angle(Vector3.up, transform.up ) + ", euler Z " + transform.eulerAngles.z);
        
            transform.Rotate(Vector3.forward, rot, Space.World);
        Vector3 euler = transform.eulerAngles;
        
        if (euler.z < 100f)
            euler.z = Mathf.Clamp(euler.z, 0, 40);

        if (euler.z > 100)
            euler.z = Mathf.Clamp(euler.z, 320, 360);
        transform.eulerAngles = euler;
        velocity += dt * -Vector3.right * rot / 25;
        velocity -= Vector3.up * dt/(1/g);
        bool hitSomething = CheckDrillCollision();

        CheckBodyCollision();
       
            if (hitSomething)
            {
                if (!charging)
                {
                    SoundEffectManager._.RemoveSound("Fall");
                    velocity = (transform.up.normalized) * ((Time.time>= drillFastEndTime + highJumpThreshold) ? jumpHeight : jumpHeight * 1.2f);
                    drillScaleLerp = 0;
                    g = 0;

                }
            }
       
        prevPos = transform.position;
        if (charging && hitSomething)
        {
            return;
        }
        velocity += jetPackVelocity;
        Vector3 p = prevPos + velocity;
        if (p.y > maxY) p.y = maxY;
        transform.position = p;
       

        if (drillScaleLerp<= 1f) drillScaleLerp +=  dt * 2;
        posDelta = (transform.position - prevPos).magnitude;
        if (transform.position.y < prevPos.y) fallDuration += Time.deltaTime;
        else fallDuration = 0;
       

    }

    private void LateUpdate()
    {
        Animations();
    }



    void Animations()
    {
        if(!charging && !boosting) CameraControl._.CameraOffset(Vector3.zero);
        speedLines.transform.Rotate(Vector3.up, drillRotSpeed * Time.deltaTime);
        speedLines.SetActive(charging);
        jetpackExhaustFX.SetActive(boosting);
        drill.Rotate(-Vector3.forward, drillRotSpeed * Time.deltaTime);
        drill.transform.localScale = new Vector3(1, 1, drillScaleCurve.Evaluate(drillScaleLerp));
        anim.SetFloat("Rotation", rot);
        anim.SetFloat("Velocity", Mathf.Clamp((transform.position - prevPos).y * yVelocityMultiplier, -1, 1));

      if(!boosting)  SoundEffectManager._.RemoveSound("Jetpack");
        if (!charging)
        {
            SoundEffectManager._.RemoveSound("Drill");
            if (fallDuration >= .5f) SoundEffectManager._.CreateSound("Fall");
            else SoundEffectManager._.RemoveSound("Fall");
        }

        pointer.gameObject.SetActive((goal.transform.position - transform.position).magnitude >= 10);
        pointer.up = (goal.transform.position - transform.position).normalized;


    }

    public void Rotate(float rotValue)
    {
        rotationValue = rotValue;
       
    }

    public bool CheckDrillCollision()
    {
        Vector3 rayStart = transform.position - (transform.up * rayOffset);
        Debug.DrawLine(rayStart, rayStart + (-transform.up * (rayLength + posDelta)) , Color.red);
        RaycastHit hit = new RaycastHit();
        //if (Physics.SphereCast(new Ray(rayStart, -transform.up), rayRadius,out hit, (rayLength + posDelta), groundMask, QueryTriggerInteraction.Collide))
        //{
        //    GroundBlock g = hit.collider.GetComponent<GroundBlock>();
        //    if (g != null)  g.GetDrilled(transform.position - hit.point);
        //    drillFX.transform.position = rayStart + (-transform.up * (rayLength + posDelta));
        //    drillFX.SetActive(false);
        //    drillFX.SetActive(true);
        //    if (hit.collider.GetComponent<Lava>()?.GetComponent<HurtBox>())
        //    {
        //        TakeDamage(hit.collider.GetComponent<Lava>().GetComponent<HurtBox>().damage, hit.point);
        //    }


        Collider[] cc = Physics.OverlapSphere(rayStart, rayRadius, groundMask, QueryTriggerInteraction.Collide);
            if(cc.Length > 0)
            {
            foreach (Collider c in cc)
            {
                GroundBlock g = c.GetComponent<GroundBlock>();
                if (g != null) g.GetDrilled(transform.position - hit.point, drillPower);
                drillFX.transform.position = rayStart + (-transform.up * (rayLength + posDelta));
                drillFX.SetActive(false);
                drillFX.SetActive(true);
                if (c.GetComponent<Lava>()?.GetComponent<HurtBox>())
                {
                    TakeDamage(c.GetComponent<Lava>().GetComponent<HurtBox>().damage, hit.point);
                    return false;
                }
            }
 
            if (!charging) SoundEffectManager._.CreateSound("Pogo boing");
            SoundEffectManager._.CreateSound("Drill hit");
            Debug.DrawLine(rayStart, rayStart - (transform.up * (rayLength + posDelta)), Color.green);
            return true;
       
        }
        return false;
    }

    #region health
    [Header("Body collisions")]
    public float collisionStart; 
    public float collisionHeight; 
    public float collisionRadius;

    public bool CheckCollision()
    {
        Vector3 rayStart = transform.position + (transform.up * collisionStart);
        //Debug.DrawLine(rayStart, rayStart + (transform.up * (collisionHeight + posDelta)), Color.cyan);
        RaycastHit hit = new RaycastHit();
        if (Physics.SphereCast(new Ray( rayStart, transform.up), collisionRadius, out hit, (collisionHeight + posDelta), groundMask))
        {

            Debug.DrawRay(hit.collider.transform.position, hit.normal, Color.red, 1);
            Debug.DrawRay(transform.position, transform.up, Color.blue, 1);
            print(string.Format("dot {0}", Vector3.Dot(transform.up, hit.normal)));

            if (hit.collider.GetComponent<HurtBox>())
            {
                TakeDamage(hit.collider.GetComponent<HurtBox>().damage, hit.point);
            }
            velocity = hit.normal * Time.deltaTime * 2;
            //Debug.DrawLine(rayStart, rayStart + (transform.up * (collisionHeight + posDelta)), Color.black);
            return true;


        }
        return false;
    }

    public bool CheckBodyCollision()
    {
        Vector3 rayStart = transform.position + (transform.up * collisionStart);
        //Debug.DrawLine(rayStart, rayStart + (transform.up * (collisionHeight + posDelta)), Color.cyan);
        RaycastHit hit = new RaycastHit();
        Vector3 bumpVelocity = Vector3.zero;

        Collider[] colliders = Physics.OverlapBox(rayStart + (transform.up * (collisionHeight + posDelta) / 2), new Vector3((collisionRadius / 2), (collisionHeight + posDelta) / 2, 1), transform.rotation, groundMask);
            foreach (Collider c in colliders)
            {
                Vector3 dir = (rayStart + (transform.up * (collisionHeight + posDelta) - c.transform.position)).normalized;
                if (c.GetComponent<HurtBox>())
                {
                    TakeDamage(c.GetComponent<HurtBox>().damage, hit.point);
                }
            bumpVelocity += dir;
            }
        if(colliders != null && colliders.Length >0)   velocity = bumpVelocity * Time.deltaTime * 2;
        return colliders != null;
           

        //if (Physics.SphereCast(new Ray(rayStart, transform.up), collisionRadius, out hit, (collisionHeight + posDelta), groundMask))
        //{

        //    Debug.DrawRay(hit.collider.transform.position, hit.normal, Color.red, 1);
        //    Debug.DrawRay(transform.position, transform.up, Color.blue, 1);
        //    print(string.Format("dot {0}", Vector3.Dot(transform.up, hit.normal)));

        //    if (hit.collider.GetComponent<HurtBox>())
        //    {
        //        TakeDamage(20, hit.point);
        //    }
        //    velocity = hit.normal * Time.deltaTime * 2;
        //    //Debug.DrawLine(rayStart, rayStart + (transform.up * (collisionHeight + posDelta)), Color.black);
        //    return true;


        //}
        //return false;
    }



    public void TakeDamage(int dmg, Vector3 hitPos)
    {
        if (Time.time >= lastHitTime + iFrameDuration)
        {
            SoundEffectManager._.CreateSound("Hit");
            hitFX.transform.position = hitPos;
            hitFX.SetActive(false);

            hitFX.SetActive(true);
            lastHitTime = Time.time;
            StartCoroutine(HitFlash());
            hp = Mathf.Clamp(hp -= dmg, 0, maxHP);
            if (OnHPChange != null) OnHPChange(hp, maxHP);
            if (hp <= 0) Die();
        }

    }

    IEnumerator HitFlash()
    {
        body.sharedMaterial = white;
        yield return new WaitForSeconds(.1f);
        body.sharedMaterial = mat;


    }

    public void Die()
    {
        SoundEffectManager._.RemoveSound("Fall");
        isDead = true;
        boosting = false;
        charging = false;
        exploding = false;
        CameraControl._.Shake();
        drillFX.SetActive(false);
        speedLines.SetActive(false);
        foreach (Collider collider in GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }
        transform.GetChild(0).gameObject.SetActive(false);
        ragdoll.gameObject.SetActive(true);
        rb_drill.gameObject.SetActive(true);
        rb_pogo.gameObject.SetActive(true);

        deathFX.SetActive(true);
        deathFX.transform.position = transform.position;
        ragdoll.transform.position = transform.position;
        rb_drill.transform.position = transform.position + Vector3.up * -.1f;
        rb_pogo.transform.position = transform.position + Vector3.up * -.1f;

        Rigidbody[] ragd = ragdoll.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in ragd)
        {
            rb.AddRelativeForce(Random.insideUnitSphere * 2, ForceMode.Impulse);
        }

        rb_drill.AddExplosionForce(explosionForce, transform.position, 12);
        rb_pogo.AddExplosionForce(explosionForce, transform.position, 12);
        if (OnGameOver != null) OnGameOver();

    }
    #endregion

    #region changeStats

    public void Heal(int val)
    {
        print("aaa hp");
        hp = Mathf.Clamp( hp += val, 0, maxHP);
       if (OnHPChange != null) OnHPChange(hp, maxHP);
    }

    public void GetGold(int val)
    {
        print("aaa gold");
        gold += val;
        if (OnGoldChange != null) OnGoldChange(gold, val);
    }

    public void GetFuel(int val)
    {
        print("aaa fuel");
        fuel = Mathf.Clamp(fuel += val, 0, maxFuel);
        if (OnFuelChange != null) OnFuelChange(fuel, maxFuel);
    }

    public void GetAir(float val)
    {
        print("aaa air");
        air = Mathf.Clamp(air += val, 0, maxAir);
        if (OnAirChange != null) OnAirChange(air, maxAir);
    }

    void UpdateAir()
    {
        if (depth > -5f)

        {
            air = Mathf.Clamp(air += Time.deltaTime * (airConsumptionPerFrame * 5), 0, 100);
            if (OnAirChange != null) OnAirChange(air, maxAir);
            return;
        }

        if(air >= 0)
        {
            //print(airConsumptionPerFrame + (Mathf.Abs(depth / 100f)));
            air -= Time.deltaTime * ( airConsumptionPerFrame + ( Mathf.Abs(depth)/100f));
            if (OnAirChange != null) OnAirChange(air, maxAir);
        }
        else
        {
            if(Time.time >= lastAirDamageTime + airDamageThreshold)
            {
                TakeDamage(airDamage, transform.position);
                lastAirDamageTime = Time.time;
            }
        }
    }
    #endregion

    #region special moves
    public void Drill()
    {
        if (fuel <= 0) return;
        fuel -= Time.deltaTime * fuelConsumptionPerFrame;
        if (OnFuelChange != null) OnFuelChange(fuel, maxFuel);
        if(charging == false)
        {
            velocity = Time.deltaTime / 10 * -transform.up * drillSpeed;
        }
        SoundEffectManager._.CreateSound("Drill", 2);
        charging = true;
        velocity += Time.deltaTime/ 10 * -transform.up * drillSpeed;
        CameraControl._.CameraOffset(Vector3.down * 2);
    }

    IEnumerator Explode()
    {
        if (fuel <= fuelConsumptionPerExplosion) yield break;
        fuel = Mathf.Clamp(fuel -= fuelConsumptionPerExplosion, 0, maxFuel);
        if (OnFuelChange != null) OnFuelChange(fuel, maxFuel);
        exploding = true;
        velocity = Vector3.zero;
        SoundEffectManager._.RemoveSound("Fall");
        explosionSprite.gameObject.SetActive(true);
        float i = 0;
        CameraControl._.Shake();
        SoundEffectManager._.CreateSound("Explosion");
        while (i <= 1f)
        {
            explosionSprite.color = explosiongradient.Evaluate(i);
            float range = explosionCurve.Evaluate(i) * explosionRange;
            explosionSprite.transform.localScale = Vector3.one * range;
            i += Time.deltaTime / explosionDuration;
            explosionSprite.material.SetFloat("_Mask_Ratio", i);
            Collider[] c = Physics.OverlapSphere(transform.position, range / 2, groundMask);
            foreach (Collider collider in c)
            {
                GroundBlock g = collider.GetComponent<GroundBlock>();
                if (g != null) g.GetDestroyed(g.transform.position - transform.position);
            }

            yield return null;
        }
        yield return new WaitForSeconds(explosionCooldown);
        exploding = false;
        explosionSprite.gameObject.SetActive(false);
    }

    public void Jetpack()
    {
        if (jetpackFuel <= 0) return;
        charging = false;
        jetpackFuel -= Time.deltaTime * jetpackFuelConsumptionPerFrame;
        if (OnJetpackChange != null) OnJetpackChange(jetpackFuel, maxJetpackFuel);
       // if (OnFuelChange != null) OnFuelChange(fuel, maxFuel);
        if (boosting == false)
        {
            g = 0;
            SoundEffectManager._.CreateSound("Burst");
            SoundEffectManager._.CreateSound("Jetpack");
            velocity = Time.deltaTime / 10 * transform.up * jetpackSpeed;
        }
        boosting = true;
        CameraControl._.CameraOffset(Vector3.up * 9);

    }

    #endregion

    private void OnCollisionStay(Collision collision)
    {
        
    }

    private void OnDrawGizmos()
    {
        Vector3 rayStart = transform.position - (transform.up * rayOffset);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(rayStart, rayStart + (-transform.up *( rayLength + posDelta)));
        Gizmos.DrawWireSphere(rayStart, rayRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + (transform.up.normalized) * collisionStart, transform.position + (transform.up.normalized) * (collisionStart + collisionHeight));
        Gizmos.DrawWireSphere(transform.position + (transform.up.normalized) * collisionStart,collisionRadius);
        Gizmos.DrawWireSphere(transform.position + (transform.up.normalized) * (collisionStart + collisionHeight), collisionRadius);
    }
}
