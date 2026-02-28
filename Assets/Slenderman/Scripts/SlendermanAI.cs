using UnityEngine;
using Photon.Pun;

public class SlendermanAI : MonoBehaviourPun, IPunObservable
{
    public Transform player;
    public float teleportDistance = 10f;
    public float teleportCooldown = 5f;
    public float returnDistance = 10f;
    [Range(0f, 1f)] public float ChaseProbability = 0.65f;
    public float rotationSpeed = 5f;
    public AudioClip teleportSound;

    // ลบ staticObject ออกแล้ว — ย้ายไปฝั่ง PlayerController แทน

    private Vector3 baseTeleportPosition;
    private float TeleportTimer;
    private bool returningToBase;

    // Network sync variables
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    void Start()
    {
        baseTeleportPosition = transform.position;
        TeleportTimer = teleportCooldown;
        networkPosition = transform.position;
        networkRotation = transform.rotation;
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (player == null || !player.gameObject.activeInHierarchy)
            {
                FindNewPlayerTarget();
            }

            if (player == null) return;

            TeleportTimer -= Time.deltaTime;

            if (TeleportTimer <= 0f)
            {
                if (returningToBase)
                {
                    TeleportToBaseSpot();
                    returningToBase = false;
                }
                else
                {
                    DecideTeleportAction();
                    TeleportTimer = teleportCooldown;
                }
            }
            RotateTowardsPlayer();
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10f);
        }
    }

    private void FindNewPlayerTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            int randomIndex = Random.Range(0, players.Length);
            player = players[randomIndex].transform;
            Debug.Log("Slenderman is now targeting: " + player.name);
        }
        else
        {
            player = null;
            Debug.Log("Slenderman can't find any players to target.");
        }
    }

    private void DecideTeleportAction()
    {
        float randomValue = Random.value;
        if (randomValue <= ChaseProbability)
        {
            TeleportNearPlayer();
        }
        else
        {
            TeleportToBaseSpot();
            returningToBase = true;
        }
    }

    private void TeleportNearPlayer()
    {
        Vector3 randomPosition = player.position + Random.onUnitSphere * teleportDistance;
        randomPosition.y = transform.position.y;
        transform.position = randomPosition;
        if (photonView != null)
            photonView.RPC("PlayTeleportSoundRPC", RpcTarget.All, transform.position);
    }

    private void TeleportToBaseSpot()
    {
        transform.position = baseTeleportPosition;
        returningToBase = true;
        if (photonView != null)
            photonView.RPC("PlayTeleportSoundRPC", RpcTarget.All, transform.position);
    }

    [PunRPC]
    private void PlayTeleportSoundRPC(Vector3 position)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFXAtPosition(teleportSound, position);
    }

    private void RotateTowardsPlayer()
    {
        if (player == null) return;

        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0f;

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
