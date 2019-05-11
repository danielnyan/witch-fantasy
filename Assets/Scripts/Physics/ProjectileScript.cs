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
                    PlayerManager.instance.SendKillRequest(collision.transform.root.gameObject.GetPhotonView().ViewID);
                    GameObject hitEffectInstance = PhotonNetwork.Instantiate(hitEffect.name,
                        collision.GetContact(0).point,
                        Quaternion.LookRotation(collision.GetContact(0).normal));
                    hitEffectInstance.SetActive(true);
                }
            }
        }
        else if (collision.gameObject.tag == "Destructible")
        {
            GameObject hitEffectInstance = PhotonNetwork.Instantiate(hitEffect.name,
                    collision.GetContact(0).point,
                    Quaternion.LookRotation(collision.GetContact(0).normal));
            hitEffectInstance.SetActive(true);
            Destroy(collision.gameObject);
        }
    }

    private void Update()
    {
        if (transform.position.y < -200f)
        {
            KillBullet();
        } else if (stallTime > 0.5f)
        {
            KillBullet();
        } else if (currentLifetime < 0f)
        {
            KillBullet();
        }
        currentLifetime -= Time.deltaTime;
        if (rb.velocity.magnitude < 5f)
        {
            stallTime += Time.deltaTime;
        } else
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
    #endregion
}
