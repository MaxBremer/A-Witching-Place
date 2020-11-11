using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempMeshBuilder : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        buildMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void buildMesh()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        Vector3[] verts = new Vector3[]
            {
                //front face
                new Vector3(0,0,0), //front left bottom
                new Vector3(0,1,0), //front left top
                new Vector3(1,0,0), //front right bottom
                new Vector3(1,1,0), //front right top
                new Vector3(1,0,1), //back left bottom
                new Vector3(1,1,1), // back left top
                new Vector3(0,0,1), //back right bottom
                new Vector3(0,1,1), //back right top
                new Vector3(0,1,0), //top left bottom
                new Vector3(0,1,1), //top left top
                new Vector3(1,1,0), //top right bottom
                new Vector3(1,1,1), //top right top
                new Vector3(0,0,1), //left left bottom
                new Vector3(0,1,1), //left left top
                new Vector3(0,0,0), //left right bottom
                new Vector3(0,1,0), //left right top
                new Vector3(1,0,0), //right left bottom
                new Vector3(1,1,0), //right left top
                new Vector3(1,0,1), //right right bottom
                new Vector3(1,1,1), //right right top
                new Vector3(0,0,1), //bottom left bottom
                new Vector3(0,0,0), //bottom left top
                new Vector3(1,0,1), //bottom right bottom
                new Vector3(1,0,0) //bottom right top
            };

        int[] tris = new int[] {0, 1, 2, 1, 3, 2, 4, 5, 6, 5, 7, 6, 8, 9, 10, 9, 11, 10, 12, 13, 14, 13, 15, 14, 16, 17, 18, 17, 19, 18, 20, 21, 22, 21, 23, 22 };// 0, 3, 2, 0, 1, 3, 5, 1, 0, 0, 4, 5, 1, 5, 3, 6, 3, 5 };//, 0, 7, 4, 0, 2, 7, 3, 2, 6,  };
        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = tris;
        //Vector3 vf = -Vector3.forward;
        //mesh.normals = new Vector3[] { vf, vf, vf, vf, Vector3.left, Vector3.left, Vector3.up };
        /*mesh.uv = new Vector2[]
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,0),
            new Vector2(1,1),
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1)
        };*/
        Debug.Log("recalc");
        mesh.Optimize();
        mesh.RecalculateNormals();
    }
}
