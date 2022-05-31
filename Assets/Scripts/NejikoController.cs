using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NejikoController : MonoBehaviour
{
    const int MinLane = -2;
    const int MaxLane = 2;
    const float LaneWidth = 1.0f;
    const int DefaultLife = 3;
    const float StumDuration = 0.5f;

    CharacterController controller;
    Animator animator;

    Vector3 moveDirection = Vector3.zero;
    int targetLane;
    int life = DefaultLife;
    float recoverTime = 0.0f;

    public float gravity;
    public float speedZ;
    public float speedX;            // 横方向スピードのパラメータ
    public float speedJump;
    public float accelerationZ;     // 前進加速度のパラメータ

    // ライフ取得関数
    public int Life()
    {
        return life;
    }

    // 気絶判定
    bool IsStun()
    {
        return recoverTime > 0.0f || life <= 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        // 必要なコンポーネントを自動取得
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // デバッグ用キー入力
        if (Input.GetKeyDown("left")) MoveToLeft();
        if (Input.GetKeyDown("right")) MoveToRight();
        if (Input.GetKeyDown("space")) Jump();

        // 気絶時の行動
        if (IsStun())
        {
            // 動きを止め気絶状態からの復帰カウントを進める
            moveDirection.x = 0.0f;
            moveDirection.z = 0.0f;
            recoverTime -= Time.deltaTime;
        }
        else
        {
            // 徐々に加速しZ方向に常に前進させる
            float acceleratedZ = moveDirection.z + (accelerationZ * Time.deltaTime);
            moveDirection.z = Mathf.Clamp(acceleratedZ, 0, speedZ);

            // X方向は目標のポジションまでの差分の割合で速度を計算
            float ratioX = (targetLane * LaneWidth - transform.position.x) / LaneWidth;
            moveDirection.x = ratioX * speedX;
        }

        /*
        // 接地しているかの判定
        if (controller.isGrounded)
        {
            // 前進ベロシティの設定
            if (Input.GetAxis("Vertical") > 0.0f)
            {
                moveDirection.z = Input.GetAxis("Vertical") * speedZ;
            }
            else
            {
                moveDirection.z = 0;
            }

            // 方向の回転
            transform.Rotate(0, Input.GetAxis("Horizontal") * 3, 0);

            // ジャンプ処理
            if (Input.GetButton("Jump"))
            {
                moveDirection.y = speedJump;
                animator.SetTrigger("Jump");
            }
        }
        */

        // 重力分の力を毎フレーム追加
        moveDirection.y -= gravity * Time.deltaTime;

        // 移動実行
        Vector3 globalDirection = transform.TransformDirection(moveDirection);
        controller.Move(globalDirection * Time.deltaTime);

        // 移動後接地していたらY方向の速度はリセットする
        if (controller.isGrounded) moveDirection.y = 0;

        // 速度が0以上なら走っているフラグをtrueにする
        animator.SetBool("run", moveDirection.z > 0.0f);
    }

    // 左のレーンに移動を開始
    public void MoveToLeft()
    {
        // 気絶時の入力キャンセル
        if (IsStun()) return;
        if (controller.isGrounded && targetLane > MinLane) targetLane--;
    }

    // 右のレーンに移動を開始
    public void MoveToRight()
    {
        // 気絶時の入力キャンセル
        if (IsStun()) return;
        if (controller.isGrounded && targetLane < MaxLane) targetLane++;
    }

    // ジャンプ関数
    public void Jump()
    {
        // 気絶時の入力キャンセル
        if (IsStun()) return;
        if (controller.isGrounded)
        {
            moveDirection.y = speedJump;

            // ジャンプトリガーを設定
            animator.SetTrigger("Jump");
        }
    }

    // CharacterControllerに衝突判定が生じた時の処理
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // 気絶時の入力キャンセル
        if (IsStun()) return;

        // ヒット処理
        if (hit.gameObject.tag == "Robo")
        {
            // ライフを減らして気絶状態に移行
            life--;
            recoverTime = StumDuration;

            // ダメージトリガーを設定
            animator.SetTrigger("damage");

            // ヒットしたオブジェクトは削除
            Destroy(hit.gameObject);
        }
    }
}
