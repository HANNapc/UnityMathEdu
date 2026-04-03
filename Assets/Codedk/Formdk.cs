using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using Firebase.Firestore;
using Firebase.Extensions;

public class Formdk : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    private FirebaseAuth auth;
    private FirebaseFirestore db;

    public TextMeshProUGUI messageText;
    bool isPasswordHidden = true;
   
    //bat tat mat khau

    public void TogglePassword()
    {
        if (isPasswordHidden)
        {
            passwordInput.contentType = TMP_InputField.ContentType.Standard;
            isPasswordHidden = false;
        }
        else
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            isPasswordHidden = true;
        }

        passwordInput.ForceLabelUpdate(); // cập nhật UI
    }

    void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firebase OK");
            }
            else
            {
                Debug.LogError("Firebase lỗi");
            }
        });
    }

    public void Onclickdk()
    {
        string email=emailInput.text;
        string password=passwordInput.text;
        string hoten=nameInput.text;

        //kiem tra dwu lieu
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hoten))
        {
            Debug.Log(string.Format("Vui lòng nhập đầy đủ dữ liệu"));
            ShowMessage("Vui lòng nhập đầy đủ dữ liệu");
            return;
        }
        else if (!IsValidEmail(email))
        {
            Debug.Log("Email không hợp lệ");
            ShowMessage("Email không hợp lệ");
            return;
        }
        else if (!IsValidPassword(password))
        {
            Debug.Log("Mật khẩu phải ≥ 6 ký tự và có chữ + số");
            ShowMessage("Mật khẩu phải ≥ 6 ký tự và có chữ + số");
            return;
        }
        else
        {
            auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("Đăng ký lỗi: " + task.Exception);
                    ShowMessage("Đăng ký lỗi. Vui lòng thử lại sau.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("Đăng ký lỗi: " + task.Exception);
                    ShowMessage("Đăng ký lỗi. Vui lòng thử lại sau.");
                    return;
                }

                FirebaseUser newUser = task.Result.User;

                SaveUserToFirestore(newUser.UserId, hoten, email);

                Debug.Log("Đăng ký thành công");
                ShowMessage("Đăng ký thành công");
                //chuyen sang login 
                SceneManager.LoadScene("Formlogin");
            });
        }
    }
    //luu vao firestore
    void SaveUserToFirestore(string uid, string name, string email)
    {
        Dictionary<string, object> user = new Dictionary<string, object>()
        {
            { "uid", uid },
            { "name", name },
            { "email", email },
            { "avatar", 0 },
            { "role", "Parent" }
        };

        db.Collection("users").Document(uid).SetAsync(user).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Lưu Firestore thành công");
            }
            else
            {
                Debug.LogError("Lỗi lưu Firestore");
                ShowMessage("Đăng ký lỗi.Vui lòng thử lại sau.");
            }
        });
    }
    //click button dăng nhập 
    public void Gotologin()
    {
        SceneManager.LoadScene("Formlogin");
    }
    //kiem tra email

    bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }
    //kiem tra mat khau
    bool IsValidPassword(string password)
    {
        if (password.Length < 6)
            return false;

        bool hasLetter = false;
        bool hasDigit = false;

        foreach (char c in password)
        {
            if (char.IsLetter(c)) hasLetter = true;
            if (char.IsDigit(c)) hasDigit = true;
        }

        return hasLetter && hasDigit;
    }
  
    //them thong bao
    void ShowMessage(string msg)
    {
        messageText.text = msg;
    }


}
