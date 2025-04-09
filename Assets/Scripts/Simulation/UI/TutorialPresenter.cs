using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPresenter : MonoBehaviour
{
    public GameObject[] popUps;
    private int popUpIndex;

    private void Start()
    {
        popUpIndex = 0;
        popUps[1].SetActive(false);

        Cursor.visible = false;
    }

    void Update()
    {
        if (popUpIndex == 0)
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.X))
            {
                popUpIndex = 1;

                popUps[0].SetActive(false);
                popUps[1].SetActive(!popUps[1].activeSelf);
            }
        }
        else if (popUpIndex == 1)
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.X))
            {
                popUps[1].SetActive(!popUps[1].activeSelf);
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Cursor.visible = !Cursor.visible;
        }

    }
}
