using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeMenuUI : MonoBehaviour
{
    [Header("Root")]
    public GameObject root;

    [Header("Header")]
    public TMP_Text titleText;
    public TMP_Text goldText;

    [Header("Goods Rows")]
    public Transform rowsParent;
    public TradeRowUI rowPrefab;

    [Header("Destinations")]
    public Transform destinationsParent;
    public DestinationButtonUI destinationButtonPrefab;

    [Header("Debug Buttons")]
    public Button debugAdd50Gold;
    public Button debugAddSilk;
    public Button debugAddSpices;

    [Header("Debug Items")]
    public ItemData silkItem;
    public ItemData spicesItem;

    public bool IsOpen { get; private set; }

    PlayerInventory _inv;
    TownData _town;

    readonly Dictionary<ItemData, TradeGood> _goodsByItem = new();
    readonly List<TradeRowUI> _rows = new();
    readonly List<DestinationButtonUI> _destButtons = new();
    
    System.Action<string> _onTravelRequested;

    void Awake()
    {
        if (debugAdd50Gold) debugAdd50Gold.onClick.AddListener(() => { _inv?.AddGold(50); Refresh(); });
        if (debugAddSilk)   debugAddSilk.onClick.AddListener(() => { if (_inv && silkItem) _inv.Add(silkItem, 5); Refresh(); });
        if (debugAddSpices) debugAddSpices.onClick.AddListener(() => { if (_inv && spicesItem) _inv.Add(spicesItem, 5); Refresh(); });

        Close();
    }

    public void Open(
        TownData town,
        PlayerInventory inv,
        string currentTownName,
        List<string> destinationTownNames,
        System.Action<string> onTravelRequested)
    {
        Debug.Log("TradeMenuUI.Open called");
        _town = town;
        _inv = inv;
        _onTravelRequested = onTravelRequested;

        if (_inv != null) _inv.OnChanged += Refresh;

        IsOpen = true;
        if (root) root.SetActive(true);

        BuildGoodsLookup();
        BuildGoodsRows();
        BuildDestinationButtons(currentTownName, destinationTownNames);

        Refresh();
        
        Time.timeScale = 0f;
    }

    public void Close()
    {
        if (_inv != null) _inv.OnChanged -= Refresh;

        IsOpen = false;
        if (root) root.SetActive(false);

        _town = null;
        _inv = null;
        _onTravelRequested = null;

        ClearGoodsRows();
        ClearDestinationButtons();
        _goodsByItem.Clear();

        Time.timeScale = 1f;
    }

    void BuildGoodsLookup()
    {
        _goodsByItem.Clear();
        if (_town == null) return;

        foreach (var g in _town.goods)
            if (g?.item != null)
                _goodsByItem[g.item] = g;
    }

    void BuildGoodsRows()
    {
        ClearGoodsRows();
        if (_town == null || _inv == null || rowPrefab == null || rowsParent == null) return;

        foreach (var g in _town.goods)
        {
            if (g?.item == null) continue;

            var row = Instantiate(rowPrefab, rowsParent);
            _rows.Add(row);

            row.Setup(
                g.item,
                _inv.GetAmount(g.item),
                g.buyPrice,
                g.sellPrice,
                Buy,
                Sell
            );
        }
    }

    void ClearGoodsRows()
    {
        foreach (var r in _rows)
            if (r != null) Destroy(r.gameObject);
        _rows.Clear();
    }

    void BuildDestinationButtons(string currentTownName, List<string> destinations)
    {
        Debug.Log("BuildDestinationButtons called. destinations count = " + (destinations == null ? -1 : destinations.Count));
        ClearDestinationButtons();

        if (destinationsParent == null || destinationButtonPrefab == null) return;
        
        if (titleText) titleText.text = currentTownName;

        if (destinations == null) return;

        foreach (var dest in destinations)
        {
            Debug.Log("Spawned destination button for: " + dest);
            var btn = Instantiate(destinationButtonPrefab, destinationsParent);
            _destButtons.Add(btn);

            btn.Setup($"{dest}", () =>
            {
                var cb = _onTravelRequested;
                Close();
                cb?.Invoke(dest);
            });
        }
    }

    void ClearDestinationButtons()
    {
        foreach (var b in _destButtons)
            if (b != null) Destroy(b.gameObject);
        _destButtons.Clear();
    }

    void Refresh()
    {
        if (!IsOpen || _inv == null) return;

        if (goldText) goldText.text = $"Gold: {_inv.Gold}";
        
        if (_town != null) BuildGoodsRows();
    }

    void Buy(ItemData item)
    {
        if (_inv == null || item == null) return;
        if (!_goodsByItem.TryGetValue(item, out var g)) return;
        if (_inv.Gold < g.buyPrice) return;

        if (_inv.Add(item, 1))
            _inv.SpendGold(g.buyPrice);

        Refresh();
    }

    void Sell(ItemData item)
    {
        if (_inv == null || item == null) return;
        if (!_goodsByItem.TryGetValue(item, out var g)) return;
        if (_inv.GetAmount(item) <= 0) return;

        if (_inv.Remove(item, 1))
            _inv.AddGold(g.sellPrice);

        Refresh();
    }
}
