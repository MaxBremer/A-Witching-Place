using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack 
{

    public ItemType myType;
    public Metadata myData;
    public int count;

    public ItemStack(int itemType, Metadata data, int itemCount)
    {
        myType = (ItemType)itemType;
        myData = data;
        count = itemCount;
    }

    public ItemStack(ItemType itemType, Metadata data, int itemCount)
    {
        myType = itemType;
        myData = data;
        count = itemCount;
    }
}

public enum ItemType {bGround, bPillar, InfPatch, InfThread }

public class Metadata
{
    public static int numDataTypes = 2;
    public bool isActive;
    public bool[] typesActive;
    
    public Metadata(bool[] typesToStore) { typesActive = typesToStore; }

    public void activate() { isActive = true; }

}

public enum MetadataType {active, inventory }
