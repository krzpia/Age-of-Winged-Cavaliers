using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAI
{
    public Unit unit;
    public int unitItemf;

    public void AddUnitToPlayItem(Unit unitToAdd)
    {
        unit = unitToAdd;
    }

    public void ChangeUnitf(int unitItemfToChange)
    {
        unitItemf += unitItemfToChange;
    }
}
