using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections; // 引用 IEnumerator 需要此命名空間
using System.IO; // 引用 IO 以讀取檔案

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance;

    [Header("UI 元件 (請拖曳對應物件)")]
    public GameObject quizPanel;   // 整個問答介面的 Panel
    public Text questionText;      // 顯示題目的 Text
    public InputField answerInput; // 輸入答案的 InputField
    public Text feedbackText;      // 顯示對錯回饋的 Text
    public Button submitButton;    // 「送出答案」按鈕
    public Button retryButton;     // 「重新作答」按鈕
    public Button nextButton;      // 「下一題」按鈕

    [Header("資料設定")]
    public TextAsset csvFile;      // 請將 quiz.csv 拖曳到這裡 (需放在 Assets 資料夾)
    
    [Header("角色動畫設定 (請拖曳角色2)")]
    public SpriteRenderer characterRenderer; // 請將角色2的 SpriteRenderer 拖曳到此
    public Sprite[] winSprites;              // 答對動畫 (請將切好的12張圖拖入)
    public Sprite[] loseSprites;             // 答錯動畫 (請將切好的13張圖拖入)
    public float animSpeed = 0.1f;           // 動畫播放速度 (秒/張)

    private Sprite defaultSprite;            // 記錄角色原始圖片
    private Coroutine animCoroutine;         // 控制動畫的 Coroutine
    private List<Dictionary<string, string>> questions = new List<Dictionary<string, string>>();
    private int currentQuestionIndex = 0;

    void Awake()
    {
        Instance = this;
        LoadAutoSprites(); // 自動載入圖片與設定
        ParseCSV();
        quizPanel.SetActive(false); // 遊戲開始時先隱藏介面
    }

    void Update()
    {
        // 偵測 Enter 鍵，讓玩家不需滑鼠點擊即可送出或繼續
        if (quizPanel.activeSelf && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (submitButton.gameObject.activeSelf)
            {
                OnSubmit();
            }
            else if (nextButton.gameObject.activeSelf)
            {
                OnNext();
            }
        }
    }

    // 讀取 CSV 檔案
    void ParseCSV()
    {
        if (csvFile == null) return;

        string[] lines = csvFile.text.Split('\n');
        // 假設第一行是標題，從第二行開始讀
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            
            // 使用 Regex 分割逗號，但忽略引號內的逗號
            string pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";
            string[] values = Regex.Split(lines[i], pattern);

            if (values.Length >= 5)
            {
                Dictionary<string, string> entry = new Dictionary<string, string>();
                entry["question"] = values[0].Trim().Trim('"');
                entry["answer"] = values[1].Trim().Trim('"');
                entry["correct_feedback"] = values[2].Trim().Trim('"');
                entry["wrong_feedback"] = values[3].Trim().Trim('"');
                entry["hint"] = values[4].Trim().Trim('"');
                questions.Add(entry);
            }
        }
    }

    // 被寶箱呼叫：開啟指定題目
    public void OpenQuiz(int index)
    {
        // 修改：不再強制設定為傳入的 index，而是接續目前的題目進度，避免不同角色重複問第一題
        // currentQuestionIndex = index;

        // 如果題目已經全部問完，則重置回第一題 (循環出題)
        if (currentQuestionIndex >= questions.Count) currentQuestionIndex = 0;

        if (currentQuestionIndex < questions.Count)
        {
            // 開啟問題前，先記錄角色目前的狀態 (Idle)，以便之後恢復
            if (characterRenderer != null)
            {
                defaultSprite = characterRenderer.sprite;
            }

            quizPanel.SetActive(true);
            ShowQuestion();
            
            // 當介面自動跳出時，確保滑鼠游標顯示並解鎖，方便玩家操作
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void ShowQuestion()
    {
        // 顯示新題目時，停止之前的動畫並恢復原狀
        StopAnimation();

        var q = questions[currentQuestionIndex];
        questionText.text = "題目: " + q["question"];
        answerInput.text = "";
        feedbackText.text = "";

        // 初始化按鈕狀態：只顯示送出
        submitButton.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        
        answerInput.ActivateInputField(); // 自動讓游標跳進輸入框
        answerInput.Select(); // 確保輸入框取得焦點
    }

    // 請綁定在「送出答案」按鈕的 OnClick
    public void OnSubmit()
    {
        string playerAnswer = answerInput.text.Trim();
        string correctAnswer = questions[currentQuestionIndex]["answer"];

        submitButton.gameObject.SetActive(false); // 隱藏送出按鈕

        if (playerAnswer == correctAnswer)
        {
            // 答對
            feedbackText.text = "系統: " + questions[currentQuestionIndex]["correct_feedback"];
            feedbackText.color = Color.green;
            
            // 答對 -> 只顯示「下一題」
            nextButton.gameObject.SetActive(true);

            // 播放答對動畫
            StartAnimation(winSprites);
        }
        else
        {
            // 答錯
            feedbackText.text = "系統: " + questions[currentQuestionIndex]["wrong_feedback"] + "\n提示: " + questions[currentQuestionIndex]["hint"];
            feedbackText.color = Color.red;

            // 答錯 -> 顯示「重新作答」和「下一題」
            retryButton.gameObject.SetActive(true);
            nextButton.gameObject.SetActive(true);

            // 播放答錯動畫
            StartAnimation(loseSprites);
        }
    }

    // 請綁定在「重新作答」按鈕的 OnClick
    public void OnRetry()
    {
        ShowQuestion(); // 重新顯示題目，清空輸入框
    }

    // 請綁定在「下一題」按鈕的 OnClick
    public void OnNext()
    {
        currentQuestionIndex++; // 索引 +1
        if (currentQuestionIndex < questions.Count)
        {
            ShowQuestion(); // 顯示下一題
        }
        else
        {
            // 沒題目了，關閉介面
            feedbackText.text = "測驗結束！";
            quizPanel.SetActive(false);
            StopAnimation(); // 確保動畫停止並恢復原狀
        }
    }

    // --- 動畫控制方法 ---

    void StartAnimation(Sprite[] clips)
    {
        if (characterRenderer == null) { Debug.LogError("【錯誤】找不到角色！請確認場景中有物件名為 '2提問者一號'"); return; }
        if (clips == null || clips.Length == 0) { Debug.LogError("【錯誤】沒有載入到動畫圖片！"); return; }

        StopAnimation(); // 先停止當前動畫，避免重疊
        animCoroutine = StartCoroutine(PlaySpriteLoop(clips));
    }

    void StopAnimation()
    {
        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
            animCoroutine = null;
        }

        // 恢復原始圖片
        if (characterRenderer != null && defaultSprite != null)
        {
            characterRenderer.sprite = defaultSprite;
        }
    }

    IEnumerator PlaySpriteLoop(Sprite[] clips)
    {
        int index = 0;
        while (true)
        {
            characterRenderer.sprite = clips[index];
            index = (index + 1) % clips.Length; // 循環播放
            yield return new WaitForSeconds(animSpeed);
        }
    }

    // --- 自動載入圖片與設定功能 (針對不會操作 Unity 介面的輔助) ---
    void LoadAutoSprites()
    {
        // 1. 嘗試自動尋找角色2 (假設物件名稱為 "2提問者一號")
        if (characterRenderer == null)
        {
            GameObject obj = GameObject.Find("2提問者一號");
            if (obj != null) 
                characterRenderer = obj.GetComponentInChildren<SpriteRenderer>(); // 改用 InChildren 比較保險
            
            if (characterRenderer == null) Debug.LogError("【錯誤】找不到 '2提問者一號' 或該物件沒有 SpriteRenderer！");
        }

        // 2. 直接從指定路徑讀取圖片並切割
        // 改進：優先使用 Application.dataPath (Assets 資料夾) 相對路徑，避免絕對路徑因電腦不同而失效
        string baseDir = Application.dataPath; 
        string winPath = Path.Combine(baseDir, "2提問者一號", "win", "all.png");
        string losePath = Path.Combine(baseDir, "2提問者一號", "lose", "all.png");

        // 備用：如果相對路徑找不到，再試試看原本的絕對路徑
        if (!File.Exists(winPath)) winPath = @"C:\Users\user\OneDrive\桌面\程式設計\5動畫期末\2提問者一號\win\all.png";
        if (!File.Exists(losePath)) losePath = @"C:\Users\user\OneDrive\桌面\程式設計\5動畫期末\2提問者一號\lose\all.png";

        if (File.Exists(winPath)) 
        {
            winSprites = LoadAndSliceSprite(winPath, 12);
        }
        else Debug.LogError("【嚴重錯誤】找不到 Win 圖片！請檢查路徑: " + winPath);

        if (File.Exists(losePath)) 
        {
            loseSprites = LoadAndSliceSprite(losePath, 13);
        }
        else Debug.LogError("【嚴重錯誤】找不到 Lose 圖片！請檢查路徑: " + losePath);
    }

    Sprite[] LoadAndSliceSprite(string path, int count)
    {
        byte[] data = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(data);
        tex.filterMode = FilterMode.Point; // 保持像素清晰

        Sprite[] result = new Sprite[count];
        float w = tex.width / (float)count;
        for (int i = 0; i < count; i++)
            result[i] = Sprite.Create(tex, new Rect(i * w, 0, w, tex.height), new Vector2(0.5f, 0.5f));
        return result;
    }
}