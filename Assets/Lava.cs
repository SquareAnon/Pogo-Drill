using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : MonoBehaviour
{
    public bool rising;
    public float riseSpeed;
    public float startRiseTime;
    public float maxHeight;
    public float distanceThreshold = 5;
    public float riseDelay;
    [SerializeField]float speed;
   

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!rising)
        {
            if (Pogo._.transform.position.y - transform.position.y <= distanceThreshold)
            {
                SoundEffectManager._.CreateSound("Dun dun duun");
                rising = true;
                startRiseTime = Time.time;
                CameraControl._.Shake();
            }
       }

        if(rising)
        {
            if(Time.time >= startRiseTime + riseDelay)
            {
                speed = Mathf.MoveTowards(speed, riseSpeed, Time.deltaTime * 5);
                transform.position += Vector3.up * Time.deltaTime * riseSpeed;
            }
        }
    }

   private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position + Vector3.up * distanceThreshold, 1); 
    }
}
