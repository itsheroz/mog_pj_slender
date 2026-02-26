using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollectPage : MonoBehaviour
{
    //public GameObject collectText;
    public AudioClip collectSound;
    private GameObject page;
    private bool inReach;
    private GameObject gameLogic;

    void Start()
    {
        //collectText.SetActive(false);
        inReach = false;
        gameLogic = GameObject.FindGameObjectWithTag("GameLogic");
        page = this.gameObject;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Reach")
        {
            if (other != null)
            {
                //collectText = other.gameObject;
                //collectText.SetActive(true);
            } 
            inReach = true;
        }
    }
    GameObject collectText;
    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Reach")
        {
            if (other != null)
            {
                //collectText = null;
                //collectText.SetActive(false);
            }
            inReach = false;
        }
    }

    void Update()
    {
        if(inReach && Input.GetButtonDown("collect"))
        {
            if (gameLogic != null)
            {
                GameLogic gl = gameLogic.GetComponent<GameLogic>();
                if (gl != null)
                {
                    gl.pageCount++;
                }
            }
            SoundManager.Instance.PlaySFX(collectSound);
            page.SetActive(false);
            //if(collectText != null) 
           //     collectText.SetActive(false);
            inReach = false;
        }
    }
}
