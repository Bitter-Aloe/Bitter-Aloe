using System.Collections.Generic;
using UnityEngine;

// Remap from one value range to another
public static class ExtensionMethods
{
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

}


public class FlowerPopulater : MonoBehaviour
{
    private GameObject[] flowers;
    private Vector2 objectPoolPosition = new Vector2(-50f, 50f);
    public int objectPoolSize = 1000;
    public GameObject flowerPrefab;
    public float spawnScale = 200.0f;
    public float glowIntensity = 8.0f;

    private bool shouldDropPlants = true;

    Vector2 maxInDataSet(List<DataEntry> dataEntries)
    {
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        for (int i = 0; i < dataEntries.Count; i++)
        {
            DataEntry entry = dataEntries[i];
            if(entry.x > max.x)
            {
                max.x = entry.x;
            }
            if(entry.y > max.y)
            {
                max.y = entry.y;
            }
        }

        return max;
    }

    Vector2 minInDataSet(List<DataEntry> dataEntries)
    {
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        for (int i = 0; i < dataEntries.Count; i++)
        {
            DataEntry entry = dataEntries[i];
            if (entry.x < min.x)
            {
                min.x = entry.x;
            }
            if (entry.y < min.y)
            {
                min.y = entry.y;
            }
        }

        return min;
    }


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Spawning Plants at t=" + Time.realtimeSinceStartupAsDouble);
        List<DataEntry> dataset = GlobalVariables.GetTestimonyData();
        Debug.Log(Mathf.Min(objectPoolSize, dataset.Count));
        flowers = new GameObject[Mathf.Min(objectPoolSize, dataset.Count)];

    

        Vector2 max = maxInDataSet(dataset);
        Vector2 min = minInDataSet(dataset);

        Debug.Log("Plant Count = " + objectPoolSize);

        for (int i = 0; i < flowers.Length; i++)
        {
            //Debug.Log(GlobalVariables.GetTestimonyEntry(i).x);
            DataEntry entry = GlobalVariables.GetTestimonyEntry(i);
            Vector3 pos = new Vector3(entry.x.Remap(min.x, max.x, -spawnScale, spawnScale), 0, entry.y.Remap(min.y, max.y, -spawnScale, spawnScale));
            flowers[i] = (GameObject)Instantiate(flowerPrefab, pos, Quaternion.AngleAxis(Random.value * 360, Vector3.up));
            flowers[i].GetComponent<PopupManager>().dataIndex = i;

            // FLOWER GLOW OPTION
            /*float hue = GlobalVariables.GetTestimonyEntry(i).topic;
            hue = hue.Remap(0, 266, 80, 150);
            hue = hue / 360.0f;
            Color eColor = Color.HSVToRGB(hue, 0.2f, 0.7f);

            flowers[i].GetComponentInChildren<MeshRenderer>().material.color = Color.HSVToRGB(hue, 0.2f, 1.0f);
            flowers[i].GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissiveColor", eColor * glowIntensity);
            flowers[i].GetComponentInChildren<MeshRenderer>().material.EnableKeyword("_EmissiveIntensity");*/

        }
        Debug.Log("Finished Spawning Plants at t=" + Time.realtimeSinceStartupAsDouble);

    }

    private void FixedUpdate()
    {
        if(shouldDropPlants)
        {
            Debug.Log("Dropping and deleting plants at t="+Time.realtimeSinceStartupAsDouble);
            Vector3[] hitPoints = new Vector3[flowers.Length];
            List<int> removeIndices = new List<int>();
            for (int i = 0; i < flowers.Length; i++)
            {
                GameObject flower = flowers[i];
                Rigidbody body;
                if ((body = flower.GetComponent<Rigidbody>()) != null)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(flower.transform.position, flower.transform.TransformDirection(Vector3.down), out hit, 1000.0f))
                    {
                        //Debug.Log(hit.point);
                        hitPoints[i] = hit.point;
                    }
                    else // destroy flower and add to list of flowers to delete from array
                    {
                        //Debug.Log("Could not find ground for plant at " +  flower.transform.position);
                        removeIndices.Add(i);
                        flower.SetActive(false);
                    }
                }
            }

            for(int i = 0; i < flowers.Length; i++)
            { // transform all plants together
                if (hitPoints[i] != null)
                {
                    flowers[i].transform.position = hitPoints[i];
                }
            }
            Debug.Log("Finished Dropping and deleting plants at t=" + Time.realtimeSinceStartupAsDouble);
            shouldDropPlants = false;
        }
      
    }


}
