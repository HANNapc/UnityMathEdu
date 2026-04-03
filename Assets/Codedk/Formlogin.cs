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
using System.Linq;

public class Formlogin : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    public TextMeshProUGUI messageText;
    bool isFirebaseReady = false;
    bool isPasswordHidden = true;
    //doc
    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
    }

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
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;

                isFirebaseReady = true;

                Debug.Log("Firebase OK");
            }
            else
            {
                Debug.LogError("Firebase lỗi");
            }
        });
    }
    public void OnClickLogin()
    {
        string email = emailInput.text;
        string password = passwordInput.text;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.Log(string.Format("Vui lòng nhập đầy đủ dữ liệu"));
            ShowMessage("Vui lòng nhập đầy đủ dữ liệu");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
           
             if (task.Exception != null)
            {

                //kiem tra loi
                FirebaseException firebaseEx = task.Exception.GetBaseException() as FirebaseException;

                if (firebaseEx != null)
                {
                    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                    switch (errorCode)
                    {
                        case AuthError.WrongPassword:
                            ShowMessage("Sai mật khẩu hoặc tên đăng nhập.");
                            passwordInput.text = "";
                            return;
                        case AuthError.UserNotFound:
                        case AuthError.InvalidEmail:
                            LoginStudents(email, password);
                            return;

                        default:
                            ShowMessage("Lỗi đăng nhập: " + errorCode);
                            Debug.Log(task.Exception);
                            return;
                    }
                }

            }
            else
            {
                Firebase.Auth.AuthResult result = task.Result;
                Debug.LogFormat("Đăng nhập thành công.",
                    result.User.DisplayName, result.User.UserId);
                ShowMessage("Đăng nhập thành công.");
                //chuyen sang home 
                SceneManager.LoadScene("HomePH");
            }

        });
    }
    public void Gotodk()
    {
        SceneManager.LoadScene("Formdk");
    }
    //them thong bao
    void ShowMessage(string msg)
    {
        messageText.text = msg;
    }
    //firestore hs
    void LoginStudents(string username, string password)
    {
        string passwordHash = HashPassword(password);
        db.Collection("Students")
          .WhereEqualTo("name", username)
          .WhereEqualTo("password", passwordHash)
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsFaulted)
              {
                  ShowMessage("Lỗi kiểm tra dữ liệu");
                  return;
              }

              QuerySnapshot snapshot = task.Result;

              if (snapshot.Count > 0)
              {
                  // đã tồn tại
                  ShowMessage("Đăng nhập thành công.");
                  //lưu sesion sau khi đăng nhập thành công 
                  var doc = snapshot.Documents.First();
                  string lop = doc.ContainsField("lop") ? doc.GetValue<string>("lop") : "Chưa có lớp";
                  Debug.Log("Tên " + lop);
                  SaveStudentSession(doc.Id, username, lop);
                  SceneManager.LoadScene("Menu");

              }
              else
              {
                  ShowMessage("Sai tài khoản hoặc mật khẩu.");
                  return;
              }
          });
    }
    //lưu session học sinh 
    void SaveStudentSession(string uid, string name,string lop)
    {
        PlayerPrefs.SetString("student_uid", uid);
        PlayerPrefs.SetString("student_name", name);
        PlayerPrefs.SetString("student_class", lop);
        PlayerPrefs.SetInt("isStudent", 1);
        PlayerPrefs.Save(); // lưu ngay
        Debug.Log("Đã lưu session học sinh");
    }
    //hash mat khau
    string HashPassword(string password)
    {
        SHA256 sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        StringBuilder builder = new StringBuilder();
        foreach (byte b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }



}
