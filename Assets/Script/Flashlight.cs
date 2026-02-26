using UnityEngine;


public class Flashlight : MonoBehaviour
{
    public GameObject flashlight;

    public AudioClip turnOnSound;
    public AudioClip turnOffSound;

    private bool on;
    private bool off;

    void Start()
    {
        off = true;
        flashlight.SetActive(false);
    }

    void Update()
    {
        if(off && Input.GetButtonDown("flashlight"))
        {
            flashlight.SetActive(true);
            SoundManager.Instance.PlaySFX(turnOnSound);
            off = false;
            on = true;
        }
        else if(on && Input.GetButtonDown("flashlight"))
        {
            flashlight.SetActive(false);
            SoundManager.Instance.PlaySFX(turnOffSound);
            off = true;
            on = false;
        }
    }
}
