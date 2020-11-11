using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime;

//REFERENCE
//Coordinate Spaces
    //Similar to how 3D engines generally can reference Local, Global, Normal, and sometimes other more varied
    //coordinate spaces, so too does this game have different coordinate spaces. They are:
    //GLOBAL: The actual Unity coordinate system. The in-engine x,y,z position.
    //CHUNK LOCAL: The coordinates within the chunk itself of a position. 
        //Generally, think of this as GLOBAL mod 16, with some edge cases.
    //CHUNK POS: The coordinate system of managing chunks themselves. The best way to explain is through example: 
        //Chunks themselves are 16 blocks wide by default, and sequentially make up the entire terrain map. 
        //Given that each block is of 1 Global width in every dimension, this means that along the x-axis chunks are located at 0, 16, 32, 48, etc.
        //So for simplicity, we call these coordinates 0, 1, 2, 3, etc. 
    //NOTE THAT YOU CANNOT CONVERT FROM CHUNK LOCAL TO GLOBAL OR CHUNK POS. It's a one-way function.

public class TerrainGenerator : MonoBehaviour
{
    public int renderDistance;

    const int loadDelay = 5;
    private int chunkTimer = 0;

    public GameObject terrainChunk;

    //Dictionary of chunks, keyed by ChunkPos coords.
    public static Dictionary<Vector2Int, GameObject> chunks;
    //Dictionary of chunk block edits, keyed by ChunkPos coords.
    //Edits themselves stored as Dictionaries of block index integers keyed by Chunk Local coords.
    public static Dictionary<Vector2Int, Dictionary<Vector3Int, int>> blockChanges;

    //Queues for chunk loading/chunk deletion. Much faster than doing all at once.
    Queue<Vector2Int> toBeLoaded;
    Queue<Vector2Int> toBeDeleted;

    //Tracked player coordinates in Chunk Pos coordinates.
    Vector2Int playerChunkLoc;


    GameObject player;
    SimplexNoiseGenerator noiseGenerator;
    void Start()
    {
        //Init objects.
        chunks = new Dictionary<Vector2Int, GameObject>();
        blockChanges = new Dictionary<Vector2Int, Dictionary<Vector3Int, int>>();
        toBeLoaded = new Queue<Vector2Int>();
        toBeDeleted = new Queue<Vector2Int>();
        player = GameObject.FindGameObjectWithTag("Player");
        noiseGenerator = new SimplexNoiseGenerator();

        playerChunkLoc = Vector2Int.zero;

        //On Start, build chunk objects/block data out to the render distance.
        for (int x = -renderDistance; x < renderDistance; x++)
        {
            for (int z = -renderDistance; z < renderDistance; z++)
            {
                chunks.Add(new Vector2Int(x, z), buildChunkData(x, z));
            }
        }

        //Then, have those chunks build their meshes from block data.
        for (int x = -renderDistance; x < renderDistance; x++)
        {
            for (int z = -renderDistance; z < renderDistance; z++)
            {
                chunks[new Vector2Int(x, z)].GetComponent<TerrainChunk>().buildMesh();
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        //Get the players position
        int curX = player.transform.position.x >= 0 ? (int)(player.transform.position.x / TerrainChunk.chunkWidth) : (int)(player.transform.position.x / TerrainChunk.chunkWidth) - 1;
        int curZ = player.transform.position.z >= 0 ? (int)(player.transform.position.z / TerrainChunk.chunkWidth) : (int)(player.transform.position.z / TerrainChunk.chunkWidth) - 1;
        //if the players in a new chunk, queue up relevant added/deleted chunks and update player position.
        //Ergo, infinite terrain generation.
        if (curX != playerChunkLoc.x || curZ != playerChunkLoc.y) {
            if (curX < playerChunkLoc.x)
            {
                for (int i = -renderDistance; i < renderDistance; i++)
                {
                    toBeLoaded.Enqueue(new Vector2Int(curX - renderDistance, curZ + i));
                    toBeDeleted.Enqueue(new Vector2Int(curX + renderDistance, curZ + i));
                }
            } else if (curX > playerChunkLoc.x)
            {
                for (int i = -renderDistance; i < renderDistance; i++)
                {
                    toBeLoaded.Enqueue(new Vector2Int(curX + (renderDistance - 1), curZ + i));
                    toBeDeleted.Enqueue(new Vector2Int(curX - renderDistance - 1, curZ + i));
                }
            }

            if (curZ < playerChunkLoc.y)
            {
                for (int i = -renderDistance; i < renderDistance; i++)
                {
                    toBeLoaded.Enqueue(new Vector2Int(curX + i, curZ - renderDistance));
                    toBeDeleted.Enqueue(new Vector2Int(curX + i, curZ + renderDistance));
                }
            } else if (curZ > playerChunkLoc.y)
            {
                for (int i = -renderDistance; i < renderDistance; i++)
                {
                    toBeLoaded.Enqueue(new Vector2Int(curX + i, curZ + (renderDistance - 1)));
                    toBeDeleted.Enqueue(new Vector2Int(curX + i, curZ - renderDistance - 1));
                }
            }
            playerChunkLoc.x = curX;
            playerChunkLoc.y = curZ;
        }

        //Every time the chunkTimer fully ticks down, build/destroy the next chunk mesh in the toBeLoaded and toBeDeleted queue respectively.
        //MUCH faster computationally when set on a timer instead of done all at once.
        if (chunkTimer <= 0)
        {
            if (toBeLoaded.Count > 0)
            {
                Vector2Int coords = toBeLoaded.Dequeue();
                GameObject newChunk = buildChunkData(coords.x, coords.y);
                newChunk.GetComponent<TerrainChunk>().buildMesh();
                chunks.Add(coords, newChunk);

            }
            if (toBeDeleted.Count > 0)
            {
                Vector2Int coords = toBeDeleted.Dequeue();
                GameObject delChunk = chunks[coords];
                chunks.Remove(coords);
                Destroy(delChunk);
            }
            chunkTimer = loadDelay;
        }
        else
        {
            chunkTimer--;
        }
    }


    //Given a chunk-x and chunk-z position, build the actual GameObject for that chunk.
    //Return the chunk GameObject.
    GameObject buildChunkData(int x, int z)
    {
        Vector2Int chunkPos = new Vector2Int(x, z);

        x *= TerrainChunk.chunkWidth;
        z *= TerrainChunk.chunkWidth;
        
        GameObject theChunk = Instantiate(terrainChunk, new Vector3(x, 0, z), Quaternion.identity);
        TerrainChunk theBoi = theChunk.GetComponent<TerrainChunk>();
        theBoi.loadFaceVerts();
        
        //TERRAIN GEN HAPPENS HERE
        for (int xt = 0; xt < TerrainChunk.chunkWidth + 2; xt++)
        {
            for (int zt = 0; zt < TerrainChunk.chunkWidth + 2; zt++)
            {
                float pointNoise = pointNoiseVal(x, z, xt - 1, zt - 1);
                bool haventSet = true;
                for (int y = 0; y < TerrainChunk.chunkHeight; y++)
                {
                    if (surfaceNoiseVal(pointNoise, y) || y < 25)
                    {
                        theBoi.blockData[xt, y, zt] = 1;
                    }
                    else
                    {
                        theBoi.blockData[xt, y, zt] = 0;
                        if (haventSet && y > 0 && theBoi.blockData[xt, y - 1, zt] != 0)
                        {
                            theBoi.depthMap[xt, zt] = y;
                            haventSet = false;
                        }
                    }
                }
            }

        }
        theBoi.chunkwiseStructs(chunkPos);

        //Check to see if this position has received block edits.
        if (blockChanges.ContainsKey(chunkPos))
        {
            //If so, make the changes.
            //This system of tracking changes instead of every block in the chunk is FAR more efficient in both space and time complexity.
            //
            foreach(KeyValuePair<Vector3Int, int> entry in blockChanges[chunkPos])
            {
                Vector3Int changePos = entry.Key;
                theBoi.blockData[changePos.x, changePos.y, changePos.z] = entry.Value;
            }
        }
        else
        {
            blockChanges.Add(chunkPos, new Dictionary<Vector3Int, int>());
        }

        return theChunk;
    }

    //Determine whether the terrain at height y should exist given float noise value ptNoise.
    //Isolated this into function so the way noise is processed can be easily changed later.
    bool surfaceNoiseVal(float ptNoise, int y)
    {
        return (ptNoise + y) < TerrainChunk.chunkHeight * .5f;
    }

    //Get a float point noise value given a chunk location and an x/z location within that chunk. Uses Simplex Noise from SimplexNoiseGenerator
    //Determines y-height of basic terrain heightmap at that x/z location
    float pointNoiseVal(int chunkX, int chunkZ, int x, int z)
    {
        float freq = .1f;
        return noiseGenerator.coherentNoise((x + chunkX) * freq, 0, (z + chunkZ) * freq, 3) * 80;
        //Below is old Perlin noise method. keep for posterity.
        //return (Mathf.PerlinNoise((x + chunkX) * .05f, (z + chunkZ) * .05f)) * 9 + (Mathf.PerlinNoise((x + chunkX) * .1f, (z + chunkZ) * .1f));
    }

    //Returns the block index, or -1 if the chunk isn't currently loaded.
    public int getBlockAt(int x, int y, int z)
    {
        int chunkX = chunkNum(x);
        int chunkZ = chunkNum(z);

        Vector2Int cPos = new Vector2Int(chunkX, chunkZ);

        int xInChunk = inChunkCoord(x);
        int zInChunk = inChunkCoord(z);

        if (chunks.ContainsKey(cPos))
        {
            return chunks[cPos].GetComponent<TerrainChunk>().blockData[xInChunk, y, zInChunk];
        }
        else
        {
            return -1;
        }
    }

    //sets the block at (x,y,z) to block type blockInd.
    //optional param refreshInstant changes whether the chunk mesh is instantly reconstructed or not. Useful for structure gen/placement.
    //returns whether the operation was successful
    public bool setBlockAt(int x, int y, int z, int blockInd, bool refreshInstant = true)
    {
        int chunkX = chunkNum(x);
        int chunkZ = chunkNum(z);

        Vector2Int cPos = new Vector2Int(chunkX, chunkZ);

        int xInChunk = inChunkCoord(x);
        int zInChunk = inChunkCoord(z);

        List<TerrainChunk> toBeRefreshed = new List<TerrainChunk>();
        
        if (xInChunk == 1)
        {
            bool succ = trueSet(chunkX - 1, chunkZ, TerrainChunk.chunkWidth+1, y, zInChunk, blockInd);
            if (refreshInstant && succ) { toBeRefreshed.Add(chunks[new Vector2Int(chunkX - 1, chunkZ)].GetComponent<TerrainChunk>()); }
        }else if(xInChunk == TerrainChunk.chunkWidth)
        {
           bool succ = trueSet(chunkX + 1, chunkZ, 0, y, zInChunk, blockInd);
            if (refreshInstant && succ) { toBeRefreshed.Add(chunks[new Vector2Int(chunkX + 1, chunkZ)].GetComponent<TerrainChunk>()); }
        }
        if(zInChunk == 1)
        {
            bool succ = trueSet(chunkX, chunkZ - 1, xInChunk, y, TerrainChunk.chunkWidth+1, blockInd);
            if (refreshInstant && succ) { toBeRefreshed.Add(chunks[new Vector2Int(chunkX, chunkZ - 1)].GetComponent<TerrainChunk>()); }
        }
        else if(refreshInstant && zInChunk == TerrainChunk.chunkWidth)
        {
            bool succ = trueSet(chunkX, chunkZ + 1, xInChunk, y, 0, blockInd);
            if (refreshInstant && succ) { toBeRefreshed.Add(chunks[new Vector2Int(chunkX, chunkZ + 1)].GetComponent<TerrainChunk>()); }
        }
        bool returner = trueSet(cPos, xInChunk, y, zInChunk, blockInd);
        if (refreshInstant)
        {
            if (returner)
            {
                toBeRefreshed.Add(chunks[cPos].GetComponent<TerrainChunk>());
            }
            foreach (TerrainChunk item in toBeRefreshed)
            {
                item.refreshMesh();
            }
        }
        return returner;
    }

    //Sets a group of blocks at once, only refreshing relevant chunk meshes after all specified blocks have been set.
    //Returns whether the operation was successful.
    //Though it's currently only used for struct placement, I've separated this functionality off for other future applications, e.g. player abilities.
    public bool setGroupBlocks(List<Vector3Int> blocksToSet, List<int> setToInds)
    {
        List<TerrainChunk> toBeRefreshed = new List<TerrainChunk>();
        List<Vector2Int> refChunkInds = new List<Vector2Int>();
        int count = 0;
        foreach (Vector3Int pos in blocksToSet)
        {
            int chunkX = chunkNum(pos.x);
            int chunkZ = chunkNum(pos.z);
            Vector2Int chunkPos = new Vector2Int(chunkX, chunkZ);
            if (!chunks.ContainsKey(chunkPos))
            {
                return false;
            }
            //track chunks that need refreshing
            if (!refChunkInds.Contains(chunkPos))
            {
                refChunkInds.Add(chunkPos);
                toBeRefreshed.Add(chunks[chunkPos].GetComponent<TerrainChunk>());
            }
            setBlockAt(pos.x, pos.y, pos.z, setToInds[count], false);
            count++;
        }
        foreach (TerrainChunk item in toBeRefreshed)
        {
            item.refreshMesh();
        }
        return true;
    }

    //Given a Structure object, place it where specified by placeAt.
    //Return whether the operation was successful.
    public bool placeStruct(Structure struc, Vector3Int placeAt)
    {
        List<Vector3Int> truePoses = new List<Vector3Int>();
        foreach (Vector3Int relPos in struc.blockCoordsRel)
        {
            truePoses.Add(relPos + placeAt);
        }
        return setGroupBlocks(truePoses, struc.blockTypes);
    }

    //version of trueSet that takes two coords instead of Vector2Int for chunkPos. Just for convenience.
    bool trueSet(int chunkX, int chunkZ, int x, int y, int z, int blockInd, bool recChange = true)
    {
        Vector2Int thePos = new Vector2Int(chunkX, chunkZ);
        return trueSet(thePos, x, y, z, blockInd, recChange);
    }

    //trueSet actually makes the block change as specified at (x,y,z) to type blockInd in chunk at pos.
    //returns whether the operation was successful.
    bool trueSet(Vector2Int chunkPos, int x, int y, int z, int blockInd, bool recChange = true)
    {
        if (!chunks.ContainsKey(chunkPos))
        {
            return false;
        }
        if (y < TerrainChunk.chunkHeight && y >= 0)
        {
            chunks[chunkPos].GetComponent<TerrainChunk>().blockData[x, y, z] = blockInd;
            if (recChange)
            {
                if(blockChanges[chunkPos].ContainsKey(new Vector3Int(x, y, z)))
                {
                    blockChanges[chunkPos][new Vector3Int(x, y, z)] = blockInd;
                }
                else
                {
                    blockChanges[chunkPos].Add(new Vector3Int(x, y, z), blockInd);
                }
            }
        }
        else
        {
            return false;
        }
        return true;
    }

    //Convenience functions

    //Coordinate space conversions
    //Given Global coordinate (either x or z) returns the respective Chunk Pos coordinate.
    int chunkNum(int coord)
    {
        return coord < 0 ? (int)(coord / TerrainChunk.chunkWidth) - 1 : (int)(coord / TerrainChunk.chunkWidth);
    }

    //Given Global coordinate (either x or z) returns the respective Chunk Local coordinate.
    int inChunkCoord(int coord)
    {
        return (coord >= 0 ? coord % TerrainChunk.chunkWidth : (TerrainChunk.chunkWidth) + (coord % TerrainChunk.chunkWidth)) + 1;
    }
}
