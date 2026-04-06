using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FishingMinigameController : MonoBehaviour
{
    [Header("THIẾT LẬP MINIGAME")]
    [Tooltip("Tiến trình bắt đầu (0 -> 1, mặc định 0.33 = 33%)")]
    [Range(0f, 1f)]
    public float initialProgress = 0.33f;

    [Tooltip("Kích thước vùng cá (% theo thanh 0 -> 100)")]
    [Range(10f, 80f)]
    public float fishZoneSize = 40f;

    [Tooltip("Kích thước mục tiêu người chơi (% theo thanh 0 -> 100)")]
    [Range(1f, 20f)]
    public float playerTargetSize = 5f;

    [Space]
    [Tooltip("Tốc độ tăng tiến trình mỗi giây khi mục tiêu nằm trong vùng cá")]
    public float progressGainRate = 0.6f;

    [Tooltip("Tốc độ giảm tiến trình mỗi giây khi mục tiêu nằm ngoài vùng cá")]
    public float progressLossRate = 0.4f;

    [Space]
    [Tooltip("Tốc độ mục tiêu di chuyển lên khi giữ chuột")]
    public float playerTargetAscendSpeed = 60f;

    [Tooltip("Tốc độ mục tiêu rơi xuống khi thả chuột")]
    public float playerTargetDescendSpeed = 40f;

    [Space]
    [Tooltip("Tốc độ vùng cá di chuyển ngang qua lại")]
    public float fishZoneMoveSpeed = 20f;

    // Sự kiện callback để FishingController lắng nghe
    public UnityEvent<bool> OnMinigameEnd = new UnityEvent<bool>();

    // --- Biến nội bộ ---
    private bool isActive = false;       // Có đang chơi minigame không?
    private float currentProgress;       // Tiến trình hiện tại (0 -> 1)

    private float fishZonePosition;      // Vị trí bắt đầu của vùng cá (0 -> 100)
    private float playerTargetPosition;  // Vị trí mục tiêu người chơi (0 -> 100)

    private int fishZoneDirection = 1;   // Hướng di chuyển vùng cá (+1 = phải, -1 = trái)

    private float progressToWin = 1f;    // Ngưỡng thắng (điểm max)

    void Update()
    {
        if (!isActive) return;

        HandlePlayerTargetMovement(); // xử lý di chuyển mục tiêu
        HandleFishZoneMovement();     // xử lý vùng cá trôi ngang
        HandleProgress();             // xử lý tiến trình thắng/thua
    }
    public void StartMiniGame()
    {
        currentProgress = initialProgress;

        // Đặt mục tiêu người chơi ở giữa thanh
        playerTargetPosition = 50f - (playerTargetSize / 2f);

        // Đặt vùng cá ở ngẫu nhiên trong thanh
        fishZonePosition = Random.Range(0f, 100f - fishZoneSize);

        // Random hướng trôi ban đầu
        fishZoneDirection = Random.value > 0.5f ? 1 : -1;

        isActive = true;
        Debug.Log("[Minigame] BẮT ĐẦU");
    }

    private void EndMinigame(bool success)
    {
        isActive = false;
        Debug.Log(success ? "[Minigame] CÂU THÀNH CÔNG!" : "[Minigame] LÀM CÁ XỔNG RỒI!");
        OnMinigameEnd.Invoke(success);
    }

    private void HandlePlayerTargetMovement()
    {
        if (Input.GetMouseButton(0))
        {
            playerTargetPosition += playerTargetAscendSpeed * Time.deltaTime;
        }
        else
        {
            playerTargetPosition -= playerTargetDescendSpeed * Time.deltaTime;
        }

        // Giới hạn trong 0 -> 100 - playerTargetSize
        playerTargetPosition = Mathf.Clamp(playerTargetPosition, 0f, 100f - playerTargetSize);
    }

    private void HandleFishZoneMovement()
    {
        fishZonePosition += fishZoneDirection * fishZoneMoveSpeed * Time.deltaTime;

        // Nếu chạm biên thì đổi hướng
        if (fishZonePosition <= 0f)
        {
            fishZonePosition = 0f;
            fishZoneDirection = 1;
        }
        else if (fishZonePosition >= 100f - fishZoneSize)
        {
            fishZonePosition = 100f - fishZoneSize;
            fishZoneDirection = -1;
        }
    }

    private void HandleProgress()
    {
        if (IsTargetInZone())
        {
            currentProgress += progressGainRate * Time.deltaTime;
        }
        else
        {
            currentProgress -= progressLossRate * Time.deltaTime;
        }

        currentProgress = Mathf.Clamp01(currentProgress);

        if (currentProgress >= progressToWin)
        {
            EndMinigame(true);
        }
        else if (currentProgress <= 0f)
        {
            EndMinigame(false);
        }
    }


    private bool IsTargetInZone()
    {
        float playerTarget_min = playerTargetPosition;
        float playerTarget_max = playerTargetPosition + playerTargetSize;

        float fishZone_min = fishZonePosition;
        float fishZone_max = fishZonePosition + fishZoneSize;

        return playerTarget_min < fishZone_max && playerTarget_max > fishZone_min;
    }


}
