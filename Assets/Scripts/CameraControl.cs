using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraControl : MonoBehaviour
{
    public static CameraControl _;
    public float defaulShakeForce = .5f;
    public Vector3 cameraOffset;
    Vector3 camOffset;
    public float offsetTransitionSpeed;
    public float camOffsetMultiplier = 5;
    public int cameraTargetXClamp = 5;
    public int cameraTargetYClamp = 10;
    private void Awake()
    {
        _ = this;

    }

    public CinemachineImpulseSource impulseSource;
    public CinemachineVirtualCamera vc;
    CinemachineFramingTransposer tp;
    public Transform target;


    private void Start()
    {
        tp = vc.GetCinemachineComponent<CinemachineFramingTransposer>();
        vc.Follow = target;
    }
    public void Shake(float force)
    {
       
        Vector3 random = Random.onUnitSphere;
        random.z = 0;
        impulseSource.GenerateImpulse(random *  force);
    }

    public void Shake()
    {
        Shake(defaulShakeForce);
    }

    private void Update()
    {
        Vector3 p = Pogo._.transform.position;
        p.x = Mathf.Clamp(p.x, Cave._.startX + cameraTargetXClamp , Cave._.startX + Cave._.sizeX - cameraTargetXClamp - 1 );
        if (p.y >= Cave._.startY + cameraTargetYClamp) p.y = Cave._.startY + cameraTargetYClamp;
        target.transform.position = p;
        camOffset = Vector3.MoveTowards(camOffset, cameraOffset, Time.deltaTime * offsetTransitionSpeed);
        tp.m_TrackedObjectOffset = camOffset;
    }

    public void CameraOffset(Vector3 raw)
    {
        cameraOffset = raw * camOffsetMultiplier;
    }

    private void OnDrawGizmos()
    {
        Cave c = FindObjectOfType<Cave>();
        Gizmos.DrawCube(Vector3.right *( c.startX + cameraTargetXClamp), Vector3.one);
        Gizmos.DrawCube(Vector3.right * (c.startX + c.sizeX - cameraTargetXClamp), Vector3.one);
    }


}
