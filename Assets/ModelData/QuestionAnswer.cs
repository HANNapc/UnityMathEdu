using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionAnswer : MonoBehaviour
{
    public string Question;      // Nội dung câu hỏi
    public List<string> Answers; // Các đáp án
    public string SelectedAnswer;
    public string CorrectAnswer;
    public bool IsCorrect;
    public string Explanation;
    public int StarEarned;
    public string ImageUrl;
}
