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
using System;

public class xemds : MonoBehaviour
{
    public Transform content;       // Content của ScrollView
    public GameObject itemPrefab;
    private FirebaseAuth auth;

    FirebaseFirestore db;

    //panel
    public GameObject panelds;
    public GameObject panelcapnhat;
    //
    private DataStudent currentStudent;
    public TMP_InputField inputName;
    public TMP_InputField inputpass;
    public TMP_InputField inputpassxn;
    public TMP_Dropdown dropdown;
    private Action<DataStudent> onUpdated;
    bool isPasswordHidden = true;
    private bool isPassHidden = true;
    public TextMeshProUGUI messageText;//message

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        LoadDanhSach();
        //hien thi panelds
        panelds.SetActive(true);
        panelcapnhat.SetActive(false);

    }

   
    void LoadDanhSach()
    {
        FirebaseUser user = auth.CurrentUser;
        if (user == null)
        {
            Debug.LogError("Chua Login ");
            return;
        }

        db.Collection("Students")
        .WhereEqualTo("uidphuhuynh",user.UserId)
        .GetSnapshotAsync()
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Số lượng: " + task.Result.Count);
                foreach (Transform child in content)
                {
                    Destroy(child.gameObject);
                }

                QuerySnapshot snapshot = task.Result;

                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    try
                    {
                        DataStudent st = new DataStudent()   // 🔥 tạo mới mỗi lần
                        {
                            id = doc.Id,
                            name = doc.GetValue<string>("name"),
                            password = doc.GetValue<string>("password"),
                            lop = doc.GetValue<string>("lop"),
                            avatar = doc.GetValue<string>("avatar"),
                            Status = doc.GetValue<string>("Status"),
                            CurrentStar = doc.GetValue<int>("CurrentStar"),
                            CompletedLessonCount = doc.GetValue<int>("CompletedLessonCount"),
                            role = doc.GetValue<string>("role"),
                            uidphuhuynh = doc.GetValue<string>("uidphuhuynh"),
                        };

                        Debug.Log("ID: " + st.id + " | Name: " + st.name);

                        GameObject item = Instantiate(itemPrefab, content);

                        StudentItemUI ui = item.GetComponent<StudentItemUI>();
                        ui.Setup(st, OnEditStudent);
                        

                        //  sau khi load xong → mới set student
                        if (currentStudent != null)
                        {
                            SetDropdownByClass(currentStudent.lop);
                        }

                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Lỗi document: " + doc.Id + " | " + e.Message);
                    }
                }
            }
            else
            {
                Debug.LogError("Lỗi load danh sách: " + task.Exception);
            }
        });
    }
    //check hiển thị panel 
    void OnEditStudent(DataStudent st)
    {
        panelds.SetActive(false);
        panelcapnhat.SetActive(true);

        Open(st, OnStudentUpdated);
    }

    public void Open(DataStudent st, Action<DataStudent> callback)
    {
        gameObject.SetActive(true);

        currentStudent = st;
        onUpdated = callback;

        inputName.text = st.name;
        //dropdown.captionText.text = st.lop;
        SetDropdownByClass(st.lop);
    }

    //cap nhat hs
    public async void UpdateInfor()
    {
        if (inputpass.text != inputpassxn.text)
        {
            ShowMessage("Vui lòng nhập đúng mật khẩu xác nhận.");
        }
        currentStudent.name = inputName.text;
        currentStudent.password = HashPassword(inputpass.text);
        currentStudent.lop= dropdown.options[dropdown.value].text;

        //UPDATE FIRESTORE
        DocumentReference docRef = db.Collection("Students").Document(currentStudent.id);

        Dictionary<string, object> updates = new Dictionary<string, object>()
        {
            { "name", currentStudent.name },
            { "password", currentStudent.password },
            {"lop",currentStudent.lop}
        };

        await docRef.UpdateAsync(updates);

        //callback để update UI
        OnStudentUpdated(currentStudent);

        gameObject.SetActive(false);
    }
    //callback để update UI
    void OnStudentUpdated(DataStudent st)
    {
        Debug.Log("Đã cập nhật: " + st.name);

        //reload lại list 
        LoadDanhSach();

        panelds.SetActive(true);
        panelcapnhat.SetActive(false);
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

    //quay lai 
    public void ql()
    {
        panelds.SetActive(true);
        panelcapnhat.SetActive(false);
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
    //thong bao
    void ShowMessage(string msg)
    {
        messageText.text = msg;
    }
    //hien thi lop
    void SetDropdownByClass(string lop)
    {
        int index = dropdown.options.FindIndex(option => option.text == lop);

        if (index >= 0)
        {
            dropdown.value = index;
            dropdown.RefreshShownValue();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy lớp: " + lop);
        }
    }
}
