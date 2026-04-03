using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    private FirebaseFirestore db;
    public List<DataQuestion> questions = new List<DataQuestion>();
    private List<int> shuffledIndexes = new List<int>();
    private int currentIndex = 0;
    public bool isLoaded = false;
    string useridlop;
    public System.Action OnDataLoaded;

    void Awake()
    {
        Instance = this;
        db = FirebaseFirestore.DefaultInstance;
        useridlop = PlayerPrefs.GetString("student_class");
        StartCoroutine(LoadQuestions());
    }
    string GetStringSafe(DocumentSnapshot doc, string field)
    {
        if (!doc.ContainsField(field)) return "";

        object value = doc.GetValue<object>(field);

        return value != null ? value.ToString() : "";
    }
    public string GetClassID(string currentClass)
    {
        if (string.IsNullOrEmpty(currentClass))
            return null;
        return currentClass.Replace("Lớp ", ""); // "Lớp 1" → "1"
    }
    IEnumerator LoadQuestions()
    {
        Debug.Log("🚀 Bắt đầu gọi Firebase...");
        Debug.Log("Class" + useridlop);
        string lop = GetClassID(useridlop);
        if (string.IsNullOrEmpty(lop))
        {
            Debug.LogError("❌ currentClass bị null!");
            yield break;
        }
        Debug.LogError("🔥 Innerlop: " + lop);// 🔥 lớp
        int lessonID = GameData.currentLessonID;
        Debug.LogError("🔥 Innerlesson: " + lessonID);
        var task = db.Collection("classes")
                     .Document(lop)
                     .Collection("Lessons")
                     .Document("Lesson" + lessonID)
                     .Collection("Questions")
                     .GetSnapshotAsync();

        yield return new WaitUntil(() => task.IsCompleted);

        Debug.Log("📡 Firebase trả về");
        if (task.Exception != null)
        {
            Debug.LogError("🔥 Firebase lỗi: " + task.Exception);
            isLoaded = true;
            yield break;
        }

        questions.Clear();

        int count = 0;

        foreach (var doc in task.Result.Documents)
        {
            DataQuestion q = new DataQuestion();
            if (!doc.ContainsField("Content")) continue;
            q.question = doc.GetValue<string>("Content");
            try
            {
                q.answers = new List<string>()
                {
                    GetStringSafe(doc, "OptionA"),
                    GetStringSafe(doc, "OptionB"),
                    GetStringSafe(doc, "OptionC"),
                    GetStringSafe(doc, "OptionD")
                };
            }
            catch
            {
                Debug.LogError("❌ Thiếu đáp án ở doc: " + doc.Id);
                continue;
            }
            string correct = doc.GetValue<string>("CorrectAnswer");
            q.correctIndex = correct switch
            {
                "A" => 0,
                "B" => 1,
                "C" => 2,
                "D" => 3,
                _ => 0
            };
            if (doc.ContainsField("ExplainAnswer"))
                q.explain = doc.GetValue<string>("ExplainAnswer");
            if (doc.ContainsField("Image"))
                q.imageUrl = doc.GetValue<string>("Image");
            else
                q.imageUrl = "";

            questions.Add(q);
            count++;
        }

        isLoaded = true;
        Debug.Log("isLoaded: " + isLoaded);
        Debug.Log("questions null? " + (questions == null));
        Debug.Log("Số câu hỏi: " + (questions == null ? 0 : questions.Count));
        OnDataLoaded?.Invoke();
    }
    void ShuffleQuestions()
    {
        shuffledIndexes.Clear();

        for (int i = 0; i < questions.Count; i++)
        {
            shuffledIndexes.Add(i);
        }

        // Fisher-Yates shuffle
        for (int i = 0; i < shuffledIndexes.Count; i++)
        {
            int rand = Random.Range(i, shuffledIndexes.Count);
            int temp = shuffledIndexes[i];
            shuffledIndexes[i] = shuffledIndexes[rand];
            shuffledIndexes[rand] = temp;
        }

        currentIndex = 0;
    }

    public DataQuestion GetRandomQuestion()
    {
        if (!isLoaded || questions.Count == 0)
        {
            Debug.LogWarning("⚠️ Chưa có câu hỏi!");
            return null;
        }

        if (shuffledIndexes.Count == 0 || currentIndex >= shuffledIndexes.Count)
        {
            ShuffleQuestions(); // 🔥 shuffle lại khi hết
        }

        int index = shuffledIndexes[currentIndex];
        currentIndex++;

        return questions[index];
    }
}