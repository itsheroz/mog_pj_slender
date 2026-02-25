using UnityEngine;
using UnityEngine.UIElements;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public Transform player;
    public float teleportDistance = 10f;
    public float teleportCooldown = 5f;
    public float returnDistance = 10f;
    [Range(0f, 1f)] public float ChaseProbability = 0.65f;
    public float rotationSpeed = 5f;
    public AudioClip teleportSound;
    private AudioSource audioSource;

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

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.clip = teleportSound;

        if (staticObject == null)
        {
            staticObject.SetActive(false);
            Debug.LogError("Static Object is not assigned in the inspector.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(player == null)
        {
            Debug.LogError("Player Transform is not assigned in the inspector.");
            return;
        }
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

        audioSource.Play();
    }

    private void TeleportToBaseSpot()
    {
        transform.position = baseTeleportPosition;
        returningToBase = true;
        audioSource.Play();
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
