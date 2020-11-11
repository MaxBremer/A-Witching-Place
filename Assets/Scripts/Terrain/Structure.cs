using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Very simple class: Stores its block coordinates and types, as well as its own "radius" (the max distance from center to any given x/z coordinate).
public class Structure
{
    public int widthRadius;

    //BLOCK COORDINATES STORED RELATIVE TO "ORIGIN" OF STRUCT, i.e. its center.
    //Center is determined arbitrarily by definer of Structure.
    public List<Vector3Int> blockCoordsRel;
    public List<int> blockTypes;
    int curRad;

    public Structure() {
        blockCoordsRel = new List<Vector3Int>();
        blockTypes = new List<int>();
        curRad = 0;
    }

    public void addBlock(Vector3Int relPos, int blockType)
    {
        blockCoordsRel.Add(relPos);
        curRad = Mathf.Max(curRad, relPos.x, relPos.z);
        blockTypes.Add(blockType);
    }

    //maybe todo?
    public void recalcCenter() { }

    //maybe todo?
    public void recalcWidthRadius() { }

    public int getRadius()
    {
        return curRad;
    }
}
