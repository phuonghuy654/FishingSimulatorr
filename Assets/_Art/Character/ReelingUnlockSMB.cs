using UnityEngine;

// StateMachineBehaviour là lớp cơ sở để tạo các hành vi cho các trạng thái hoạt ảnh.
public class ReelingUnlockSMB : StateMachineBehaviour
{
    // Hàm này được gọi ngay trước khi Animator thoát khỏi trạng thái mà script này được gắn vào (trạng thái Reeling).
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 1. Tắt cờ "isFishing".
        // Việc này sẽ ngay lập tức mở khóa di chuyển trong ThirdPersonController.cs.
        animator.SetBool("isFishing", false);

        // 2. Debug (Tùy chọn)
        Debug.Log("ReelingUnlockSMB: isFishing reset to FALSE. Movement unlocked.");
    }

    // Bạn có thể xóa các hàm khác (OnStateEnter, OnStateUpdate, v.v.)
    // vì chúng ta chỉ cần OnStateExit.
}