using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using System.Collections.Generic;

public class NotiManager : MonoBehaviour
{
    FirebaseFirestore db;

    void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    public void Send(string uidParent,string title, string message, string uidStudent)
    {
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            { "uidParent", uidParent },
            { "uidStudent", uidStudent},
            { "title", title },
            { "message", message },
            { "time", Timestamp.GetCurrentTimestamp() },
            { "isRead", false }
        };

        db.Collection("Notifications").AddAsync(data);

        Debug.Log("Đã gửi thông báo!");
    }
}
