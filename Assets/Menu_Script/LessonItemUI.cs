using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LessonItemUI : MonoBehaviour
{
    public Button lessonButton;
    public GameObject lockIcon;
    public Image[] stars;
    public TextMeshProUGUI lessonNumberText;

    public Sprite starOn;
    public Sprite starOff;

    private int lessonIndex;

    public void Setup(int lessonNumber, bool unlocked, int starCount)
    {
        lessonIndex = lessonNumber;

        if (lessonNumberText != null)
            lessonNumberText.text = lessonNumber.ToString();

        if (lessonButton != null)
            lessonButton.interactable = unlocked;

        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);

        starCount = Mathf.Clamp(starCount, 0, 3);
        UpdateStars(starCount);
    }

    void UpdateStars(int count)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                stars[i].sprite = (i < count) ? starOn : starOff;
            }
        }
    }
    public void SetClick(System.Action action) // ✅ THÊM HÀM NÀY
    {
        if (lessonButton == null) return;

        lessonButton.onClick.RemoveAllListeners();
        lessonButton.onClick.AddListener(() => action());
    }
}