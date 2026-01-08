using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public ItemType type;

    [TextArea] public string description;

    public Sprite icon;

    public bool stackable = true;
    public int maxStack = 9999999;
    
    public int valueInGold = 1;
}