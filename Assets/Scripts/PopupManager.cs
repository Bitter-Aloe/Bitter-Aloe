using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // Important for TextMeshPro

public class PopupManager : MonoBehaviour, IPointerClickHandler
{

    [SerializeField]
    public GameObject popupPrefab;
    private GameObject popupInstance;

    public int dataIndex = 0;

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
        if (popupPrefab)
        {
            /*popupText.text = message;
            popupPanel.SetActive(true);*/
            isPopupOpen = true;
            popupInstance = Instantiate(popupPrefab, new Vector3(transform.position.x, transform.position.y + 1.25f, transform.position.z), Quaternion.identity);
            TextMeshPro textMesh = popupInstance.GetComponentInChildren<TextMeshPro>();
            textMesh.text = message;
        }
    }

    private void ClosePopup()
    {
        if (popupPrefab)
        {
            /*popupPanel.SetActive(false);*/
            isPopupOpen = false;
            Destroy(popupInstance);
        }
    }

    private string GetCSVData()
    {
        var firstEntry = GlobalVariables.GetTestimonyEntry(dataIndex); // Index should start at 0 to get the first entry
        return $"Name: {firstEntry.name}\nDescription: {firstEntry.description}\nCoordinates: ({firstEntry.x}, {firstEntry.y})\nTopic: {firstEntry.topic}";
    }
}
