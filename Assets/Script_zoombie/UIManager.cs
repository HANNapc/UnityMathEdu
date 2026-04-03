using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject panel;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;
    public Image questionImage;
    [SerializeField] private GameObject UIStop;
    private DataQuestion currentQ;
    private GraveTrigger currentGrave;
    private PlayerController currentPlayer;
    public Button[] buttonUI;
    public void Start()
    {
        UIStop.SetActive(false);
        clickButton();
    }
    void Awake()
    {
        Instance = this;
        if (panel != null)
            panel.SetActive(false);
    }

    // ?? G?i coroutine thay vì ch?y tr?c ti?p
    public void ShowQuestion(DataQuestion q, PlayerController player, GraveTrigger grave)
    {
        StartCoroutine(ShowQuestionRoutine(q, player, grave));
    }

    IEnumerator ShowQuestionRoutine(DataQuestion q, PlayerController player, GraveTrigger grave)
    {
        if (q == null) yield break;

        currentQ = q;
        currentGrave = grave;
        currentPlayer = player;

        // ? ?n panel tr??c ?? tránh hi?n l?ch
        panel.SetActive(false);
        // set câu h?i
        if (questionText != null)
            questionText.text = q.question;

        // set answers
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;

            if (q.answers != null && i < q.answers.Count)
            {
                answerButtons[i].gameObject.SetActive(true);

                var txt = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                    txt.text = q.answers[i];

                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnAnswer(index));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }

        // load ?nh tr??c khi show panel
        if (questionImage != null && !string.IsNullOrEmpty(q.imageUrl))
        {
            yield return StartCoroutine(LoadImage(q.imageUrl));
        }
        else
        {
            if (questionImage != null)
                questionImage.gameObject.SetActive(false);
        }

        // ? Hi?n t?t c? cùng lúc
        panel.SetActive(true);
        ManagerGamer.Instance.StartQuestion();
    }

    void OnAnswer(int index)
    {
        if (currentQ == null) return;

        panel.SetActive(false);
        ManagerGamer.Instance.StopTimer();

        string selectedAnswer = currentQ.answers[index];
        string correctAnswer = currentQ.answers[currentQ.correctIndex];
        bool isCorrect = (index == currentQ.correctIndex);

        // LƯU CÂU TRẢ LỜI VÀO SESSION
        ManagerGamer.Instance.AddAnswerToSession(
            currentQ.question,            // nội dung câu hỏi
            selectedAnswer,               // đáp án học sinh chọn
            correctAnswer,                // đáp án đúng
            isCorrect,                    // đúng/sai
            currentQ.explain,         // giải thích
            currentQ.imageUrl,            // ảnh nếu có
            isCorrect ? 1 : 0             // sao câu này
        );

        if (isCorrect)
        {
            ManagerGamer.Instance.CorrectAnswer();
        }
        else
        {
            ManagerGamer.Instance.WrongAnswer();
        }

        if (currentGrave != null && currentPlayer != null)
        {
            currentGrave.OnAnswered(currentPlayer);
        }

        currentQ = null;
        currentGrave = null;
        currentPlayer = null;
    }

    IEnumerator LoadImage(string url)
    {
        if (questionImage == null) yield break;
        questionImage.sprite = null;
        questionImage.gameObject.SetActive(false);

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D tex = DownloadHandlerTexture.GetContent(request);

            questionImage.sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)
            );

            questionImage.gameObject.SetActive(true);
        }
        else
        {
            questionImage.gameObject.SetActive(false);
        }
    }
    public void ForceWrongAnswer()
    {
        panel.SetActive(false);

        if (currentGrave != null && currentPlayer != null)
        {
            currentGrave.OnAnswered(currentPlayer);
        }
    }
    public void clickButton()
    {
        for (int i = 0; i < buttonUI.Length; i++)
        {
            int index = i;
            Debug.Log("chay thanh cong");
            buttonUI[i].onClick.AddListener(() => OpenUi(index));
        }
    }
    public void OpenUi(int index)
    {
        if (index == 0)
        {
            UIStop.SetActive(true);
            Time.timeScale = 0f;

        }
        else if (index == 1)
        {
            UIStop.SetActive(false);
            Time.timeScale = 1f;

        }
        else if(index == 2)
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
