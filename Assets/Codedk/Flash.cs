using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using Firebase.Firestore;
using System.Security.Cryptography;
using System.Text;
using Firebase.Extensions;
public class Flash : MonoBehaviour
{
    public string nextSceneName = "Select"; // tên scene chính

    void Start()
    {
        var parent = FirebaseAuth.DefaultInstance.CurrentUser;

        //Nếu là phụ huynh
        if (parent != null)
        {
            Debug.Log("Auto login phụ huynh");
            SceneManager.LoadScene("HomePH");
            return;
        }

        // Nếu là học sinh
        if (PlayerPrefs.GetInt("isStudent", 0) == 1)
        {
            string uid = PlayerPrefs.GetString("student_uid");
            string lop = PlayerPrefs.GetString("student_class");
            if (!string.IsNullOrEmpty(uid))
            {
                Debug.Log("Auto login học sinh");
                Debug.Log("stdin" + uid);
                Debug.Log("stdin" + lop);
                SceneManager.LoadScene("Menu");
                return;
            }
        }

        //  Chưa đăng nhập
        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(2f); // đợi 2 giây
        SceneManager.LoadScene(nextSceneName);
    }
    
}
