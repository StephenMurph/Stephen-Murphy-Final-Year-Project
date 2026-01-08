using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TradeGood
{
    public ItemData item;
    public int buyPrice = 10;
    public int sellPrice = 7;
}

[CreateAssetMenu(menuName = "Trade/Town")]
public class TownData : ScriptableObject
{
    public string townName = "Town";
    public List<TradeGood> goods = new List<TradeGood>();
}