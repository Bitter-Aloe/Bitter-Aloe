using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

[System.Serializable]
public class DataEntry
{
    public int id;
    public string name;
    public string description;
    public float x;
    public float y;
    public int topic;
}


public class CSVParser : MonoBehaviour
{
    public TextAsset textAssetData;

    public List<DataEntry> dataList = new List<DataEntry>();

    void Awake()
    {
        Debug.Log("Loading CSV Data... t=" + Time.realtimeSinceStartupAsDouble);
        if (textAssetData != null)
        {
            // Normalize line endings to \n
            string[] lines = textAssetData.text.Replace("\r\n", "\n").Split('\n');

            for (int i = 1; i < lines.Length; i++) // Skip the header line
            {
                List<string> values = ParseCSVLine(lines[i]);

                if (values.Count >= 6) // Ensure there are at least 6 fields
                {
                    DataEntry data = new DataEntry();
                    if (int.TryParse(values[0], out int id) &&
                        float.TryParse(values[3], NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
                        float.TryParse(values[4], NumberStyles.Any, CultureInfo.InvariantCulture, out float y) &&
                        int.TryParse(values[5], out int topic))
                    {
                        data.id = id;
                        data.name = values[1];
                        data.description = values[2];
                        data.x = x;
                        data.y = y;
                        data.topic = topic;

                        dataList.Add(data);

                        //Debug.Log($"ID: {data.id}, Name: {data.name}, Description: {data.description}, X: {data.x}, Y: {data.y}, Topic: {data.topic}");
                    }
                    else
                    {
                        Debug.LogError($"Failed to parse data on line {i + 1}: {lines[i]}");
                    }
                }
                else if (values.Count >= 5) // Ensure there are at least 6 fields
                {
                    DataEntry data = new DataEntry();
                    if  (float.TryParse(values[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
                        float.TryParse(values[3], NumberStyles.Any, CultureInfo.InvariantCulture, out float y) &&
                        int.TryParse(values[4], out int topic))
                    {
                        data.id = i;
                        data.name = values[0];
                        data.description = values[1];
                        data.x = x;
                        data.y = y;
                        data.topic = topic;

                        dataList.Add(data);

                        //Debug.Log($"ID: {data.id}, Name: {data.name}, Description: {data.description}, X: {data.x}, Y: {data.y}, Topic: {data.topic}");
                    }
                    else
                    {
                        Debug.LogError($"Failed to parse data on line {i + 1}: {lines[i]}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Not enough values on line {i + 1}: {lines[i]}");
                }
            }
        }
        else
        {
            Debug.LogError("CSV file not assigned.");
        }
        Debug.Log("Finished Loading CSV Data... t=" + Time.realtimeSinceStartupAsDouble);
    }

    // Parse a CSV line with potential quoted fields
    private List<string> ParseCSVLine(string line)
    {
        List<string> fields = new List<string>();
        string field = "";
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(field);
                field = "";
            }
            else
            {
                field += c;
            }
        }
        fields.Add(field); // add last field

        // Trim each field and remove quotes
        for (int i = 0; i < fields.Count; i++)
        {
            fields[i] = fields[i].Trim().Trim('\"');
        }

        return fields;
    }
}
