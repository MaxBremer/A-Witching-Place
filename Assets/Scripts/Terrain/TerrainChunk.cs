using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//Class that stores a single Chunk worth of block and mesh data.
public class TerrainChunk : MonoBehaviour
{

    public const int chunkWidth = 16;
    public const int chunkHeight = 64;

    const string VERTS_PATH = "config\\verts.txt";

    //{front, left, right, top, bottom, back}
    private List<List<Vector3>> faceVerts = new List<List<Vector3>>();

    //Block data:
    //0 - air
    //1 - dirt
    public int[,,] blockData = new int[chunkWidth + 2, chunkHeight, chunkWidth + 2];
    bool[,,,] edgeData = new bool[chunkWidth, chunkHeight, chunkWidth, 6];
    public int[,] depthMap = new int[chunkWidth+2, chunkWidth+2];
    private int[] triOffsets = { 0, 1, 2, 3, 2, 1 };

    int seedOffset;

    //Changed to Awake, since seed initialization has to happen before Start.
    void Awake()
    {
        seedOffset = GameObject.Find("GameController").GetComponent<GameController>().seed;
    }

    //Generate all single-chunk structures.
    //Currently, only pillars, but framework exists for however many are desired.
    public void chunkwiseStructs(Vector2Int chunkPos)
    {
        //NOTE: The below means POSSIBLE repetition every 10000 chunks.
        //Set seed based on chunk pos so that structures generate the same way every time for the same chunk.
        Random.seed = (chunkPos.x * 10000) + chunkPos.y + seedOffset;
        genChunkPillars(.01f, 10);
    }

    //Generates pillars randomly on the surface level of this chunk.
    public void genChunkPillars(float pillarChance, int maxHeight)
    {
        for (int i = 0; i < chunkWidth; i++)
        {
            for (int j = 0; j < chunkWidth; j++)
            {
                if(Random.Range(0,1f) < pillarChance)
                {
                    int pillarHeight = Random.Range(1, maxHeight);
                    for (int k = 0; k < pillarHeight; k++)
                    {
                        blockData[i+1, Mathf.Min(depthMap[i+1, j+1] + k, chunkHeight-1), j+1] = 2;
                    }
                }
            }
        }
    }

    //Given that our block data has been fully generated, build this chunk mesh.
    public void buildMesh()
    {
        //Mesh requires vertices, uvs, and triangles. verts and tris for actual mesh, uvs for texturing.
        List<Vector3> vertList = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        int fC = 0;
        for (int y = 0; y < chunkHeight; y++)
        {
            for (int x = 1; x < chunkWidth + 1; x++)
            {
                for (int z = 1; z < chunkWidth + 1; z++)
                {
                    //if the blockData is 0, then the block is Air. Don't generate anything.
                    if (blockData[x, y, z] != 0)
                    {
                        //Given that this block is not empty, check if adjacent blocks are empty.
                        //A mesh face only needs to be drawn if this block is not air and an adjacent block is.
                        bool[] adjEmptys = airCount(x, y, z);

                        for (int i = 0; i < 6; i++)
                        {
                            //If we find one that is, add the relevant face.
                            if (adjEmptys[i])
                            {
                                for (int j = 0; j < 4; j++)
                                {
                                    //NOTE: faceVerts is cube face vertex data loaded from configs.
                                    //If that config is changed, things are gonna look WACKY.
                                    Vector3 actualAdd = faceVerts[i][j];
                                    actualAdd.x += x - 1;
                                    actualAdd.y += y;
                                    actualAdd.z += z - 1;
                                    vertList.Add(actualAdd);
                                }

                                foreach (int offset in triOffsets)
                                {
                                    triangles.Add(fC + offset);
                                }
                                
                                fC += 4;

                                //Then, add the UV data based on static info pulled from Block.
                                //NOTE: is gross. Fix this.
                                uvs.AddRange(Block.blockInfo[Block.bls[blockData[x,y,z]]].myUVs(i));
                            }
                        }
                    }
                }
            }
        }
        Mesh mesh = new Mesh();

        //Convert data to arrays, let Unity calculate the normals, then instantiate the mesh.
        mesh.vertices = vertList.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray(); 
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    //Self-explanatory. Delete the mesh, then re-build it.
    public void refreshMesh()
    {
        GetComponent<MeshFilter>().mesh.Clear();
        GetComponent<MeshCollider>().sharedMesh.Clear();
        buildMesh();
    }

    //Given a Chunk Local coordinate, return a list of whether adjacent blocks are transparent.
    //For now, that just means whether they're air or not.
    //{front, left, right, top, bottom, back}
    bool[] airCount(int x, int y, int z)
    {
        bool[] returner = new bool[6] { false, false, false, false, false, false };
        returner[0] = Block.blockTransparent((bl)blockData[x, y, z - 1]);
        returner[1] = Block.blockTransparent((bl)blockData[x - 1, y, z]);
        returner[2] = Block.blockTransparent((bl)blockData[x + 1, y, z]);
        returner[3] = y >= chunkHeight - 1 || Block.blockTransparent((bl)blockData[x, y + 1, z]);
        returner[4] = y > 0 && Block.blockTransparent((bl)blockData[x, y - 1, z]);
        returner[5] = Block.blockTransparent((bl)blockData[x, y, z + 1]);
        return returner;
    }

    //For pre-loading the face vertex information.
    public void loadFaceVerts()
    {
        string[] lines = File.ReadAllLines(VERTS_PATH);
        foreach (string item in lines)
        {
            List<Vector3> faceToAdd = new List<Vector3>();
            string[] verts = item.Split('|');
            foreach (string vert in verts)
            {
                string[] coords = vert.Split(',');
                Vector3 vertex = new Vector3();
                int i;
                int.TryParse(coords[0] + "", out i);
                vertex.x = i;
                int.TryParse(coords[1], out i);
                vertex.y = i;
                int.TryParse(coords[2], out i);
                vertex.z = i;
                faceToAdd.Add(vertex);
            }
            faceVerts.Add(faceToAdd);
        }
    }
}
