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
using System;


public class capnhat : MonoBehaviour
{
    public GameObject paneltt;
    public GameObject panelavatar;
    public TMP_InputField inputTen;
    public TMP_InputField inputEmail;

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    public TextMeshProUGUI messageText;//message

    //anh
    public string avatarID; // ví dụ: "avatar1"
    public Image avatarHienTai;
    public string avatarDaChon;


    void Start()
    {
        paneltt.SetActive(true);
        panelavatar.SetActive(false);
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        LoadAvatar();
        LoadUserInfo();//load thong tin 
    }

    public void ShowChange()
    {
        panelavatar.SetActive(true);
        paneltt.SetActive(false);
        Debug.Log("paneltt: " + paneltt);
        Debug.Log("panelavatar: " + panelavatar);
    }
    //quay lai 
    public void ql()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
    //load infor
    void LoadUserInfo()
    {
            FirebaseUser user = auth.CurrentUser;

            if (user == null)
            {
                Debug.Log("Chưa đăng nhập");
                return;
            }


            //  Lấy tên từ Firestore
            db.Collection("users").Document(user.UserId)
                .GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        var snap = task.Result;

                        if (snap.Exists && snap.ContainsField("name"))
                        {
                            string ten = snap.GetValue<string>("name");
                            string email = snap.GetValue<string>("email");
                            inputTen.text = ten;
                            inputEmail.text = email;
                        }
                        else
                        {
                            Debug.Log("Không có field name");
                        }
                    }
                    else
                    {
                        Debug.LogError("Lỗi load user: " + task.Exception);
                    }
                });
        
    }
    //cap nhat
    public void CapNhatThongTin()
    {
        
            Debug.Log("Đã update ");
            Firebase.Auth.FirebaseUser user = auth.CurrentUser;//lay nguoi dung hien tai

            if (user == null)
            {
                Debug.LogError("Chưa đăng nhập!");
                return;
            }

            string uid = user.UserId;
            string ten = inputTen.text;
            string email = inputEmail.text;

            Debug.Log("Đã xác nhận cập nhật");
            //  Cập nhật Firestore
            Dictionary<string, object> data = new Dictionary<string, object>
                {
                    { "name", ten },
                    { "email", email },
                };

            db.Collection("users").Document(uid).UpdateAsync(data).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Cập nhật Firestore thành công!");
                    ShowMessage("Cập nhật thành công!");
                }
                else
                {
                    Debug.LogError("Lỗi Firestore: " + task.Exception);
                    ShowMessage("Cập nhật không thành công!");
                }
            });

 
    }
    public void ChonAvatar(string id, Sprite sprite)
    {
        avatarDaChon = id;
        avatarHienTai.sprite = sprite;

        Debug.Log("Đã chọn avatar: " + id);

        LuuAvatar(); // 🔥 thêm dòng này để auto lưu
    }
    //luu vao firebase
    public void LuuAvatar()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        if (user == null) return;
        if (string.IsNullOrEmpty(avatarDaChon))
        {
            Debug.Log("Chưa chọn avatar");
            return;
        }

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "avatar", avatarDaChon }
        };

        db.Collection("users").Document(user.UserId)
            .UpdateAsync(data)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Lưu avatar thành công!");
                    ShowMessage("Thay đổi avatar thành công");
                }
                else
                {
                    Debug.LogError(task.Exception);
                    ShowMessage("Thay đổi avatar không thành công");
                }
                    
            });
    }
    //load anh tu firebase
    public void LoadAvatar()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.Log("Chưa đăng nhập");
            return;
        }

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        db.Collection("users").Document(user.UserId)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    var snap = task.Result;

                    if (snap.Exists && snap.ContainsField("avatar"))
                    {
                        avatarID = snap.GetValue<string>("avatar");

                        Sprite avatar = Resources.Load<Sprite>("Avatar/" + avatarID);
                        Debug.Log("Đang load avatar: Avatar/" + avatarID);

                        if (avatar != null)
                        {
                            avatarHienTai.sprite = avatar;
                            avatarDaChon = avatarID;
                        }
                        else
                        {
                            Debug.LogError("Không tìm thấy avatar: " + avatarID);
                        }
                    }
                }
            });
    }
    //thong bao
    void ShowMessage(string msg)
    {
        messageText.text = msg;
    }
}
