using UnityEngine;
using Photon.Pun;

public class SlendermanAI : MonoBehaviourPun
{
    public Transform player;
    public float teleportDistance = 10f;
    public float teleportCooldown = 5f;
    public float returnDistance = 10f;
    [Range(0f, 1f)] public float ChaseProbability = 0.65f;
    public float rotationSpeed = 5f;
    public AudioClip teleportSound;

    public GameObject staticObject;
    public float staticObjectDistance = 5f;

    private Vector3 baseTeleportPosition;
    private float TeleportTimer;
    private bool returningToBase;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        baseTeleportPosition = transform.position;
        TeleportTimer = teleportCooldown;

        if (staticObject == null)
        {
            staticObject.SetActive(false);
            Debug.LogError("Static Object is not assigned in the inspector.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            // Only the Master Client controls the AI
            return;
        }

        // If we don't have a target or the target is inactive, find a new one.
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            FindNewPlayerTarget();
        }

        // If still no player, do nothing.
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

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if(distanceToPlayer <= staticObjectDistance)
        {
            if(staticObject != null && !staticObject.activeSelf)
            {
                staticObject.SetActive(true);
            }
        }
        else
        {
            if(staticObject != null && staticObject.activeSelf)
            {
                staticObject.SetActive(false);
            }
        }
    }

    private void FindNewPlayerTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            // Pick a random player to target
            int randomIndex = Random.Range(0, players.Length);
            player = players[randomIndex].transform;
            Debug.Log("Slenderman is now targeting: " + player.name);
        }
        else
        {
            player = null; // No players found
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
        randomPosition.y = transform.position.y; // Keep the same height
        transform.position = randomPosition;
        if(photonView!=null)
            photonView.RPC("PlayTeleportSoundRPC", RpcTarget.All, transform.position);
    }

    private void TeleportToBaseSpot()
    {
        transform.position = baseTeleportPosition;
        returningToBase = true;
            if(photonView!=null)
                photonView.RPC("PlayTeleportSoundRPC", RpcTarget.All, transform.position);
    }

    [PunRPC]
    private void PlayTeleportSoundRPC(Vector3 position)
    {
        SoundManager.Instance.PlaySFXAtPosition(teleportSound, position);
    }

    private void RotateTowardsPlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0f; // Keep the rotation on the horizontal plane

        if(directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
