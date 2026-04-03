using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

public class LevelMenuFirestore : MonoBehaviour
{
    public LessonItemUI[] lessonItems; // kéo 9 item vào Inspector
    public string userId;

    private FirebaseFirestore db;

    private int completedLevel = 0;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        userId = PlayerPrefs.GetString("student_uid");
        LoadUserProgress();
    }

    void LoadUserProgress()
    {
        DocumentReference userRef = db
            .Collection("Students")
            .Document(userId);

        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DocumentSnapshot doc = task.Result;

                if (doc.ContainsField("completedLevel"))
                {
                    completedLevel = doc.GetValue<int>("completedLevel");
                }

                Debug.Log("Completed Level: " + completedLevel);
                LoadLessons();
            }
            else
            {
                Debug.LogError("Không tìm thấy user!");
            }
        });
    }

    void LoadLessons()
    {
        Debug.Log("🔥 LoadLessons CALLED");

        CollectionReference lessonsRef = db
            .Collection("Students")
            .Document(userId)
            .Collection("LearningHistory");

        lessonsRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("❌ Firebase LoadLessons lỗi: " + task.Exception);
                return;
            }

            if (!task.IsCompleted)
            {
                Debug.LogError("❌ Task chưa complete");
                return;
            }

            Debug.Log("✅ LoadLessons SUCCESS");

            QuerySnapshot snapshot = task.Result;

            // Reset UI trước
            for (int i = 0; i < lessonItems.Length; i++)
            {
                int lessonNumber = i + 1;
                bool unlocked = lessonNumber <= completedLevel + 1;

                lessonItems[i].Setup(lessonNumber, unlocked, 0);
                int capturedLesson = lessonNumber;
                lessonItems[i].SetClick(() => OpenLesson(capturedLesson));
            }

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                string docId = doc.Id;
                int lessonID = int.Parse(docId.Replace("Lesson", ""));
                int stars = doc.GetValue<int>("StarEarned");
                Debug.Log($"Lesson {lessonID} - Stars: {stars}");

                if (lessonID - 1 < lessonItems.Length)
                {
                    bool unlocked = lessonID <= completedLevel + 1;

                    lessonItems[lessonID - 1].Setup(lessonID, unlocked, stars);
                    int capturedLesson = lessonID;
                    lessonItems[lessonID - 1].SetClick(() => OpenLesson(capturedLesson));
                }

            }
        });
    }
    void OpenLesson(int lessonNumber)
    {
        GameData.currentLessonID = lessonNumber;
        string sceneName = "Lesson" + lessonNumber;
        Debug.Log("CLICK lesson: " + lessonNumber);
        SceneManager.LoadScene("SelectGame");
    }
}