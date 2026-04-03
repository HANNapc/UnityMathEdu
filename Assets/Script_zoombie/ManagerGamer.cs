using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Extensions;
public class ManagerGamer : MonoBehaviour
{
    public static ManagerGamer Instance;

    public double score = 0;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI hiensao;
    public TextMeshProUGUI timeText;
    [SerializeField] private GameObject gameOverUI;
    private bool isOver = false;
    [SerializeField] private GameObject gameWinUI;
    private bool isWin = false;
    public Image star_zero;
    public Image star_one;
    public Image star_two;
    public Image star_three;
    public float timePerQuestion = 20f;
    private float timeRemaining;
    private bool isCounting = false;
    private string sessionId;
    private List<QuestionAnswer> sessionAnswers = new List<QuestionAnswer>();

    void Awake()
    {
        Instance = this;
        UpdateScoreUI();
        resultText.gameObject.SetActive(false);
        timeText.gameObject.SetActive(false);
        gameOverUI.SetActive(false);
        gameWinUI.SetActive(false);
        sessionId = System.Guid.NewGuid().ToString();
    }
    public void Start()
    {
        LoadCurrentStar();
    }
    void Update()
    {
        if (!isCounting) return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            int seconds = Mathf.CeilToInt(timeRemaining);
            timeText.text = "Thời gian: " + seconds;

            if (timeRemaining <= 5)
                timeText.color = Color.red;
            else
                timeText.color = Color.white;
        }
        else
        {
            timeRemaining = 0;
            timeText.text = "Thời gian: 0";
            isCounting = false;
            TimeUp();
        }
    }

    public void StartQuestion()
    {
        timeRemaining = timePerQuestion;
        isCounting = true;
        timeText.gameObject.SetActive(true);
        timeText.text = "Thời gian: " + Mathf.CeilToInt(timePerQuestion);
        resultText.gameObject.SetActive(false);
        timeText.color = Color.white;
    }

    public void StopTimer()
    {
        isCounting = false;
        timeText.gameObject.SetActive(false);
    }

    public void CorrectAnswer()
    {
        score += 0.5;
        UpdateScoreUI();
        ShowResult("Tuyệt vời! Bạn giỏi quá!", Color.green);
    }

    public void WrongAnswer()
    {
        ShowResult("Không sao! Cố lên nhé!", Color.red);
    }

    void TimeUp()
    {
        ShowResult("Hết giờ!", Color.yellow);
        UIManager.Instance.ForceWrongAnswer();
    }

    void UpdateScoreUI()
    {
        scoreText.text = "Điểm: " + score;
    }

    void ShowResult(string text, Color color)
    {
        resultText.gameObject.SetActive(true);
        resultText.text = text;
        resultText.color = color;
        CancelInvoke();
        Invoke(nameof(HideResult), 1.5f);
    }
    public void GameOver()
    {
        if(score < 7)
        {
            isOver = true;
            gameOverUI.SetActive(true);
        }
        else
        {
            isWin = true;
            gameWinUI.SetActive(true);
        }
        setstar();
    }
    public void setstar()
    {
        star_zero.gameObject.SetActive(false);
        star_one.gameObject.SetActive(false);
        star_two.gameObject.SetActive(false);
        star_three.gameObject.SetActive(false);

        if (score >= 9)
        {
            star_three.gameObject.SetActive(true);
        }
        else if (score >= 7)
        {
            star_two.gameObject.SetActive(true);
        }
        else if (score >= 6)
        {
            star_one.gameObject.SetActive(true);
        }
        else
        {
            star_zero.gameObject.SetActive(true);
        }
    }

    void HideResult()
    {
        resultText.gameObject.SetActive(false);
    }

    // =========================
    // LƯU CÂU TRẢ LỜI MỖI CÂU
    // =========================
    public void AddAnswerToSession(
        string question,
        string selectedAnswer,
        string correctAnswer,
        bool isCorrect,
        string explanation = "",
        string imageUrl = "",
        int starEarned = 0
    )
    {
        // Nếu câu này đã tồn tại thì xóa bản cũ trước

        sessionAnswers.Add(new QuestionAnswer
        {
            Question = question,
            SelectedAnswer = selectedAnswer,
            CorrectAnswer = correctAnswer,
            IsCorrect = isCorrect,
            Explanation = explanation,
            ImageUrl = imageUrl
        });

       
    }

    private int CalculateStar(double score)
    {
        if (score >= 9)
            return 3;
        else if (score >= 7)
            return 2;
        else if (score >= 6)
            return 1;
        else
            return 0;
    }

    public void EndSession(int lessonId, float timeSpentSeconds)
    {
        int starEarned = CalculateStar(score);

        Debug.Log("Số câu đã lưu trước khi upload: " + sessionAnswers.Count);

        SaveLearningHistory(lessonId, timeSpentSeconds, starEarned);
        if (starEarned >= 2)
        {
            UpdateCompletedLevel(lessonId);
        }
        sessionAnswers.Clear();
        score = 0;
        UpdateScoreUI();
    }

    private void SaveLearningHistory(int lessonId, float timeSpentSeconds, int starEarnedTotal)
    {
        string userId = AuthManager.Instance.currentUserID;
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("Chưa login học sinh!");
            return;
        }

        string lessonDocId = "Lesson" + lessonId;

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference studentRef = db.Collection("Students").Document(userId);
        DocumentReference lessonRef = studentRef.Collection("LearningHistory").Document(lessonDocId);

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "StudentID", userId },
            { "LessonID", lessonId },
            { "score", score },
            { "StarEarned", starEarnedTotal },

            // nếu muốn lưu thời gian làm bài thật thì dùng dòng dưới
            { "timeSpentSeconds", timeSpentSeconds },

            // thời điểm nộp bài
            { "submittedAt", Timestamp.FromDateTime(System.DateTime.UtcNow) },

            // session id để phân biệt mỗi lần làm
            { "sessionId", sessionId }
        };

        Dictionary<string, object> answersData = new Dictionary<string, object>();

        foreach (var qa in sessionAnswers)
        {
         

            answersData[qa.Question] = new Dictionary<string, object>
            {
                { "Question", qa.Question ?? "" },
                { "SelectedAnswer", qa.SelectedAnswer ?? "" },
                { "CorrectAnswer", qa.CorrectAnswer ?? "" },
                { "IsCorrect", qa.IsCorrect },
                { "Explanation", qa.Explanation ?? "" },
                { "ImageUrl", qa.ImageUrl ?? "" },
            };
        }

        data["answers"] = answersData;

        lessonRef.SetAsync(data).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("LearningHistory uploaded successfully!");
            }
            else
            {
                Debug.LogError("Error uploading LearningHistory: " + task.Exception);
            }
        });
    }
    // update qua màn
    private void UpdateCompletedLevel(int lessonId)
    {
        string userId = AuthManager.Instance.currentUserID;
        int starEarned = CalculateStar(score);

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("Chưa login học sinh!");
            return;
        }

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference studentRef = db.Collection("Students").Document(userId);
        DocumentReference lessonStarRef = studentRef.Collection("LessonStars").Document("Lesson" + lessonId);

        db.RunTransactionAsync(transaction =>
        {
            return transaction.GetSnapshotAsync(lessonStarRef).ContinueWith(task =>
            {
                DocumentSnapshot snapshot = task.Result;

                int oldStar = 0;

                if (snapshot.Exists && snapshot.ContainsField("Star"))
                {
                    oldStar = snapshot.GetValue<int>("Star");
                }

                // 🔥 chỉ update nếu star mới cao hơn
                if (starEarned > oldStar)
                {
                    int diff = starEarned - oldStar;
                    // cộng thêm vào totalStar
                    transaction.Update(studentRef, new Dictionary<string, object>
                {
                    { "CurrentStar", FieldValue.Increment(diff) },
                    { "completedLevel", lessonId }
                });
                }
            });
        }).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Update star OK!");
                LoadCurrentStar();
            }
            else
            {
                Debug.LogError("Error: " + task.Exception);
            }
        });
    }
    void LoadCurrentStar()
    {
        string userId = PlayerPrefs.GetString("student_uid");
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("Chưa login!");
            return;
        }

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference studentRef = db.Collection("Students").Document(userId);

        studentRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;

                if (snapshot.Exists && snapshot.ContainsField("CurrentStar"))
                {
                    int currentStar = snapshot.GetValue<int>("CurrentStar");

                    Debug.Log("Current Star: " + currentStar);

                    // 👉 HIỂN THỊ LÊN UI
                    hiensao.text = currentStar.ToString();
                }
                else
                {
                    hiensao.text = "0";
                }
            }
            else
            {
                Debug.LogError("Lỗi load star: " + task.Exception);
            }
        });
    }

}