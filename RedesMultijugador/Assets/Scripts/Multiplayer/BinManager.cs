using System.Collections;
using UnityEngine;

public class BinManager : MonoBehaviour
{
    public GameObject paperBinPrefab;
    public float spawnInterval = 10f;
    public Vector2 spawnAreaMin = new Vector2(-30, -30);
    public Vector2 spawnAreaMax = new Vector2(30, 30);

    private GameObject currentBin;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnPaperBin();
            yield return new WaitForSeconds(spawnInterval);
            Destroy(currentBin);
        }
    }

    void SpawnPaperBin()
    {
        if (currentBin != null)
            Destroy(currentBin);

        Vector3 randomPos = new Vector3(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            -1f,
            Random.Range(spawnAreaMin.y, spawnAreaMax.y)
        );

        currentBin = Instantiate(paperBinPrefab, randomPos, Quaternion.identity);
    }
}