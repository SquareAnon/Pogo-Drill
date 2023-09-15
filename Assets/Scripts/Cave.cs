using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Unity.Mathematics;
using System;
using UnityEngine.Rendering.Universal;

public class Cave : MonoBehaviour
{

    public static Cave _;
    public int blocksDrilled;

    public Lava lava;
    [Header("Prefabs")]
    public GroundBlock blockPrefab;
    public HurtBox stalatitesPrefab;

    public Dictionary<Vector2Int, BlockData> blockDataDict = new Dictionary<Vector2Int, BlockData>();

    [Header("Chunk")]
    public int chunkHeight;
    public int currentChunk, previousChunk, lastChunk;



    [Header("Cave Parameters")]
    public float startX;
    public float startY;
    public int sizeX = 20, sizeY = 100;
    public float noiseScale = 50;

    [Header("Special block Chances")]
    public float bombChance = 99f;
    public float metalChance = 60f;
    public float stalagmiteChance = 60f;
    public float diamongChance = 990f;

    [Header("Gem Chances")]
    public float redChance;
    public float blueChance;
    public float greenChance;
    public float goldChance;
    bool spawnedDiamond;
    public int diamondDepthThreshold;

    public ObjectPool<GroundBlock> groundBlockPool;
    public bool usePool;
   [SerializeField] List<GroundBlock> groundBlocks;


    private void Awake()
    {
        _ = this;
        groundBlocks = new List<GroundBlock>(); 
        

    }

    private void Update()
    {
        if (Mathf.Clamp( Mathf.Abs((int)(Pogo._.transform.position.y / chunkHeight)), 0, 999999) != currentChunk)
        {
            
            previousChunk = currentChunk;
            currentChunk = Mathf.Clamp(Mathf.Abs((int)(Pogo._.transform.position.y / chunkHeight)), 0, 999999);
          if(  previousChunk > lastChunk) lastChunk = previousChunk; 
            LoadUnloadChunk(previousChunk, currentChunk);


        }
    }


    public void LoadUnloadChunk(int oldChunk, int newChunk)
    {
        if (!usePool) return;
       
       int newStartY = currentChunk * chunkHeight ;
        int oldStartY = previousChunk * chunkHeight;
        print("loading new chunk" );

        for (int y = oldStartY; y < oldStartY + chunkHeight; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                foreach (GroundBlock item in groundBlocks)
                {
                    if(item.xy == new Vector2Int(x, y))
                        Remove(item);
                }


            }
        }
        print("block dictionary has " + blockDataDict.Keys.Count + " keys.");
        for (int y = newStartY; y < newStartY + chunkHeight; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                if (blockDataDict.ContainsKey(new Vector2Int(x, y)))
                    GenerateBlock(x, y, blockDataDict[new Vector2Int(x, y)]);
             


            }
        }
    }


    void GenerateBlock(int x, int y, BlockData bd)
    {
        print("pool size " + groundBlockPool.CountAll);
        GroundBlock g = usePool ? groundBlockPool.Get() : Instantiate(blockPrefab, new Vector3(startX + x, startY - y, 0), Quaternion.identity, transform);
            g.transform.position = new Vector3(startX + x, startY -  y, 0);
            g.maxHP = bd.maxHP;
            g.hp = bd.hp;
            g.gemType = bd.gemType;
            g.type = bd.type;
            g.Init(x, y);
        if (g != null) groundBlocks.Add(g);
        
    }


    // Start is called before the first frame update
    void Start()
    {
        groundBlockPool = new ObjectPool<GroundBlock>(() =>
        {
            return Instantiate(blockPrefab);
        }, block =>
        {
            block.gameObject.SetActive(true);
        }, block =>
        {
            block.gameObject.SetActive(false);
        }, block =>
        {
            Destroy(block.gameObject);
        }, false, 300, 400);

        CreateCave();

        groundBlocks.AddRange(FindObjectsOfType<GroundBlock>(true));
    }

    int RandomGem(float rndm)
    {
        if (rndm <= goldChance) return 4;
        if (rndm <= greenChance) return 3;
        if (rndm <= blueChance) return 2;
        if (rndm <= redChance) return 1;
        return 0;
    }

    void CreateCave()
    {
        lava.transform.position = new Vector3(startX + (sizeX / 2), startY - sizeY - 50, 0);
        UnityEngine.Random.InitState(PlayerPrefs.GetInt("high score"));

        print(groundBlockPool.CountAll);
        //border
        for (int y = -10; y < sizeY; y++)

        {
            GenerateBlock((int)startX - 1, (int)startY - y, BlockType.metal, false);
            ///GenerateBlock((int)startX - 2, (int)startY - y, BlockType.metal, false);
            GenerateBlock((int)startX + sizeX, (int)startY - y, BlockType.metal, false);
            ///GenerateBlock((int)startX + (int)sizeX + 1, (int)startY - y, BlockType.metal, false);
        }

        for (int x = 0; x < sizeX; x++)
        {
            GenerateBlock((int)startX + x, (int)startY - (int)sizeY, BlockType.metal, false);
        }

        //cave
        for (int y = 0; y < chunkHeight; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                GenerateBlock(x, y, true);
            }
        }
    }


    private void OnDrawGizmos()
    {

        Gizmos.DrawWireCube(new Vector3(startX + sizeX / 2, startY - sizeY / 2, 1), new Vector3(sizeX, sizeY, 1));

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(startX, startY - sizeY + diamondDepthThreshold, 1), 3);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(startX, startY - sizeY + (diamondDepthThreshold + 20), 1), 3);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(startX, startY - (currentChunk * chunkHeight), 1), 2);
        Gizmos.DrawWireSphere(new Vector3(startX, startY - (currentChunk * chunkHeight) - chunkHeight, 1), 2);

    }

    public void GenerateBlock(int x, int y, BlockType type, bool pooled)
    {
        GroundBlock g = null;
        if (pooled)
        {

        }
        else
        {
             g = Instantiate(blockPrefab, new Vector3(x, y, 0), Quaternion.identity, transform);

            g.type = type;
            g.Init(x, y);
        }
        if (g != null) groundBlocks.Add(g);


    }

    public void GenerateBlock(int x, int y, bool pooled)
    {
        float xCoord = (float)(x) / (float)sizeX * noiseScale;
        float yCoord = (float)(y) / (float)sizeX * noiseScale;
        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        float sampleinverse = Mathf.PerlinNoise(yCoord, xCoord);
        int gem = RandomGem(UnityEngine.Random.Range(0, 100f));
        float random = UnityEngine.Random.Range(0, 100f);
        //print(sample);
        bool bomb_ = (random >= bombChance);
        //print(sample);
        GroundBlock g = null;
        if (sample <= .4f)
        {
            return;
        }
       
        else if (sample <= .5f)
        {

            g = pooled ? groundBlockPool.Get() : Instantiate(blockPrefab, new Vector3(startX + x, startY - y, 0), Quaternion.identity, transform);
            g.transform.position = new Vector3(startX + x, startY - y, 0);
            g.maxHP = 1;
            g.gemType = (GemType)gem;
            g.type = BlockType.ground;
            g.Init(x, y);
            //float yCoord_ = (float)(y - 1) / (float)sizeX * noiseScale;
            //float samplebelow = Mathf.PerlinNoise(xCoord, (float)(y + 1) / (float)sizeX * noiseScale);
            //float sampleabove = Mathf.PerlinNoise(xCoord, (float)(y - 1) / (float)sizeX * noiseScale);
            //if (samplebelow <= .4f && Random.Range(0, 100f) >= stalagmiteChance) Instantiate(stalatitesPrefab, new Vector3(startX + x, startY - y - 1, 0), Quaternion.identity, g.transform);
            //if (!bomb_) g.gemType = (GemType)gem;
            //if (!bomb_ && sampleabove <= .4f) g.AddProp();

        }
        else if (sample <= .7f)
        {

             g = pooled ? groundBlockPool.Get() : Instantiate(blockPrefab, new Vector3(startX + x, startY - y, 0), Quaternion.identity, transform);
            g.transform.position = new Vector3(startX + x, startY - y, 0);
            g.maxHP = 2;
            g.gemType = (GemType)gem;
            g.type = bomb_ ? BlockType.bomb : BlockType.ground;
            g.Init(x, y);

        }
        else if (sample <= .9f)
        {
            g = pooled ? groundBlockPool.Get() : Instantiate(blockPrefab, new Vector3(startX + x, startY - y, 0), Quaternion.identity, transform);
            g.transform.position = new Vector3(startX + x, startY - y, 0);
            g.maxHP = 2;
            g.type = random >= metalChance ? BlockType.metal : BlockType.ground;
            bool isDiamond = g.type == BlockType.ground && !spawnedDiamond && y >= sizeY - diamondDepthThreshold && y < (sizeY - diamondDepthThreshold + 20) && !spawnedDiamond && (sample * 1000f) >= diamongChance;

            g.gemType = isDiamond ? GemType.diamond : (GemType)gem;
            if (isDiamond) spawnedDiamond = true;
            g.Init(x, y);
        }
        if (g != null) groundBlocks.Add(g);
    }

    public void Remove(GroundBlock block)
    {
        Debug.Log("removing", block);
        block.UpdateDictionary();
        if (usePool) groundBlockPool.Release(block);
       
        // if (groundBlocks.Contains(block)) groundBlocks.Remove(block);
        //else Destroy(block.gameObject, .1f);
    }


 
}

[System.Serializable]
public class BlockData
{
    public int hp, maxHP;
    public BlockType type;
    public GemType gemType;

    public BlockData(int _hp, int _maxHP, BlockType _type, GemType _gemType)
    {
        hp = _hp;
        maxHP = _maxHP;
        type = _type;
        gemType = _gemType;

    }

}

