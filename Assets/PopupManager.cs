using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // Important for TextMeshPro

public class PopupManager : MonoBehaviour, IPointerClickHandler
{
    public GameObject popupPanel;   // The UI panel that contains the popup text
    public TextMeshProUGUI popupText;  // Reference to your TextMeshPro Text component

    private bool isPopupOpen = false;

    public CSVParser csvParser; // This makes it assignable in the inspector

    void Awake()
    {
        csvParser = GetComponent<CSVParser>();  // Get the CSVParser component on the same GameObject
        ClosePopup();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        HandleClick();
    }

    public void HandleClick()
    {
        if (csvParser == null)
        {
            Debug.LogError("CSVParser is not assigned to PopupManager.");
            return;
        }

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
        // Check if csvParser or its dataList is null
        if (csvParser == null || csvParser.dataList == null)
        {
            return "CSVParser or dataList is not initialized.";
        }

        // Check if dataList has any entries
        if (csvParser.dataList.Count > 0)
        {
            var firstEntry = csvParser.dataList[0]; // Index should start at 0 to get the first entry
            return $"Name: {firstEntry.name}\nDescription: {firstEntry.description}\nCoordinates: ({firstEntry.x}, {firstEntry.y})\nTopic: {firstEntry.topic}";
        }
        return "No data available in CSV file.";
    }
}
