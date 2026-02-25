using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollectPage : MonoBehaviour
{
    public GameObject collectText;
    public AudioSource collectSound;
    private GameObject page;
    private bool inReach;
    private GameObject gameLogic;

    void Start()
    {
        collectText.SetActive(false);
        inReach = false;
        gameLogic = GameObject.FindGameObjectWithTag("GameLogic");
        page = this.gameObject;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Reach")
        {
            if (collectText != null) collectText.SetActive(true);
            inReach = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Reach")
        {
            if (collectText != null) collectText.SetActive(false);
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
            collectSound.Play();
            page.SetActive(false);
            collectText.SetActive(false);
            inReach = false;
        }
    }
}
