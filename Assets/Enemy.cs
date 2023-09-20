using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public LayerMask groundMask;
    public float moveSpeed;
    bool moveRight;
    [SerializeField] bool grounded;
    public float gravity = .2f;
   [SerializeField] float currentGravity;

    [Header("Jump")]
    public float jumpHeight = 3;
    public float jumpVelocity;
    public float jumpTimer;
    float lastJumpTime;

    [Header("Ground")]
    public float groundCollisionRadius = .2f;
    public float groundCollisionLength = .2f;
    public float groundCollisionWidth = .2f;
    public float groundCollisionOffset = .2f;

    [Header("Wall")]
    public float wallCollisionRadius = .2f;
    public float wallCollisionLength = .2f;
    public float wallCollisionHeight = .2f;
    public float wallCollisionOffset = .2f;




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 velocity = moveRight ? Vector3.right : Vector3.left;
       
     
        grounded = (Physics.CapsuleCast(transform.position + (Vector3.up * groundCollisionOffset) + (Vector3.right * groundCollisionWidth / 2f),
                                        transform.position + (Vector3.up * groundCollisionOffset) + (Vector3.left * groundCollisionWidth / 2f),
                                        groundCollisionRadius,Vector3.down,groundCollisionLength, groundMask));

       if (Physics.CapsuleCast(transform.position + ((moveRight? Vector3.right : Vector3.left) * wallCollisionOffset) + (Vector3.up * wallCollisionHeight / 2f),
                               transform.position + ((moveRight? Vector3.right : Vector3.left) * wallCollisionOffset) + (Vector3.down * wallCollisionHeight / 2f),
                               wallCollisionRadius, moveRight? Vector3.right : Vector3.left, wallCollisionLength, groundMask))
        {
            moveRight = !moveRight;
        }

        if (!grounded)
        {
            jumpVelocity -= Time.deltaTime * 10;
            velocity = velocity * moveSpeed/2;
            currentGravity += Time.deltaTime * 10;
            velocity += Vector3.down * Mathf.Clamp(currentGravity, 0, gravity);
        }
        else
        {
            jumpVelocity = 0;
            velocity = velocity * moveSpeed;
            currentGravity = 0;
        }

        if(Time.time >= lastJumpTime + jumpTimer)
        {
            Jump();
        }
        transform.position += ((jumpVelocity * Vector3.up) + velocity )* Time.deltaTime;
    }

    public void Jump()
    {
        if (!grounded) return;
        lastJumpTime = Time.time;
        jumpVelocity = jumpHeight;
    }

    private void OnDrawGizmos()
    {

        Gizmos.DrawRay(transform.position + (Vector3.up * groundCollisionOffset) + (Vector3.right * groundCollisionWidth / 2f), Vector3.down * groundCollisionLength);
        Gizmos.DrawRay(transform.position + (Vector3.up * groundCollisionOffset) + (Vector3.left * groundCollisionWidth / 2f), Vector3.down * groundCollisionLength);
        Gizmos.DrawWireSphere(transform.position + (Vector3.up * groundCollisionOffset) + (Vector3.right * groundCollisionWidth / 2f), groundCollisionRadius);
        Gizmos.DrawWireSphere(transform.position + (Vector3.up * groundCollisionOffset) + (Vector3.left * groundCollisionWidth / 2f), groundCollisionRadius);

        Gizmos.DrawRay(transform.position + ((moveRight ? Vector3.right : Vector3.left) * wallCollisionOffset) + (Vector3.up * wallCollisionHeight / 2f), (moveRight ? Vector3.right : Vector3.left) * wallCollisionLength);
        Gizmos.DrawRay(transform.position + ((moveRight ? Vector3.right : Vector3.left) * wallCollisionOffset) + (Vector3.down * wallCollisionHeight / 2f), (moveRight ? Vector3.right : Vector3.left) * wallCollisionLength); 
        Gizmos.DrawWireSphere(transform.position + ((moveRight ? Vector3.right : Vector3.left) * wallCollisionOffset) + (Vector3.up * wallCollisionHeight / 2f), wallCollisionRadius);
        Gizmos.DrawWireSphere(transform.position + ((moveRight ? Vector3.right : Vector3.left) * wallCollisionOffset) + (Vector3.down * wallCollisionHeight / 2f), wallCollisionRadius);

    }
}
