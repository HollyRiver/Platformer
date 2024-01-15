using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    /*
    피격 애니메이션 적용을 복잡하게 꼬아놓은 상태, 추후 코드 참고 시 Any State와 Trigger를 이용할 것.
    tlqkf 코드 존나 꼬아놨네, 적당히 최적화해서 쓸 수 있도록 할 것...
    */

    // Global Variables
    public float RunSpeed;
    public float JumpPower;
    public bool PlayerInSlope;
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
    float RightRayAngle;
    float LeftRayAngle;

    // Other Class Import
    Platform plat;

    void Awake()
    {
        // Local Variables and Component Setting
        plat = FindObjectOfType<Platform>();
        rigid = GetComponent<Rigidbody2D>();
        Model = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        PlayerHitBox = GetComponent<BoxCollider2D>();
        Jump = false;
        IsMoving = false;
        CantMoving = false;
        PlayerInSlope = false;
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
                rigid.velocity = new Vector2(rigid.velocity.x, 0);  // 가속도 초기화
                rigid.AddForce(new Vector2(0, JumpPower), ForceMode2D.Impulse);  // 점프 발생
                PlayerInSlope = false;
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

            
            // Player Stop Logic
            if (!IsMoving && !CantMoving) {
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

            if (rigid.velocity.y != 0 || PlayerInSlope) {
                // Falling Animation
                if (rigid.velocity.y < 0) {
                    anim.SetBool("IsFalling", true);
                }

                // 왼쪽을 보고 있을 때
                if (Model.flipX) {
                    RaycastHit2D RayHitRightDiag = Physics2D.Raycast(rigid.position, new Vector3(0.664f, -1, 0), 0.601f, LayerMask.GetMask("Platform"));  // 레이캐스트 생성, 물리에서 적용됨.
                    RaycastHit2D RayHitDown = Physics2D.Raycast(rigid.position, Vector3.down, 0.5f, LayerMask.GetMask("Platform"));
                    RaycastHit2D RayHitLeftDiag = Physics2D.Raycast(rigid.position, new Vector3(-0.536f, -1, 0), 0.568f, LayerMask.GetMask("Platform"));

                    if (RayHitDown.collider != null || RayHitLeftDiag.collider != null || RayHitRightDiag.collider != null)  // 레이가 물리 개체를 만났을 때
                    {
                        if ((rigid.velocity.y <= 0) && (RayHitDown.distance <= 0.5f || RayHitLeftDiag.distance <= 0.601f || RayHitRightDiag.distance <= 0.568f)) {
                            anim.SetBool("IsJumping", false);
                            anim.SetBool("IsFalling", false);
                        }
                    }

                    // 경사로 인식 로직
                    if (RayHitRightDiag.collider != null || RayHitLeftDiag.collider != null) {
                        RightRayAngle = Vector3.Angle(RayHitRightDiag.normal, new Vector3(0.664f, -1, 0));
                        LeftRayAngle = Vector3.Angle(RayHitLeftDiag.normal, new Vector3(-0.536f, -1, 0));

                        // Debug.DrawRay(rigid.position, new Vector3(0.664f, -1, 0));
                        // Debug.DrawRay(rigid.position, new Vector3(-0.536f, -1, 0));
                        
                        if (RayHitRightDiag.collider != null && RightRayAngle >= 155) {
                            // Debug.DrawRay(RayHitRightDiag.point, RayHitRightDiag.normal);
                            Debug.Log(RightRayAngle);
                            PlayerInSlope = true;
                        }

                        else if (RayHitLeftDiag.collider != null && LeftRayAngle >= 155) {
                            // Debug.DrawRay(RayHitLeftDiag.point, RayHitLeftDiag.normal);
                            Debug.Log(LeftRayAngle);
                            PlayerInSlope = true;
                        }
                    }

                    else {
                        PlayerInSlope = false;
                    }
                }
                
                // 오른쪽을 보고 있을 때
                else {
                    RaycastHit2D RayHitLeftDiag = Physics2D.Raycast(rigid.position, new Vector3(-0.664f, -1, 0), 0.601f, LayerMask.GetMask("Platform"));  // 레이캐스트 생성, 물리에서 적용됨.
                    RaycastHit2D RayHitDown = Physics2D.Raycast(rigid.position, Vector3.down, 0.5f, LayerMask.GetMask("Platform"));
                    RaycastHit2D RayHitRightDiag = Physics2D.Raycast(rigid.position, new Vector3(0.536f, -1, 0), 0.568f, LayerMask.GetMask("Platform"));

                    // 애니메이션 비활성화...?
                    if (RayHitDown.collider != null || RayHitLeftDiag.collider != null || RayHitRightDiag.collider != null)  // 레이가 물리 개체를 만났을 때
                    {
                        if ((rigid.velocity.y <= 0) && (RayHitDown.distance <= 0.5f || RayHitLeftDiag.distance <= 0.601f || RayHitRightDiag.distance <= 0.568f)) {
                            anim.SetBool("IsJumping", false);
                            anim.SetBool("IsFalling", false);
                        }
                    }

                    if (RayHitRightDiag.collider != null || RayHitLeftDiag.collider != null) {
                        RightRayAngle = Vector3.Angle(RayHitRightDiag.normal, new Vector3(0.536f, -1, 0));
                        LeftRayAngle = Vector3.Angle(RayHitLeftDiag.normal, new Vector3(-0.664f, -1, 0));

                        Debug.DrawRay(rigid.position, new Vector3(-0.664f, -1, 0));
                        Debug.DrawRay(rigid.position, new Vector3(0.536f, -1, 0));

                        if ((RayHitRightDiag.collider != null && RightRayAngle >= 155) || (RayHitLeftDiag.collider != null && LeftRayAngle >= 155)) {
                            PlayerInSlope = true;
                        }
                    }
                    
                    else {
                        PlayerInSlope = false;
                    }
                }
            }
            
            if (PlayerInSlope && !CantMoving && !IsMoving) {
                rigid.sharedMaterial = mat[2];
                plat.InSlope();
            }

            else {
                rigid.sharedMaterial = mat[0];
                plat.OutSlope();
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
        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Trap")) {
            if (other.gameObject.CompareTag("Enemy") && transform.position.y > other.transform.position.y + 0.4f) {
                OnAttack(other.transform, transform.position.x);  // 트랜스폼에서 컴포넌트를 추출할 수 있음...?
            }
            
            else {
                gameObject.layer = 10;
                anim.SetTrigger("Damaged");
                OnDamaged();
                CantMove();
                PushedOut(other.transform.position);
                Invoke("OffDamaged", 2);
                Invoke("CanMove", 1);
            }
        }
    }

    void OnAttack(Transform enemy, float PlayerPointX) {
        EnemyMovement enemyMove = enemy.GetComponent<EnemyMovement>();
        enemyMove.OnDamaged(PlayerPointX);
        anim.SetBool("IsFalling", false);
        anim.SetBool("IsJumping", false);  // 다시 점프가 가능함
        Invoke("JumpBlock", 0.05f);
        rigid.AddForce(Vector2.up*2, ForceMode2D.Impulse);
    }

    void JumpBlock() {
        if (anim.GetBool("IsJumping"))
            anim.SetBool("IsFalling", true);
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
