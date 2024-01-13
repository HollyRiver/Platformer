using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    // Global Variables
    public float RunSpeed;
    public float JumpPower;
    public PhysicsMaterial2D[] mat = new PhysicsMaterial2D[3];

    // Local Variables
    Rigidbody2D rigid;
    SpriteRenderer Model;
    Animator anim;
    BoxCollider2D PlayerHitBox;
    // bool IsLanding;  // true : 착지 중, false : 떠있음 >> 필요없음, anim.GetBool("IsJumping")으로 대체 가능.
    bool Jump;  // true : 점프 키 누름, false : 점프 키 누르지 않음
    float h;
    bool IsMoving;
    bool CantMoving;
    int dirc;

    void Awake()
    {
        // Local Variables and Component Setting
        rigid = GetComponent<Rigidbody2D>();
        Model = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        PlayerHitBox = GetComponent<BoxCollider2D>();
        Jump = false;
        IsMoving = false;
        CantMoving = false;
    }

    void FixedUpdate()
    {   
        // 플레이어가 피격후 모션이 완료되기 전까지는 아래 로직이 작동하지 않음
        if (!CantMoving) 
        {
            
            // Player Movement and Jumping Logic
            h = Input.GetAxisRaw("Horizontal");

            if (Jump) {
                rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);
                rigid.velocity = new Vector2(rigid.velocity.x, JumpPower);  // AddForce로 할 경우 경사로에서 비정상적인 점프 발생
                Jump = false;
            }

            else {
                rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);
            }

            // MaxSpeed Exceed Logic
            if (rigid.velocity.x > RunSpeed) {
                rigid.velocity = new Vector2(RunSpeed, rigid.velocity.y);
            }

            else if (rigid.velocity.x < RunSpeed * (-1)) {
                rigid.velocity = new Vector2(RunSpeed * (-1), rigid.velocity.y);
            }

            
            // Player Stop Logic : 해당 로직 때문에 피격 시 밀리는 부분에서 문제가 생김.

            if (!IsMoving && !anim.GetBool("IsDamaged")) {
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

            // Landing Platform by RayCast
            // Debug.DrawRay(rigid.position, Vector3.down,new Color32(0, 255, 0, 100));  // 디버그 씬에서 개체 중앙을 기준으로 선을 쏨

            if (rigid.velocity.y < 0) {
                anim.SetBool("IsFalling", true);  // 낙하중임을 명시함

                // 왼쪽을 보고 있을 때
                if (Model.flipX) {
                    RaycastHit2D RayHitRightDiag = Physics2D.Raycast(rigid.position, new Vector3(0.664f, -1, 0), 0.601f, LayerMask.GetMask("Platform"));  // 레이캐스트 생성, 물리에서 적용됨.
                    RaycastHit2D RayHitDown = Physics2D.Raycast(rigid.position, Vector3.down, 0.5f, LayerMask.GetMask("Platform"));
                    RaycastHit2D RayHitLeftDiag = Physics2D.Raycast(rigid.position, new Vector3(-0.536f, -1, 0), 0.568f, LayerMask.GetMask("Platform"));

                    if (RayHitDown.collider != null || RayHitLeftDiag.collider != null || RayHitRightDiag.collider != null)  // 레이가 물리 개체를 만났을 때
                    {
                        if (RayHitDown.distance <= 0.5f || RayHitLeftDiag.distance <= 0.601f || RayHitRightDiag.distance <= 0.568f) {
                            anim.SetBool("IsJumping", false);
                            anim.SetBool("IsFalling", false);

                            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Damaged")) {
                                anim.SetBool("DamagedBlock", true);
                            }
                        }
                    }
                }
                
                // 오른쪽을 보고 있을 때
                else {
                    RaycastHit2D RayHitLeftDiag = Physics2D.Raycast(rigid.position, new Vector3(-0.664f, -1, 0), 0.601f, LayerMask.GetMask("Platform"));  // 레이캐스트 생성, 물리에서 적용됨.
                    RaycastHit2D RayHitDown = Physics2D.Raycast(rigid.position, Vector3.down, 0.5f, LayerMask.GetMask("Platform"));
                    RaycastHit2D RayHitRightDiag = Physics2D.Raycast(rigid.position, new Vector3(0.536f, -1, 0), 0.568f, LayerMask.GetMask("Platform"));

                    if (RayHitDown.collider != null || RayHitLeftDiag.collider != null || RayHitRightDiag.collider != null)  // 레이가 물리 개체를 만났을 때
                    {
                        if (RayHitDown.distance <= 0.5f || RayHitLeftDiag.distance <= 0.601f || RayHitRightDiag.distance <= 0.568f) {
                            anim.SetBool("IsJumping", false);
                            anim.SetBool("IsFalling", false);

                            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Damaged")) {
                                anim.SetBool("DamagedBlock", true);
                            }
                        }
                    }
                }
            }
        }
    }

    void Update()
    {   
        if (!CantMoving)
        {
            // 플레이어 피격 후 모션이 완료되기 전까지는 해당 로직이 발생하지 않음
            // Player Fronting And Animation Logic
            if (Input.GetButtonDown("Horizontal") || h != 0) {
                IsMoving = true;
                if (h == -1) {
                    Model.flipX = true;
                    PlayerHitBox.offset = new Vector2(0.032f, 0);
                }

                else if (h == 1) {
                    Model.flipX = false;
                    PlayerHitBox.offset = new Vector2(-0.032f, 0);
                }
                
                anim.SetBool("IsRunning", true);  // Setting Parameter Value is true.
            }

            else if (Input.GetButtonUp("Horizontal") || h == 0) {
                IsMoving = false;
                anim.SetBool("IsRunning", false);
            }

            // Player Jumping Logic
            if (!anim.GetBool("IsFalling") && !anim.GetBool("IsJumping") && Input.GetButtonDown("Jump"))  // 땅에 있을 때 점프 버튼을 누르면
            {
                Jump = true;  // 점프를 누름
                anim.SetBool("IsJumping", true);
            }
        }
    }

    // 궁여지책으로 구현한 이중 점프 방지, 박스콜라이더를 이용했기에 랜더링 속도가 느림
    // void OnCollisionEnter2D(Collision2D other) {
    //     if (other.gameObject.tag == "Platform") {
    //         IsLanding = true;
    //         anim.SetBool("IsJumping", false);
    //     }
    // }

    // void OnCollisionExit2D(Collision2D other) {
    //     if (other.gameObject.tag == "Platform") {
    //         IsLanding = false;
    //     }
    // }

    void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Enemy") {
            gameObject.layer = 10;
            anim.SetBool("IsDamaged", true);
            anim.SetBool("IsJumping", false);
            anim.SetBool("IsRunning", false);
            anim.SetBool("IsFalling", true);
            anim.SetBool("DamagedBlock", false);
            OnDamaged();
            CantMove();
            PushedOut(other.transform.position);
            Invoke("OffDamaged", 2);
            Invoke("CanMove", 1);
        }
    }

    void OnDamaged() {
        Model.color = new Color32(255, 255, 255, 100);
        Invoke("OnDamaged2", 0.2f);
    }

    void OnDamaged2() {
        Model.color = new Color32(255, 255, 255, 200);
        Invoke("OnDamaged", 0.2f);
    }

    void CantMove() {
        PlayerHitBox.sharedMaterial = mat[1];
        CantMoving = true;
    }

    void CanMove() {
        PlayerHitBox.sharedMaterial = mat[0];
        CantMoving = false;
        anim.SetBool("IsDamaged", false);
        anim.SetBool("DamagedBlock", false);
    }

    void PushedOut(Vector2 EnemyPosition) {
        if (transform.position.x - EnemyPosition.x > 0) {
            dirc = 1;
            Model.flipX = true;
        }

        else {
            dirc = -1;
            Model.flipX = false;
        }

        rigid.velocity = new Vector2(dirc, 1) * 5;
        // rigid.AddForce(new Vector2(dirc, 1) * 5, ForceMode2D.Impulse);
    }

    void OffDamaged() {
        gameObject.layer = 9;
        Model.color = new Color32(255, 255, 255, 255);
        CancelInvoke();
    }
}
