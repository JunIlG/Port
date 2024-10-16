using System;
using UnityEngine;

public class ItemSO : ScriptableObject
{
    public string id = Guid.NewGuid().ToString();
    public string itemName;
    public string itemDescription;
    [HideInInspector] public bool[] itemGrid;
    public Sprite icon;

    private void OnEnable() 
    {
        if (itemGrid == null)
        {
            itemGrid = new bool[16];
        }
    }

    public bool this[int y, int x]
    {
        get => itemGrid[y * 4 + x];
        set => itemGrid[y * 4 + x] = value;
    }

    public Dimension GetItemDimension()
    {
        int mxH = 0;
        for (int x = 0; x < 4; ++x)
        {
            int stH = -1;
            int lstH = -1;
            for (int y = 0; y < 4; ++y)
            {
                if (this[y, x] == true)
                {
                    if (stH == -1)
                    {
                        stH = y;
                    }
                    
                    lstH = y;
                }
            }
            mxH = Mathf.Max(mxH, lstH - stH);
        }

        int mxW = 0;
        for (int y = 0; y < 4; ++y)
        {
            int stW = -1;
            int lstW = -1;
            for (int x = 0; x < 4; ++x)
            {
                if (this[y, x] == true)
                {
                    if (stW == -1)
                    {
                        stW = x;
                    }
                    lstW = x;
                }
            }
            mxW = Mathf.Max(mxW, lstW - stW);
        }

        return new Dimension() { width = mxW + 1, height = mxH + 1 };
    }
}
