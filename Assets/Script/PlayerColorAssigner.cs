using UnityEngine;
using Photon.Pun;

public class PlayerColorAssigner : MonoBehaviourPun
{
    [Tooltip("ระยะเวลารอก่อนตั้งสี (วินาที)")]
    public float delay = 0.5f;

    void OnEnable()
    {
        Invoke("ApplyColor", delay);
    }

    private void ApplyColor()
    {
        if (photonView == null || photonView.Owner == null) return;

        // สุ่มสีจาก ActorNumber (seed เดียวกันทุก client → สีตรงกัน)
        System.Random rng = new System.Random(photonView.Owner.ActorNumber * 1234);
        Color playerColor = new Color((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());

        // ตั้งสีให้เฉพาะ MeshRenderer ตัวหลัก (capsule) ไม่รวม child (แว่น)
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null)
        {
            Material mat = mr.material;
            mat.color = playerColor;
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", playerColor);
        }

        Debug.Log(gameObject.name + " → color: " + playerColor + " (Actor: " + photonView.Owner.ActorNumber + ")");
    }

    void OnDisable()
    {
        CancelInvoke("ApplyColor");
    }
}
