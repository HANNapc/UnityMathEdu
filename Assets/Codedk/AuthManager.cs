using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;
public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    private FirebaseAuth auth;

    // 🔥 THÔNG TIN USER
    public string currentUserID;
    public string currentClass;
    public string currentRole; // "student" | "parent"

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    // =========================
    // 👨‍🎓 LOGIN HỌC SINH
    // =========================
    public void SetStudent(string userID, string lop)
    {
        currentUserID = userID;
        currentClass = lop;
        currentRole = "student";
        Debug.Log("Login Student: " + lop);
    }

    // =========================
    // 👨‍👩‍👧 LOGIN PHỤ HUYNH
    // =========================
    public void SetParent(string userID)
    {
        currentUserID = userID;
        currentClass = null;
        currentRole = "Parent";

        Debug.Log("Login Parent");
    }

    // =========================
    // 📚 LẤY CLASS ID (cho game)
    // =========================
    public string GetClassID()
    {
        if (string.IsNullOrEmpty(currentClass))
            return null;
        return currentClass.Replace("Lớp ", ""); // "Lớp 1" → "1"
    }

    // =========================
    // 🚪 LOGOUT
    // =========================
    public void Logout()
    {
        if (auth != null)
            auth.SignOut();

        currentUserID = null;
        currentClass = null;
        currentRole = null;

        Debug.Log("Logged out");
    }
}     
