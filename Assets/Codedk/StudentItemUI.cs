using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class StudentItemUI : MonoBehaviour
{
    public TextMeshProUGUI tenText;
    public Button btnEdit;

    private DataStudent student;              
    private Action<DataStudent> onEdit;


    public void Setup(DataStudent st, Action<DataStudent> callback)
    {
        student = st;

        tenText.text = st.name;

        Debug.Log("UI name: " + gameObject.name);
        Debug.Log("btnEdit = " + (btnEdit == null ? "NULL" : "OK"));

        if (btnEdit == null)
        {
            Debug.LogError("❌ btnEdit vẫn NULL trong runtime!");
            return;
        }

        btnEdit.onClick.RemoveAllListeners();
        btnEdit.onClick.AddListener(() =>
        {
            Debug.Log("CLICKED: " + st.name);

            // gọi ra ngoài để xử lý UI
            callback?.Invoke(st);
        });
    }
    void Awake()
    {
        if (btnEdit == null)
        {
            btnEdit = GetComponentInChildren<Button>();
        }
    }

    //  cập nhật lại UI sau khi sửa
    public void Refresh()
    {
        tenText.text = student.name;
    }
    
}