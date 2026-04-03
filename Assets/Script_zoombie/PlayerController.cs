using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 3f;
    private Rigidbody2D rb;

    private bool isStopped = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isStopped)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        rb.velocity = new Vector2(speed, rb.velocity.y);
    }

    public void StopMoving()
    {
        Debug.Log("?? StopMoving");
        isStopped = true;
        rb.velocity = Vector2.zero;
    }

    public void ResumeMoving()
    {
        Debug.Log("?? ResumeMoving");
        isStopped = false;
        rb.WakeUp();
    }

}
