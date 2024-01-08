using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Global Variables
    public float RunSpeed;
    public float JumpPower;

    // Local Variables
    Rigidbody2D rigid;
    SpriteRenderer Model;
    Animator anim;
    bool IsLanding;  // true : 착지 중, false : 떠있음
    bool Jump;  // true : 점프 키 누름, false : 점프 키 누르지 않음
    float h;
    bool IsMoving;

    void Start()
    {
        // Local Variables and Component Setting
        rigid = GetComponent<Rigidbody2D>();
        Model = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        IsLanding = true;
        Jump = false;
        IsMoving = false;
    }

    void FixedUpdate()
    {
        // Player Movement and Jumping Logic
        h = Input.GetAxisRaw("Horizontal");

        if (IsLanding && Jump) {
            rigid.AddForce(new Vector2(h, JumpPower), ForceMode2D.Impulse);
            Jump = false;
            IsLanding = false;
        }

        else {
            rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);
            Debug.Log(h);
        }

        // MaxSpeed Exceed Logic
        if (rigid.velocity.x > RunSpeed) {
            rigid.velocity = new Vector2(RunSpeed, rigid.velocity.y);
        }

        else if (rigid.velocity.x < RunSpeed * (-1)) {
            rigid.velocity = new Vector2(RunSpeed * (-1), rigid.velocity.y);
        }

        // Player Stop Logic
        if (!IsMoving) {
            rigid.velocity = new Vector2(rigid.velocity.x * 0.6f, rigid.velocity.y);
        }

        // Showing Animation Logic
        // if (Mathf.Abs(rigid.velocity.x) < 0.3f) {
        //     anim.SetBool("IsRunning", false);
        // }

        // else {
        //     anim.SetBool("IsRunning", true);
        // }

        // Player Jumping Logic
    }

    void Update()
    {   
        // Player Fronting And Animation Logic
        if (Input.GetButtonDown("Horizontal") || h != 0) {
            IsMoving = true;
            Model.flipX = h == -1;
            anim.SetBool("IsRunning", true);  // Setting Parameter Value is true.
        }

        else if (Input.GetButtonUp("Horizontal") || h == 0) {
            IsMoving = false;
            anim.SetBool("IsRunning", false);
        }

        // Player Jumping Logic
        if (Input.GetButtonDown("Jump") && IsLanding) {
            Jump = true;  // 점프를 누름
        }
    }

    // 궁여지책으로 구현한 이중 점프 방지, 벽면에 닿아도 점프가 초기화되기에 수정이 필요.
    void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Platform") {
            IsLanding = true;
        }
    }

    void OnCollisionExit2D(Collision2D other) {
        if (other.gameObject.tag == "Platform") {
            IsLanding = false;
        }
    }
}
