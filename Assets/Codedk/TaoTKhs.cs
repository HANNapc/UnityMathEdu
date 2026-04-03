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

public class TaoTKhs : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField passwordInput;
    public TMP_Dropdown dropdown;

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    public TextMeshProUGUI messageText;

    string lop;
    bool isPasswordHidden = true;
    private NotiManager notiManager;
    //tim thong baao
    void Start()
    {
        notiManager = FindObjectOfType<NotiManager>();
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
    //kiem tra firebase
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
    //dang ky
    public void Onclickdk()
    {
        string name = nameInput.text;
        string password = passwordInput.text;
        string lop = dropdown.options[dropdown.value].text;

        //kiem tra dwu lieu
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
        {
            Debug.Log(string.Format("Vui lòng nhập đầy đủ dữ liệu"));
            ShowMessage("Vui lòng nhập đầy đủ dữ liệu");
            return;
        }
        else if (!IsValidPassword(password))
        {
            Debug.Log(passwordInput.text);
            Debug.Log("Mật khẩu phải ≥ 6 ký tự và có chữ + số");
            ShowMessage("Mật khẩu phải ≥ 6 ký tự và có chữ + số");
            return;
        }
        else if (string.IsNullOrEmpty(lop))
        {
                ShowMessage("Vui lòng chọn lớp");
                return;
        }
        else
        {

            CheckUsernameExists(name);
        }
    }
    //luu vao firestore
     void SaveUserToFirestore()
     {
         if (auth.CurrentUser == null)
         {
             ShowMessage("Chưa đăng nhập phụ huynh!");
             return;
         }
         //lay id phu huynh
         string parentId = auth.CurrentUser.UserId;
         //lay du lieu
         string name = nameInput.text;
         string password = passwordInput.text;
         string lop = dropdown.options[dropdown.value].text;
         string studentID = "Student " + name;

        //ma hoa
        string passwordHash = HashPassword(password);

         Dictionary<string, object> student= new Dictionary<string, object>()
         {
             { "studentID",studentID},
             { "name", name },
             { "password", passwordHash},
             {"lop",lop},
             { "uidphuhuynh", parentId },
             {"CurrentStar",0 },
             {"CompletedLessonCount",0 },
             {"Status" ,"Đang hoạt động" },
             {"avatar","avatar1" },
             {"role","student" }
         };

         db.Collection("Students").Document(studentID).SetAsync(student).ContinueWithOnMainThread(task =>
         {
             if (task.IsCompleted)
             {
                 Debug.Log("Lưu Firestore thành công");
                 ShowMessage("Đăng ký thành công.");
                 notiManager.Send(parentId,"Tạo tài khoản.", "Bạn đã tạo tài khoản thành công cho " + name+ ".", name);
                 Debug.Log("Tao thong bao thanh cong");

             }
             else
             {
                 Debug.LogError("Lỗi lưu Firestore");
                 ShowMessage("Đăng ký lỗi.Vui lòng thử lại sau.");
             }
         });
     }
    //check trùng tên
    void CheckUsernameExists(string name)
    {
        db.Collection("Students")
          .WhereEqualTo("name", name)
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsFaulted)
              {
                  ShowMessage("Lỗi kiểm tra dữ liệu");
                  return;
              }

              var snapshot = task.Result;

              if (snapshot.Count > 0)
              {
                  // đã tồn tại
                  ShowMessage("Tên đã tồn tại, vui lòng chọn tên khác");
              }
              else
              {
                  //chưa tồn tại 
                  SaveUserToFirestore();
              }
          });
    }
    //quay lai
    public void GoBack()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
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
  
    //ma hóa mật khẩu theo SHA 256
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
