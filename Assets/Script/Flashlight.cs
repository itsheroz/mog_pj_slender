using UnityEngine;
using Photon.Pun;

public class Flashlight : MonoBehaviourPun
{
    public GameObject flashlight;

    public AudioClip turnOnSound;
    public AudioClip turnOffSound;

    private bool isOn = false;

    void Start()
    {
        flashlight.SetActive(false);
        isOn = false;
    }

    void Update()
    {
        // เฉพาะเจ้าของตัวละครเท่านั้นที่กดเปิด/ปิดได้
        if (!photonView.IsMine) return;

        if (Input.GetButtonDown("flashlight"))
        {
            isOn = !isOn;
            // ส่ง RPC ไปทุก client เพื่อให้เห็นไฟฉายเปิด/ปิดตรงกัน
            photonView.RPC("ToggleFlashlightRPC", RpcTarget.All, isOn);
        }
    }

    [PunRPC]
    private void ToggleFlashlightRPC(bool on)
    {
        isOn = on;
        flashlight.SetActive(on);

        if (SoundManager.Instance != null)
        {
            if (on)
                SoundManager.Instance.PlaySFX(turnOnSound);
            else
                SoundManager.Instance.PlaySFX(turnOffSound);
        }
    }
}
