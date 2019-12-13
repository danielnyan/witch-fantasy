using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemySpawner enemySpawner;
    private Rigidbody rb;
    [SerializeField]
    private bool isStationary;
    [SerializeField]
    private int score;

    private Quaternion targetRotation;
    private float velocity;

    private float changeRotationTime = 10f;

    private void OnDestroy()
    {
        enemySpawner.numberOfEnemies--;
        // This is shitty design, but meh. It's just a demo
        if (SettingsManager.instance != null)
        {
            UIManager manager = SettingsManager.instance.transform.root.GetComponentInChildren<UIManager>();
            if (manager != null)
            {
                manager.IncrementScore(score);
            }
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (isStationary)
        {
            rb.angularVelocity = Random.insideUnitSphere * Random.Range(2, 3);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(Random.insideUnitSphere, Vector3.up);
            targetRotation = transform.rotation;
            velocity = Random.Range(3, 7);
            rb.velocity = transform.forward * Random.Range(3, 7);
        }
    }

    private void FixedUpdate()
    {
        if (!isStationary)
        {
            Quaternion target = Quaternion.RotateTowards(transform.rotation, targetRotation, 400f * Time.fixedDeltaTime);
            rb.MoveRotation(target);
            rb.velocity = Vector3.Slerp(rb.velocity, transform.forward * velocity, 0.05f);
            changeRotationTime -= Time.fixedDeltaTime;
            if (changeRotationTime < 0f)
            {
                changeRotationTime = 10f;
                targetRotation = Quaternion.LookRotation(Random.insideUnitSphere);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Kill Zone"))
        {
            transform.position = enemySpawner.transform.position + Random.insideUnitSphere * 90f;
        }
    }
}
