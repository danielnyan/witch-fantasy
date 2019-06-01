using UnityEngine;
using Photon.Pun;

public class ProjectileScript : MonoBehaviourPun, IPunObservable
{
    #region Private Serializable Fields
    [SerializeField]
    private GameObject hitEffect;
    [SerializeField]
    private GameObject dieEffect;
    #endregion

    #region Private Fields
    private int firedFrom;
    private int layer;
    private Rigidbody rb;
    private Collider projCollider;
    private float currentLifetime = 30f;
    private float stallTime = 0f;
    #endregion

    #region IPunObservable Implementation
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(firedFrom);
            stream.SendNext(layer);
            stream.SendNext(currentLifetime);
            stream.SendNext(stallTime);
        }
        else
        {
            firedFrom = (int)stream.ReceiveNext();
            gameObject.layer = (int)stream.ReceiveNext();
            currentLifetime = (float)stream.ReceiveNext();
            stallTime = (float)stream.ReceiveNext();
        }
    }

    #endregion
    public void SetupProjectile(int firedFrom, int layer, float projectileSpeed,
        Vector3 firePosition, Vector3 fireDirection, Vector3 currVelocity)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            this.firedFrom = firedFrom;
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }
            gameObject.layer = layer;
            transform.position = firePosition;
            rb.velocity = projectileSpeed * fireDirection + currVelocity;
        }
    }

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        projCollider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        gameObject.layer = layer;
        currentLifetime = 30f;
        stallTime = 0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            // Failsafe in case the projectile somehow collides with an allied 
            // unit.
            if (layer == collision.transform.root.gameObject.layer)
            {
                Physics.IgnoreCollision(collision.collider, projCollider);
                return;
            }

            if (PhotonNetwork.IsMasterClient)
            {
                int viewID = collision.transform.root.gameObject.GetPhotonView().ViewID;
                if (viewID != firedFrom)
                {
                    GameEvents.KillPlayer(viewID);
                    SpawnEffect(collision.GetContact(0).point,
                        collision.GetContact(0).normal, hitEffect.name);
                }
            }
        }
        else if (collision.gameObject.tag == "Destructible")
        {
            SpawnEffect(collision.GetContact(0).point,
                collision.GetContact(0).normal, hitEffect.name);
            Destroy(collision.gameObject);
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
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
        GameObject dieEffectInstance = PhotonNetwork.Instantiate(dieEffect.name,
            transform.position,
            Quaternion.LookRotation(Vector3.up));
        dieEffectInstance.SetActive(true);
        PhotonNetwork.Destroy(transform.root.gameObject);
    }

    private void SpawnEffect(Vector3 position, Vector3 direction, string hitEffect)
    {
        GameObject hitEffectInstance = PhotonNetwork.Instantiate(hitEffect,
                position, Quaternion.LookRotation(direction));
        hitEffectInstance.SetActive(true);
    }
    #endregion
}
