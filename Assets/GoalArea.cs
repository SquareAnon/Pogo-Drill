using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalArea : MonoBehaviour
{
    bool gameHasStarted;
    public GameObject victorFX;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.GetComponent<Pogo>())
        gameHasStarted = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Pogo>() && gameHasStarted && other.GetComponent<Pogo>().maxDepth < -5f)
        {
            SoundEffectManager._.CreateSound("Cheer");
            SoundEffectManager._.CreateSound("Popper");
            other.GetComponent<Pogo>().returned = true;
            victorFX.SetActive(true);
            GameUI._.ShowGameOverScreen();
        }
           

    }


}
