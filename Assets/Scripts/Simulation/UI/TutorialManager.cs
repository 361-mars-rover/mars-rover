using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject[] popUps;
    private int popUpIndex;

    private void Start()
    {
        popUpIndex = 0;
        popUps[1].SetActive(false);
        popUps[2].SetActive(false);

        Cursor.visible = false;
    }

    void Update()
    {
        
        //popUnIndex = 0 will only happen at the start of the simulation
        //This is in case we decide to have a "sequential" tutorial
        /*
        for (int i = 0; i < popUps.Length; i++)
        {
            Debug.Log("PopUpIndex: " + popUpIndex);
            if (i == popUpIndex)
            {
                Debug.Log("Setting Active: " + popUpIndex);
                popUps[i].SetActive(true);
            }
            else
            {
                Debug.Log("Setting InActive: " + popUpIndex);
                popUps[i].SetActive(false);
            }
        }*/
        

        if (popUpIndex == 0)
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C))
            {
                popUpIndex = 1;

                popUps[0].SetActive(false);
                popUps[1].SetActive(!popUps[1].activeSelf);
            }
        }
        else if (popUpIndex == 1)
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C))
            {
                popUps[1].SetActive(!popUps[1].activeSelf);
            }
            else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.V))
            {
                popUps[popUpIndex + 1].SetActive(!popUps[popUpIndex + 1].activeSelf);
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Cursor.visible = !Cursor.visible;
        }

    }
}
