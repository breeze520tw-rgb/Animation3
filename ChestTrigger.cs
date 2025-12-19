using UnityEngine;

public class ChestTrigger : MonoBehaviour
{
    [Header("設定")]
    public int questionIndex = 0; // 設定這個寶箱要問 CSV 裡的第幾題 (從 0 開始)
    private bool hasTriggered = false;

    // 1. 針對 3D 物件的觸發 (Collider 勾選 Is Trigger)
    void OnTriggerEnter(Collider other)
    {
        TryTrigger(other.gameObject);
    }

    // 2. 針對 3D 物件的碰撞 (Collider 沒勾 Is Trigger，撞到也會觸發)
    void OnCollisionEnter(Collision collision)
    {
        TryTrigger(collision.gameObject);
    }

    // 3. 針對 2D 物件的觸發 (如果你的遊戲是 2D 的)
    void OnTriggerEnter2D(Collider2D other)
    {
        TryTrigger(other.gameObject);
    }

    // 4. 針對 2D 物件的碰撞
    void OnCollisionEnter2D(Collision2D collision)
    {
        TryTrigger(collision.gameObject);
    }

    // 統一處理邏輯
    void TryTrigger(GameObject obj)
    {
        // 檢查碰到的是不是玩家 (Tag 必須是 "Player")
        if (obj.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true; // 標記已觸發，避免重複跳出
            Debug.Log("玩家觸發寶箱，自動開始問答！");

            // 呼叫 QuizManager 開啟問答
            if (QuizManager.Instance != null)
            {
                QuizManager.Instance.OpenQuiz(questionIndex);
            }
        }
    }
}