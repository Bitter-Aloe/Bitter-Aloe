using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVariables : MonoBehaviour
{

    private static List<DataEntry> testimonyEntries;
    // Start is called before the first frame update
    void Awake()
    {
        testimonyEntries = GetComponent<CSVParser>()?.dataList;
    }

    public static List<DataEntry> GetTestimonyData()
    {
        // Check if csvParser or its dataList is null
        if (testimonyEntries == null)
        {
            Debug.Log("No entries");
            return new List<DataEntry>();
        }
        else
            return testimonyEntries;

    }

    public static DataEntry GetTestimonyEntry(int index)
    {
        // Check if csvParser or its dataList is null
        List<DataEntry> Entries = GetTestimonyData();
        if (Entries.Count > index && index >= 0)
        {
            return Entries[index];
        }
        Debug.Log("Invalid index");
        return new DataEntry();
    }
}
