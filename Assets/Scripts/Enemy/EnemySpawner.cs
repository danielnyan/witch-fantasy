using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject stationaryEnemy;
    [SerializeField]
    private GameObject movingEnemy;
    public int numberOfEnemies = 50;

    // Start is called before the first frame update
    private void Start()
    {
        for (int i = 0; i < 50; i++)
        {
            SpawnNewEnemy(1);
        }
    }

    public void SpawnNewEnemy(int type = 0)
    {
        if (type == 0)
        {
            type = Random.Range(0f, 1f) > 0.8f ? 2 : 1;
        }
        GameObject spawned;
        if (type == 1)
        {
            spawned = stationaryEnemy;
        }
        else
        {
            spawned = movingEnemy;
        }
        GameObject newObject = Instantiate(spawned, Random.insideUnitSphere * 90f, Quaternion.identity, transform);
        newObject.GetComponent<Enemy>().enemySpawner = this;
    }

    private void Update()
    {
        if (numberOfEnemies < 50)
        {
            SpawnNewEnemy();
            numberOfEnemies++;
        }
    }
}
