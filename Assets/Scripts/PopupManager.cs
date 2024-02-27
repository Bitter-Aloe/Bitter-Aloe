using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // Important for TextMeshPro

public class PopupManager : MonoBehaviour, IPointerClickHandler
{
    public GameObject popupPanel;   // The UI panel that contains the popup text
    public TextMeshProUGUI popupText;  // Reference to your TextMeshPro Text component

    private bool isPopupOpen = false;

    void Awake()
    {
        ClosePopup();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        HandleClick();
    }

    public void HandleClick()
    {

        // Toggle the popup state
        if (isPopupOpen)
            ClosePopup();
        else
        {
            string message = GetCSVData();
            DisplayPopup(message);
        }
    }

    private void DisplayPopup(string message)
    {
        if (popupPanel)
        {
            popupText.text = message;
            popupPanel.SetActive(true);
            isPopupOpen = true;
        }
    }

    private void ClosePopup()
    {
        if (popupPanel)
        {
            popupPanel.SetActive(false);
            isPopupOpen = false;
        }
    }

    private string GetCSVData()
    {
        var firstEntry = GlobalVariables.GetTestimonyEntry(0); // Index should start at 0 to get the first entry
        return $"Name: {firstEntry.name}\nDescription: {firstEntry.description}\nCoordinates: ({firstEntry.x}, {firstEntry.y})\nTopic: {firstEntry.topic}";
    }
}
