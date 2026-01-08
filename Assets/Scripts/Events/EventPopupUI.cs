using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventPopupUI : MonoBehaviour
{
    [Header("Root")]
    public GameObject root;

    [Header("Text")]
    public TMP_Text titleText;
    public TMP_Text bodyText;

    [Header("Buttons")]
    public Button okButton;
    public Button fightButton;
    public Button payButton;

    bool _choiceMade;
    string _choice; 

    void Awake()
    {
        HideImmediate();
    }

    void HideImmediate()
    {
        if (root) root.SetActive(false);
        _choiceMade = false;
        _choice = null;
    }

    public IEnumerator ShowWeather(string title, string message)
    {
        OpenBase(title, message);
        
        SetActive(ok: true, fight: false, pay: false);

        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(() => { _choice = "ok"; _choiceMade = true; });

        yield return WaitForChoice();
        CloseBase();
    }
    
    public IEnumerator ShowOk(string title, string message)
    {
        OpenBase(title, message);

        SetActive(ok: true, fight: false, pay: false);

        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(() => { _choice = "ok"; _choiceMade = true; });

        yield return WaitForChoice();
        CloseBase();
    }

    public IEnumerator ShowBandits(string title, string message, System.Action onFight, System.Action onPay)
    {
        OpenBase(title, message);
        
        SetActive(ok: false, fight: true, pay: true);

        fightButton.onClick.RemoveAllListeners();
        payButton.onClick.RemoveAllListeners();

        fightButton.onClick.AddListener(() => { _choice = "fight"; _choiceMade = true; });
        payButton.onClick.AddListener(() => { _choice = "pay"; _choiceMade = true; });

        yield return WaitForChoice();
        
        if (_choice == "fight") onFight?.Invoke();
        else if (_choice == "pay") onPay?.Invoke();

        CloseBase();
    }

    void OpenBase(string title, string message)
    {
        _choiceMade = false;
        _choice = null;

        if (titleText) titleText.text = title;
        if (bodyText) bodyText.text = message;

        if (root) root.SetActive(true);
        
        Time.timeScale = 0f;
    }

    void CloseBase()
    {
        if (root) root.SetActive(false);
        Time.timeScale = 1f;
    }

    void SetActive(bool ok, bool fight, bool pay)
    {
        if (okButton) okButton.gameObject.SetActive(ok);
        if (fightButton) fightButton.gameObject.SetActive(fight);
        if (payButton) payButton.gameObject.SetActive(pay);
    }

    IEnumerator WaitForChoice()
    {
        yield return new WaitUntil(() => _choiceMade);
    }
}
