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
    public bool hasLava;
    [Header("Prefabs")]
    public GroundBlock blockPrefab;
    public HurtBox stalatitesPrefab;

    public Dictionary<Vector2Int, Block> blockDataDict = new Dictionary<Vector2Int, Block>();

    [Header("Chunk")]
    public int chunkHeight;
    public int currentChunk, previousChunk, prevPreviousChunk = -1;



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
   [SerializeField] List<List<Block>> groundBlockChunks;


    private void Awake()
    {
        _ = this;
        groundBlockChunks = new List<List<Block>>();
        lava.gameObject.SetActive(hasLava);

    }

    private void Update()
    {
        float realYPos = Pogo._.transform.position.y;
        if (realYPos > 0) realYPos = 0;
        int chunkPos = (int)MathF.Abs(realYPos/chunkHeight) ;
        if (chunkPos != currentChunk)
        {

            prevPreviousChunk = previousChunk;
            previousChunk = currentChunk;
            currentChunk = chunkPos;
          //if(  previousChunk > lastChunk) lastChunk = previousChunk; 
            LoadUnloadChunk(prevPreviousChunk, currentChunk);


        }
    }


    public void LoadUnloadChunk(int oldChunk, int newChunk)
    {
        int newStartY = newChunk * chunkHeight;
        int oldStartY = oldChunk * chunkHeight;
        if (!usePool) return;
        if(oldChunk == newChunk) return;
        if (groundBlockChunks.Count < 1) return;
        print("there are " + groundBlockPool.CountAll + " blocks in the pool");

        if (groundBlockChunks.Count >= oldChunk && oldChunk >= 0)
        {
            print("there are " + groundBlockChunks.Count + " chunks, unloading" + oldChunk);
            foreach (var item in groundBlockChunks[oldChunk])
            {
                item.Deactivate();
            }
        }

        if (groundBlockChunks.Count > newChunk)
        {
            foreach (var item in groundBlockChunks[newChunk])
            {
                item.Activate();
            }
        }
        else
        {
            GenerateNewChunk(newChunk);

        }

     
       

    }

    void GenerateNewChunk(int chunk)
    {
        groundBlockChunks.Add(new List<Block>());
        //loop through x and y and create new blocks and add them to the chunk
        for (int y = 0; y < chunkHeight; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                Block b = GenerateBlock(x, chunk * chunkHeight + y, true);
                if(b!= null)
                groundBlockChunks[chunk].Add(b);
            }
        }
    }


    void GenerateBlock(int x, int y, Block bd)
    {
        //print("pool size " + groundBlockPool.CountAll);
        //GroundBlock g = usePool ? groundBlockPool.Get() : Instantiate(blockPrefab, new Vector3(startX + x, startY - y, 0), Quaternion.identity, transform);
        //    g.transform.position = new Vector3(startX + x, startY -  y, 0);
          
        //    g.Init(x, y, bd);

        //Block b = new Block(1, 1, BlockType.ground, (GemType)gem);
        bd.xy = new Vector2Int(x, y);   
        bd.Activate();


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
        }, true, 300, 400);

        CreateCave();

     
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

        //print(groundBlockPool.CountAll);
        //border
        Block metalWall = new Block(new Vector2Int(0, 0), 1, 1, BlockType.metal, GemType.none);
        for (int y = -10; y < sizeY; y++)
        {
            GenerateBlock((int)startX - 1, (int)startY - y, metalWall);
            GenerateBlock((int)startX + sizeX, (int)startY - y, metalWall);
        }
        for (int x = 0; x < sizeX; x++)
            GenerateBlock((int)startX + x, (int)startY - (int)sizeY, metalWall);


        ////cave
        //for (int y = 0; y < chunkHeight; y++)
        //{
        //    for (int x = 0; x < sizeX; x++)
        //    {
        //       Block b = GenerateBlock(x, y, true);

        //    }
        //}

        GenerateNewChunk(0);
        GenerateNewChunk(1);
    }


    private void OnDrawGizmos()
    {

        Gizmos.DrawWireCube(new Vector3(startX + sizeX / 2, startY - sizeY / 2, 1), new Vector3(sizeX, sizeY, 1));

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(startX, startY - sizeY + diamondDepthThreshold, 1), 3);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(startX, startY - sizeY + (diamondDepthThreshold + 20), 1), 3);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(startX, startY - (currentChunk * chunkHeight), 1), 1);
        Gizmos.DrawWireSphere(new Vector3(startX, startY - (currentChunk * chunkHeight) - chunkHeight, 1), 1);

    }

  

    public Block GenerateBlock(int x, int y, bool pooled)
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
        Block b = null;
        if (sample <= .4f)
        {
            return null;
        }
       
        else if (sample <= .5f)
        {

            //g = pooled ? groundBlockPool.Get() : Instantiate(blockPrefab, new Vector3(startX + x, startY - y, 0), Quaternion.identity, transform);
            //g.transform.position = new Vector3(startX + x, startY - y, 0);
             b = new Block(new Vector2Int(x, y), 1, 1, BlockType.ground, (GemType)gem);
            b.Activate();
            //g.Init(x, y, b);
            //float yCoord_ = (float)(y - 1) / (float)sizeX * noiseScale;
            //float samplebelow = Mathf.PerlinNoise(xCoord, (float)(y + 1) / (float)sizeX * noiseScale);
            //float sampleabove = Mathf.PerlinNoise(xCoord, (float)(y - 1) / (float)sizeX * noiseScale);
            //if (samplebelow <= .4f && Random.Range(0, 100f) >= stalagmiteChance) Instantiate(stalatitesPrefab, new Vector3(startX + x, startY - y - 1, 0), Quaternion.identity, g.transform);
            //if (!bomb_) g.gemType = (GemType)gem;
            //if (!bomb_ && sampleabove <= .4f) g.AddProp();

        }
        else if (sample <= .7f)
        {

            // g = pooled ? groundBlockPool.Get() : Instantiate(blockPrefab, new Vector3(startX + x, startY - y, 0), Quaternion.identity, transform);
            //g.transform.position = new Vector3(startX + x, startY - y, 0);
             b = new Block(new Vector2Int(x, y), 2, 2, BlockType.ground, (GemType)gem);
            b.Activate();
            //g.Init(x, y, b);

        }
        else if (sample <= .9f)
        {
            //g = pooled ? groundBlockPool.Get() : Instantiate(blockPrefab, new Vector3(startX + x, startY - y, 0), Quaternion.identity, transform);
            //g.transform.position = new Vector3(startX + x, startY - y, 0);
            bool isDiamond = random < metalChance  && !spawnedDiamond && y >= sizeY - diamondDepthThreshold && y < (sizeY - diamondDepthThreshold + 20) && !spawnedDiamond && (sample * 1000f) >= diamongChance;
            if (isDiamond) spawnedDiamond = true;
             b = new Block(new Vector2Int(x, y), 3, 3, random >= metalChance ? BlockType.metal : BlockType.ground, isDiamond ? GemType.diamond : (GemType)gem);
            b.Activate();
            //g.Init(x, y, b);
        }

        return b;
        
    }

    public void Remove( Block bloc)
    {
        blockDataDict.Remove(bloc.xy);
        foreach (var item in groundBlockChunks)
        {
            if(item.Contains(bloc))
                item.Remove(bloc);
        }
        bloc.Deactivate();
       
        // if (groundBlocks.Contains(block)) groundBlocks.Remove(block);
        //else Destroy(block.gameObject, .1f);
    }


 
}

[System.Serializable]
public class Block
{
    public Vector2Int xy;
    public int hp, maxHP;
    public BlockType type;
    public GemType gemType;
    public GroundBlock instance;

    public Block(Vector2Int _xy, int _hp, int _maxHP, BlockType _type, GemType _gemType)
    {
        hp = _hp;
        maxHP = _maxHP;
        type = _type;
        gemType = _gemType;
        xy = _xy;

    }

    public void Activate()
    {
        GroundBlock g = Cave._.groundBlockPool.Get();
        instance = g;
        //Debug.Log(new Vector3(Cave._.startX + (float)xy.x, Cave._.startY - (float)xy.y, 0));
        g.transform.position = new Vector3(Cave._.startX + (float)xy.x, Cave._.startY - (float)xy.y, 0);
        g.Init(this);

    }

    public void Deactivate()
    {
         Cave._.groundBlockPool.Release(instance);
    }
}

