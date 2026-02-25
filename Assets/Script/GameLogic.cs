using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class GameLogic : MonoBehaviour
{
    public GameObject countPage;
    public int pageCount;

    void Start()
    {
        pageCount = 0;
        
        // Find countPage if not assigned
        if (countPage == null)
        {
            countPage = GameObject.Find("countPage");
        }
    }

    void Update()
    {
        if (countPage != null)
        {
            TextMeshProUGUI textComponent = countPage.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = pageCount + "/8";
            }
        }
    }
}
