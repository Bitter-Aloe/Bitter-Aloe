using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerPopulater : MonoBehaviour
{
    public string fileData = System.IO.File.ReadAllText("Assets/split_umap_subset.csv");

    private GameObject[] flowers;
    private Vector2 objectPoolPosition = new Vector2(-50f, 50f);
    public int obstaclePoolSize = 50;
    public GameObject flowerPrefab;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(fileData);
        flowers = new GameObject[obstaclePoolSize];


        // for (int i =0; i < obstaclePoolSize; i++)
        {
            // flowers[i] = (GameObject)Instantiate(flowerPrefab, objectPoolPosition, Quaternion.identity);
        }
    }

 
}
