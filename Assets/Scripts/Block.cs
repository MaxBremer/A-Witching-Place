using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block 
{
    const float texIncr = .25f;
    const float texMargin = 0.001f;
    public List<Vector2Int> sideTexCoords = new List<Vector2Int>();
    public Block(Vector2Int texture)
    {
        for (int i = 0; i < 6; i++)
        {
            sideTexCoords.Add(texture);
        }
    }
    
    public Block(Vector2Int topTex, Vector2Int sideTex, Vector2Int bottomTex)
    {
        for (int i = 0; i < 3; i++)
        {
            sideTexCoords.Add(sideTex);
        }
        sideTexCoords.Add(topTex);
        sideTexCoords.Add(bottomTex);
        sideTexCoords.Add(sideTex);
    }

    public Block(Vector2Int[] sideTexs)
    {
        if (sideTexs.Length != 6)
        {
            throw new System.Exception("incorrect number of sides passed in array");
        }
        foreach (Vector2Int side in sideTexs)
        {
            sideTexCoords.Add(side);
        }
    }

    public static List<bl> bls = new List<bl>() { bl.Air, bl.BaseRock, bl.Pillar};

    public List<Vector2> trueUVs(Vector2Int virtualCoords)
    {
        
        List<Vector2> returner = new List<Vector2>();
        Vector2 baseCoord = new Vector2((virtualCoords.x * texIncr), (virtualCoords.y * texIncr));
        returner.Add(baseCoord);
        returner.Add(baseCoord + new Vector2(texMargin, texIncr- texMargin));
        returner.Add(baseCoord + new Vector2(texIncr - texMargin, texMargin));
        returner.Add(baseCoord + new Vector2(texIncr - texMargin, texIncr - texMargin));
        return returner;
    }

    public List<Vector2> myUVs(int sideChoice)
    {
        return trueUVs(sideTexCoords[sideChoice]);
    }

    public static bool blockTransparent(bl passed)
    {
        if (passed==bl.Air)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static Dictionary<bl, Block> blockInfo = new Dictionary<bl, Block>() { 
        { bl.BaseRock, new Block(new Vector2Int(0,0)) },
        {bl.Pillar, new Block(new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(1,0)) }
    };
}
public enum bl { Air, BaseRock, Pillar}

