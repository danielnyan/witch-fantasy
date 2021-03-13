using UnityEngine;
using Photon.Pun;

public class ParticleDestroyer : MonoBehaviour
{
    private ParticleSystem ps;

    // Start is called before the first frame update
    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (ps != null)
        {
            if (!ps.IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }
}
