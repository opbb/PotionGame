using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrepStation : MonoBehaviour, IBrewingInteractable
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // IGUIScreen Implementation    

    public void activateGUI()
    {
        throw new System.NotImplementedException();
    }

    public void deactivateGUI()
    {
        throw new System.NotImplementedException();
    }

    public bool isGUIActive()
    {
        throw new System.NotImplementedException();
    }

    // IBrewingInteractable implementation

    public void PrintTestMessage()
    {
        Debug.Log("Test message from " + gameObject.name);
    }
}
