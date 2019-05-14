using UnityEngine;
using Photon.Pun;

public class ProjectileScript : MonoBehaviour
{
    #region Private Serializable Fields
    [SerializeField]
    private GameObject hitEffect;
    [SerializeField]
    private GameObject dieEffect;
    #endregion

    #region Private Fields
    private GameObject firedFrom;
    private Rigidbody rb;
    private float currentLifetime = 30f;
    private float stallTime = 0f;
    #endregion

    public void SetupProjectile(MovementController owner)
    {
        firedFrom = owner.transform.root.gameObject;
    }

    #region MonoBehaviour Callbacks
    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        currentLifetime = 30f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (collision.transform.root.gameObject != firedFrom)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    int viewID = collision.transform.root.gameObject.GetPhotonView().ViewID;
                    GameEvents.KillPlayer(viewID);
                    SpawnEffect(collision.GetContact(0).point, 
                        collision.GetContact(0).normal, hitEffect);
                }
            }
        }
        else if (collision.gameObject.tag == "Destructible")
        {
            SpawnEffect(collision.GetContact(0).point,
                collision.GetContact(0).normal, hitEffect);
            Destroy(collision.gameObject);
        }
    }

    private void Update()
    {
        if (transform.position.y < -200f)
        {
            KillBullet();
        }
        else if (stallTime > 0.5f)
        {
            KillBullet();
        }
        else if (currentLifetime < 0f)
        {
            KillBullet();
        }
        currentLifetime -= Time.deltaTime;
        if (rb.velocity.magnitude < 5f)
        {
            stallTime += Time.deltaTime;
        }
        else
        {
            stallTime = 0;
        }
    }
    #endregion

    #region Private Methods
    private void KillBullet()
    {
        GameObject dieEffectInstance = Instantiate(dieEffect,
            transform.position,
            Quaternion.LookRotation(Vector3.up));
        dieEffectInstance.SetActive(true);
        Destroy(transform.root.gameObject);
    }

    private void SpawnEffect(Vector3 position, Vector3 direction, GameObject hitEffect)
    {
        GameObject hitEffectInstance = PhotonNetwork.Instantiate(hitEffect.name,
                position, Quaternion.LookRotation(direction));
        hitEffectInstance.SetActive(true);
    }
    #endregion
}
