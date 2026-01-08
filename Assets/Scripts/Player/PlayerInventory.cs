using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int maxSlots = 24;
    
    public ItemData goldItem;

    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();

    public event Action OnChanged;

    public IReadOnlyList<InventorySlot> Slots => slots;

    public int Gold => goldItem ? GetAmount(goldItem) : 0;

    public bool Add(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return false;
        
        if (item.stackable)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].item == item && slots[i].amount < item.maxStack)
                {
                    int space = item.maxStack - slots[i].amount;
                    int add = Mathf.Min(space, amount);
                    slots[i].amount += add;
                    amount -= add;

                    if (amount <= 0)
                    {
                        OnChanged?.Invoke();
                        return true;
                    }
                }
            }
        }
        
        while (amount > 0)
        {
            if (slots.Count >= maxSlots)
            {
                OnChanged?.Invoke();
                return false;
            }

            int add = item.stackable ? Mathf.Min(item.maxStack, amount) : 1;
            slots.Add(new InventorySlot(item, add));
            amount -= add;
        }

        OnChanged?.Invoke();
        return true;
    }

    public bool Remove(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return false;

        for (int i = slots.Count - 1; i >= 0 && amount > 0; i--)
        {
            if (slots[i].item != item) continue;

            int take = Mathf.Min(slots[i].amount, amount);
            slots[i].amount -= take;
            amount -= take;

            if (slots[i].amount <= 0)
                slots.RemoveAt(i);
        }

        OnChanged?.Invoke();
        return amount == 0;
    }

    public int GetAmount(ItemData item)
    {
        int total = 0;
        for (int i = 0; i < slots.Count; i++)
            if (slots[i].item == item)
                total += slots[i].amount;
        return total;
    }

    public bool AddGold(int amount)
    {
        if (!goldItem) { Debug.LogError("Gold item not assigned!"); return false; }
        return Add(goldItem, amount);
    }

    public bool SpendGold(int amount)
    {
        if (!goldItem) { Debug.LogError("Gold item not assigned!"); return false; }
        return Remove(goldItem, amount);
    }
}