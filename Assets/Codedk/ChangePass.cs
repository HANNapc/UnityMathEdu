using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using UnityEngine.UI;

public class ChangePass : MonoBehaviour
{
    public TMP_InputField inputpass;
    public TMP_InputField inputpassxn;
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    public TextMeshProUGUI messageText;//message
    bool isPasswordHidden = true;
    private bool isPassHidden = true;
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }
    //quay lai
    public void ql()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
    //bat tat mat khau

    public void TogglePassword()
    {
        if (isPasswordHidden)
        {
            inputpass.contentType = TMP_InputField.ContentType.Standard;
            isPasswordHidden = false;
        }
        else
        {
            inputpass.contentType = TMP_InputField.ContentType.Password;
            isPasswordHidden = true;
        }

        inputpass.ForceLabelUpdate(); // cập nhật UI
    }

    //bat tat mat khau xac nhan

    public void TogglePasswordxn()
    {
        if (isPassHidden)
        {
            inputpassxn.contentType = TMP_InputField.ContentType.Standard;
            isPassHidden = false;
        }
        else
        {
            inputpassxn.contentType = TMP_InputField.ContentType.Password;
            isPassHidden = true;
        }

        inputpassxn.ForceLabelUpdate(); // cập nhật UI
    }

    public void Check()
    {
        if (auth == null)
        {
            Debug.LogError("chua Login");
            return;
        }
        if (inputpass == null)
        {
            ShowMessage("Vui lòng nhập đầy đủ dữ liệu.");
            return;
        }
        else if (!IsValidPassword(inputpass.text))
        {
            Debug.Log("Mật khẩu phải ≥ 6 ký tự và có chữ + số");
            ShowMessage("Mật khẩu phải ≥ 6 ký tự và có chữ + số");
            return;
        }
        else if (inputpassxn.text != inputpass.text) {

            Debug.Log("Mật khẩu xác nhận không đồng nhất.");
            ShowMessage("Mật khẩu xác nhận không đồng nhất.");
            return;
        }
        else
        {
            Upadatepass();
        }
    }

    public void Upadatepass()
    {
        Firebase.Auth.FirebaseUser user = auth.CurrentUser;
        string newPassword = inputpass.text;
        if (user != null)
        {
            user.UpdatePasswordAsync(newPassword).ContinueWithOnMainThread(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("Đổi mật khẩu không thành công.Vui lòng thử lại sau.");
                    ShowMessage("Đổi mật khẩu không thành công.Vui lòng thử lại sau.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdatePasswordAsync encountered an error: " + task.Exception);
                    ShowMessage("Đổi mật khẩu không thành công.Vui lòng thử lại sau.");
                    return;
                }

                Debug.Log("Đổi mật khẩu thành công.");
                ShowMessage("Đổi mật khẩu thành công.");
            });
        }
    }
    //thong bao
    void ShowMessage(string msg)
    {
        messageText.text = msg;
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
    
}
