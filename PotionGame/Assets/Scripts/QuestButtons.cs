using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestButtons : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // accept quest button
    public void ButtonPressAccept()
    {
        // QuestManager.QuestAccept();
        Debug.Log("quest accepted!");
    }

    // reject quest button
    public void ButtonPressReject()
    {
        Debug.Log("quest rejected!");

    }
}
