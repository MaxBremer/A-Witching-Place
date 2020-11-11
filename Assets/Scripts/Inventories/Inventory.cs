using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    ItemStack[] items;
    const int defaultSize = 15;
    public Inventory(int size)
    {
        items = new ItemStack[size];
    }

    public Inventory()
    {
        items = new ItemStack[defaultSize];
    }
}
