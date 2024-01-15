using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteSetting;

    public int nextMove;
    public PhysicsMaterial2D[] mat = new PhysicsMaterial2D[1];

    int CompareX;
    bool IsDamaged;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteSetting = GetComponent<SpriteRenderer>();
        IsDamaged = false;
        Think();  // 최초값 할당. -1, 0, 1
    }

    void FixedUpdate()
    {
        if (!IsDamaged) {
            rigid.velocity = new Vector2(nextMove * 1.5f, rigid.velocity.y);
            
            // 빠지거나 벽 방향으로 계속 걷지 않도록 설정
            if (!spriteSetting.flipX) {
                RaycastHit2D RayHitFloor = Physics2D.Raycast(transform.position + Vector3.left * 0.6f, Vector3.down, 1, LayerMask.GetMask("Platform"));
                RaycastHit2D RayHitFront = Physics2D.Raycast(transform.position, Vector3.left + Vector3.up, 1, LayerMask.GetMask("Platform"));

                // 접촉 반경을 가시화하고 싶을 경우 주석 처리를 풀것.
                // Debug.DrawRay(transform.position + Vector3.left * 0.6f, Vector3.down, Color.green);
                // Debug.DrawRay(transform.position, Vector3.left + Vector3.up, Color.green);

                if ((RayHitFloor.collider == null) || (RayHitFront.collider != null && RayHitFront.collider.tag == "Platform")) {
                    nextMove *= -1;
                    spriteSetting.flipX = true;
                    CancelInvoke();
                    Invoke("Think", 3);
                }
            }

            else {
                RaycastHit2D RayHitFloor = Physics2D.Raycast(transform.position + Vector3.right * 0.6f, Vector3.down, 1, LayerMask.GetMask("Platform"));
                RaycastHit2D RayHitFront = Physics2D.Raycast(transform.position, Vector3.right + Vector3.up, 1, LayerMask.GetMask("Platform"));

                // Debug.DrawRay(transform.position + Vector3.right * 0.6f, Vector3.down, Color.green);
                // Debug.DrawRay(transform.position, Vector3.right + Vector3.up, Color.green);

                if ((RayHitFloor.collider == null) || (RayHitFront.collider != null && RayHitFront.collider.tag == "Platform")) {
                    nextMove *= -1;
                    spriteSetting.flipX = false;
                    CancelInvoke();
                    Invoke("Think", 3);
                }
            }
        }
    }

    // 재귀함수
    void Think()
    {
        nextMove = Random.Range(-1, 2);  // 랜덤 정수 호출 함수
        // 스프라이트 및 애니메이션 변경, 5초마다 한번만 호출되므로 연산이 최소화됨.
        anim.SetInteger("WalkSpeed", nextMove);

        if (nextMove != 0) {
            spriteSetting.flipX = nextMove == 1;
        }

        float nextThinkTime = Random.Range(3f, 5f);  // 랜덤 실수 호출 함수
        Invoke("Think", nextThinkTime);  // MonoBehaviour 기본 제공 함수, 5초 뒤에 해당 함수를 호출한다.
    }

    public void OnDamaged(float PlayerPointX)
    {
        IsDamaged = true;
        spriteSetting.color = new Color32(255, 255, 255, 155);
        rigid.sharedMaterial = mat[0];
        gameObject.layer = 10;
        anim.SetTrigger("DoDamaged");
        CompareX = transform.position.x > PlayerPointX ? 1 : -1;

        rigid.AddForce(new Vector2(CompareX, 1)*2, ForceMode2D.Impulse);

        Invoke("Pop", 1);
    }

    void Pop()
    {
        anim.SetBool("Pop", true);
        spriteSetting.color = new Color32(255, 255, 255, 255);
        rigid.constraints = RigidbodyConstraints2D.FreezeAll;
        Invoke("Destroy", 0.25f);
    }
    void Destroy()
    {
        gameObject.SetActive(false);
    }
}
