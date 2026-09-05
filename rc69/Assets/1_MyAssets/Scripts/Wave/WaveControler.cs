using Raccoon.Controller;
using System.Collections.Generic;
using UnityEngine;

public class WaveControler : MonoBehaviour
{

    public Transform startPos;
    public Transform endPos;

    float currentTimeSpawn;
    public float timeSpawn = 5f;

    float totalWeight = 0f;
    [SerializeField] List<DataWaveSpawn> datas;
    void Start()
    {
        indexWay = 0;
        // Tính tổng weight
        foreach (var item in datas)
        {
            totalWeight += item.percent;
        }
    }

    // Update is called once per frame
    void Update()
    {

        currentTimeSpawn += Time.deltaTime;

        if (currentTimeSpawn >= timeSpawn)
        {
            var prefab = GetWaveSpawn(); 
            if (prefab == null) return;
            SpawnWave(prefab);
            currentTimeSpawn = 0f;

        }
    }

    void SpawnWave(GameObject prefab)
    {
        var waveGO = GamePlayControl.instance.SpawnItem(prefab);
        waveGO.SetActive(true);
        waveGO.GetComponent<Wave>()?.Init(startPos, endPos);
    }

    int indexWay = 0;
    int numberSpawn = 8;
    int currentSpawn = 0;
    GameObject GetWaveSpawn()
    {
        return GetByOrder();
    }

    GameObject GetRandom()
    {
        float randomValue = UnityEngine.Random.Range(0, totalWeight);

        float current = 0f;

        foreach (var item in datas)
        {
            current += item.percent;

            if (randomValue <= current)
            {
                return item.prefab;
            }
        }
        return null;
    }

    GameObject GetByOrder()
    {
        currentSpawn++;
        if(currentSpawn > numberSpawn)
        {
            currentSpawn = 1; 
            indexWay++;
        }

        indexWay = indexWay % datas.Count;
        return datas[indexWay].prefab;
    }

}

[System.Serializable]
struct DataWaveSpawn
{
    public int percent;
    public GameObject prefab;
}
