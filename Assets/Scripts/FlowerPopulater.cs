using System.Collections;
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
    public int objectPoolSize = 50;
    public GameObject flowerPrefab;

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
        Debug.Log(GlobalVariables.GetTestimonyData().Count);
        flowers = new GameObject[objectPoolSize];

        List<DataEntry> dataset = GlobalVariables.GetTestimonyData();

        Vector2 max = maxInDataSet(dataset);
        Vector2 min = minInDataSet(dataset);

        for (int i =0; i < dataset.Count && i < objectPoolSize; i++)
        {
            //Debug.Log(GlobalVariables.GetTestimonyEntry(i).x);
            DataEntry entry = GlobalVariables.GetTestimonyEntry(i);
            Vector3 pos = new Vector3(entry.x.Remap(min.x, max.x, -50,50), 0, entry.y.Remap(min.y, max.y, -50, 50));
            flowers[i] = (GameObject)Instantiate(flowerPrefab, pos, Quaternion.identity);
            flowers[i].GetComponent<PopupManager>().dataIndex = i;

        }
    }

    private void FixedUpdate()
    {
       foreach(GameObject flower in flowers)
       {
            Rigidbody body;
            if ((body = flower.GetComponent<Rigidbody>()) != null) {
                if(Physics.Raycast(flower.transform.position, flower.transform.TransformDirection(Vector3.down), 0.5f)) {
                    body.useGravity = false;
                    body.isKinematic = true;
                    flower.transform.Translate(new Vector3(0, -0.1f, 0));
                }
            }
       }
    }


}
