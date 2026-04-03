using System.Collections;
using System.Collections.Generic;
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

public class ItemNoti : MonoBehaviour
{
    public TextMeshProUGUI txtMessage;
    public TextMeshProUGUI txtTitle;
    public Image background;
    public Button btn;

    private DocumentReference docRef;
    private bool isRead;

    public void Setup(string title,string message, bool read, DocumentReference reference)
    {
        txtMessage.text = message;
        txtTitle.text = title;
        docRef = reference;
        isRead = read;

        UpdateUI();

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);
    }
    void OnClick()
    {
        if (!isRead)
        {
            //update Firestore
            docRef.UpdateAsync("isRead", true);

            isRead = true;
            UpdateUI();
        }

        Debug.Log("Đã click thông báo");
    }
    void UpdateUI()
    {
        if (isRead)
        {
            background.color = Color.white; // đã xem
        }
        else
        {
            background.color = Color.gray; // chưa xem
        }
    }
}

