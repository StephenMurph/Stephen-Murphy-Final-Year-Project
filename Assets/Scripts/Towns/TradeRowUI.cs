using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeRowUI : MonoBehaviour
{
    public TMP_Text itemNameText;
    public TMP_Text ownedText;
    public TMP_Text buyText;
    public TMP_Text sellText;

    public Button buyButton;
    public Button sellButton;

    ItemData _item;

    public void Setup(
        ItemData item,
        int owned,
        int buyPrice,
        int sellPrice,
        System.Action<ItemData> onBuy,
        System.Action<ItemData> onSell)
    {
        _item = item;

        if (itemNameText) itemNameText.text = item.itemName;
        if (ownedText) ownedText.text = $"Owned: {owned}";
        if (buyText) buyText.text = $"Buy: {buyPrice}";
        if (sellText) sellText.text = $"Sell: {sellPrice}";

        if (buyButton)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => onBuy?.Invoke(_item));
        }

        if (sellButton)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(() => onSell?.Invoke(_item));
        }
    }
}