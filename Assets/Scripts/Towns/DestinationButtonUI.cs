using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DestinationButtonUI : MonoBehaviour
{
    public Button button;
    public TMP_Text label;

    public void Setup(string text, System.Action onClick)
    {
        Debug.Log("DestinationButtonUI.Setup: " + text);

        if (label) label.text = text;

        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                Debug.Log("DestinationButton clicked: " + text);
                onClick?.Invoke();
            });
        }
        else
        {
            Debug.LogError("DestinationButtonUI.Setup: button field is NULL");
        }
    }
}