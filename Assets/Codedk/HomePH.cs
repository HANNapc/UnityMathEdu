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


public class HomePH : MonoBehaviour
{
    public TMP_Text inputTen;
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    public Image avatarHienTai;

    public GameObject panelmenu;
    public GameObject panelNoti;
    //thong bao
    public Transform content;
    public GameObject itemPrefab;


    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        panelmenu.SetActive(false);
        panelNoti.SetActive(false);
        LoadAvatar();
        LoadUserInfo();
    }
    //menu
    public void Clickmenu()
    {
        panelmenu.SetActive(true);
    }
    //tắt panel

    public void ClosePanel()
    {
        panelmenu.SetActive(false);
    }
    //clck thong bao
    public void ClickNotification()
    {
        panelmenu.SetActive(false);
        panelNoti.SetActive(true);
        string uidParent = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        LoadNotifications(uidParent);
    }
    //load thong bao 
    void LoadNotifications(string uid)
    {
        db.Collection("Notifications")
        .WhereEqualTo("uidParent", uid)            
        .OrderByDescending("time")
        .GetSnapshotAsync()
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("ProjectID: " + FirebaseApp.DefaultInstance.Options.ProjectId);
                Debug.LogError("Lỗi load notification: " + task.Exception);
                return;
            }
            Debug.Log("UID đang query: " + uid);
            Debug.Log("Số lượng noti: " + task.Result.Count);
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            foreach (var doc in task.Result.Documents)
            {
                string message = doc.GetValue<string>("message");
                string title=doc.GetValue<string>("title");
                string student=doc.GetValue<string>("uidStudent");
                bool isRead = doc.GetValue<bool>("isRead");

                GameObject item = Instantiate(itemPrefab, content);

                ItemNoti ui = item.GetComponent<ItemNoti>();
                if (ui == null)
                {
                    Debug.LogError("Prefab thiếu ItemNoti!");
                }
                ui.Setup(title,message, isRead, doc.Reference);
            }
        });
    }
    //click button dong 
    public void Clickdong()
    {
        panelNoti.SetActive(false);
    }
    //tao tk
    public void Taotk()
    {
        SceneManager.LoadScene("TaoTkhs");
    }
    //load ten
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
                        inputTen.text = ten;
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
    //load anh
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
                        string avatarID = snap.GetValue<string>("avatar");

                        Sprite avatar = Resources.Load<Sprite>("Avatar/" + avatarID);
                        Debug.Log("Đang load avatar: Avatar/" + avatarID);

                        if (avatar != null)
                        {
                            avatarHienTai.sprite = avatar;
                        }
                        else
                        {
                            Debug.LogError("Không tìm thấy avatar: " + avatarID);
                        }
                    }
                }
            });
    }
    //cap nhat 
    public void capnhat()
    {
        SceneManager.LoadScene("Capnhat");
    }
    //changepass
    public void changepass()
    {
        SceneManager.LoadScene("ChangePass");
    }
    //Logout
    public void Logout()
    {
        auth.SignOut();
        SceneManager.LoadScene("Formlogin");
    }
    //xem ds hocj sinh 
    public void Showlist()
    {
        SceneManager.LoadScene("Xemds");
    }
}
