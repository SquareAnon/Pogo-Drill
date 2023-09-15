using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType {ground, bomb, metal}
public enum GemType
{ none, red, blue, green, gold, diamond}; 
public class GroundBlock : MonoBehaviour
{
    public BlockType type;
    public Material white;
    public GameObject drilledFX, damageFX;
    public int maxHP = 1, hp;
    protected bool iFrame;
    public Vector2Int xy;

    public List<BlockVisuals> visuals;
    public Mesh bombMesh, metalMesh;
    public MeshFilter crack;
    public Mesh[] crackMeshes;

    [Header("Gem")]
    public GemType gemType;
    public MeshFilter gemMeshFilter;
    public Mesh[] gemMeshes = new Mesh[6];

    public GameObject[] gemFXPrefabs = new GameObject[6];
    public MeshFilter propFilter;
    public Mesh[] propMeshes;



    [Header("Explosion")]
    public float explosionRange;
    public float explosionDuration;
    public AnimationCurve explosionCurve;
    public float explosionCooldown;
    public float explosionDelay;
    public SpriteRenderer explosionSprite;
    public Gradient explosiongradient;
    public bool exploding;
    public LayerMask playerGroundMask;


    public void Init(int x, int y)
    {

        this.xy = new Vector2Int(x,y);

        UpdateDictionary();
        explosionSprite.gameObject.SetActive(false);
        gemMeshFilter.mesh = null;
        crack.mesh = null;
        switch (type)
        {
            case BlockType.ground:

                gemMeshFilter.mesh = gemMeshes[(int)gemType];
                hp = maxHP;
                if (hp > 0) crack.mesh = crackMeshes[Mathf.Clamp(maxHP - hp, 0, crackMeshes.Length)];
                if (visuals.Count >= maxHP && visuals[maxHP].meshes.Length > 0) GetComponent<MeshFilter>().mesh = visuals[maxHP].meshes[Random.Range(0, visuals[maxHP].meshes.Length)];
                break;
            case BlockType.bomb:
                 GetComponent<MeshFilter>().mesh = bombMesh;
                break;
            case BlockType.metal:
                GetComponent<MeshFilter>().mesh = metalMesh;
                break;
            default:
                break;
        }
    }

    public void UpdateDictionary()
    {
        if (!Cave._.blockDataDict.ContainsKey(xy)) Cave._.blockDataDict.Add(new Vector2Int(xy.x, xy.y), new BlockData(hp, maxHP, type, gemType));
        else Cave._.blockDataDict[xy] = new BlockData(hp, maxHP, type, gemType);
    }

    public void AddProp()
    {
        if(Random.Range(0, 100f) >= 70f) propFilter.mesh = propMeshes[Random.Range(0, propMeshes.Length)]; 
    }


    public virtual bool GetDrilled(Vector3 direction, int dmg)
    {
        if (iFrame) return true;
        switch (type)
        {
            case BlockType.ground:
                //UpdateDictionary();
                hp-= dmg;
                if (hp > 0) crack.mesh = crackMeshes[Mathf.Clamp(maxHP - hp, 0, crackMeshes.Length)];


                if (hp <= 0)
                {
                    GetDestroyed(direction);
                    return true;
                }
                else StartCoroutine(Hit(direction));
                return false;
              
            case BlockType.bomb:
                GetDestroyed(direction);
                return true;
               
            case BlockType.metal:
                SoundEffectManager._.CreateSound("Hit metal");
                return true;
                
            default:
                break;

        }
        return false;


      

    }

    public virtual void GetDestroyed(Vector3 direction)
    {
        if ((transform.position - FindObjectOfType<Pogo>().transform.position).magnitude >= 24) return;
        switch (type)
        {
            case BlockType.ground:
                Cave._.blocksDrilled++;
                StartCoroutine(Hit(direction));
                SoundEffectManager._.CreateSound("Hit rock");
                GameObject fx = gemFXPrefabs[(int)gemType];
                if (fx != null) Destroy(Instantiate(fx, transform.position, Quaternion.identity), 1f);
                switch (gemType)
                {
                    case GemType.none:
                        break;
                    case GemType.red:
                        Pogo._.Heal(20);
                        SoundEffectManager._.CreateSound("Regen health");
                        break;
                    case GemType.blue:
                        Pogo._.GetAir(20);
                        SoundEffectManager._.CreateSound("Regen air");
                        break;
                    case GemType.green:
                        Pogo._.GetFuel(20);
                        SoundEffectManager._.CreateSound("Regen special");
                        break;
                    case GemType.gold:
                        Pogo._.GetGold(10);
                        SoundEffectManager._.CreateSound("Get gold");
                        break;
                    case GemType.diamond:
                        Pogo._.gotDiamond = true;
                        // GetDestroyed(direction);
                        break;
                    default:
                        break;
                }
                if (gemType != GemType.none) SoundEffectManager._.CreateSound("Gem");
                iFrame = false;
                Collider c = GetComponent<Collider>();
                c.enabled = false;
                Destroy(Instantiate(drilledFX, transform.position, Quaternion.Euler(direction)), 1f);
                Cave._.Remove(this);
                if (Cave._.blockDataDict.ContainsKey(xy)) Cave._.blockDataDict.Remove(xy);
                break;

            case BlockType.bomb: 
                StartCoroutine(Explode());
                break;

            case BlockType.metal:
                return;
               

            default:
                break;
        }


        
       

    }

    protected IEnumerator Hit(Vector3 direction)
    {
        iFrame = true;
        Collider c = GetComponent<Collider>();
        //c.enabled = false;
        Renderer r = GetComponent<Renderer>();
        Material m = r.sharedMaterial;
        r.sharedMaterial = white;
        Destroy(Instantiate(damageFX, transform.position, Quaternion.Euler(direction)), .4f);
        yield return new WaitForSeconds(.1f);
        r.sharedMaterial = m;
        c.enabled = true;
        iFrame = false;
    }

    IEnumerator Explode()
    {

        Collider co = GetComponent<Collider>();
        co.enabled = false;
        exploding = true;
        Renderer r = GetComponent<Renderer>();

        Material m = r.sharedMaterial;

        for (int l = 0; l < 3; l++)
        {
            r.sharedMaterial = white;
            SoundEffectManager._.CreateSound("Tic tac", 0);
            yield return new WaitForSeconds(explosionDelay / 4);
            r.sharedMaterial = m;
            SoundEffectManager._.CreateSound("Tic tac", 1);
            yield return new WaitForSeconds(explosionDelay / 4);
        }
        r.sharedMaterial = white;

        yield return new WaitForSeconds(explosionDelay);
        CameraControl._.Shake();
        GetComponent<MeshFilter>().mesh = null;
        Cave._.blocksDrilled++;
        explosionSprite.gameObject.SetActive(true);
        SoundEffectManager._.CreateSound("Explosion");
        float i = 0;
        while (i <= 1f)
        {
            explosionSprite.color = explosiongradient.Evaluate(i);
            float range = explosionCurve.Evaluate(i) * explosionRange;
            explosionSprite.transform.localScale = Vector3.one * range;
            i += Time.deltaTime / explosionDuration;
            explosionSprite.material.SetFloat("_Mask_Ratio", i);
            Collider[] c = Physics.OverlapSphere(transform.position, range / 2, playerGroundMask);
            foreach (Collider collider in c)
            {
                GroundBlock g = collider.GetComponent<GroundBlock>();
                if (g != null) g.GetDestroyed(g.transform.position - transform.position);
                Pogo p = collider.GetComponent<Pogo>();
                if (p != null) p.TakeDamage(50, p.transform.position);
            }

            yield return null;
        }
        yield return new WaitForSeconds(explosionCooldown);
        exploding = false;
        explosionSprite.gameObject.SetActive(false);
        iFrame = false;

        // Destroy(Instantiate(drilledFX, transform.position, Quaternion.Euler(direction)), 1f);
        Cave._.Remove(this);
        if (Cave._.blockDataDict.ContainsKey(xy)) Cave._.blockDataDict.Remove(xy);

    }



    [System.Serializable]
    public class BlockVisuals
    {
        public string blockName;
        public Mesh[] meshes;
      
    }
}
