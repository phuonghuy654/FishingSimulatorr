using UnityEngine;
using StarterAssets;

public class TestFishing : MonoBehaviour
{
    private Animator _animator;
    // Khóa di chuyển được xử lý trong ThirdPersonController.cs

    // Tên các Parameters trong Animator
    private readonly string IsFishingParam = "isFishing";
    // 🔑 Sửa: Bỏ private thứ hai
    private readonly string StartCastingParam = "StartCasting";
    private readonly string StartReelingParam = "StartReeling";

    void Start()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("Animator component not found!");
        }
    }

    void Update()
    {
        // 1. INPUT BẮT ĐẦU CÂU (Nhấn F)
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!_animator.GetBool(IsFishingParam))
            {
                StartCasting();
            }
        }

        // 2. INPUT KÉO CẦN (Nhấn R)
        if (Input.GetKeyDown(KeyCode.R) && _animator.GetBool(IsFishingParam))
        {
            StartReeling();
        }
    }

    private void StartCasting()
    {
        _animator.SetBool(IsFishingParam, true);
        _animator.SetTrigger(StartCastingParam);
    }

    private void StartReeling()
    {
        _animator.SetTrigger(StartReelingParam);
        // Việc reset cờ isFishing sẽ được gọi bởi Animation Event trong hoạt ảnh Reeling.
    }

    // 🔑 HÀM THAY THẾ CHO REELINGUNLOCKSMB.CS
    // Hàm này được gọi bởi Animation Event của clip Reeling
    public void OnReelingFinished()
    {
        // Khi hoạt ảnh kéo cần kết thúc, reset cờ để cho phép di chuyển.
        _animator.SetBool(IsFishingParam, false);
        Debug.Log("Reeling animation complete. isFishing reset by Animation Event.");
    }
}