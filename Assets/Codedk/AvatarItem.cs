using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarItem : MonoBehaviour
{
    public string avatarID;   // ví dụ: avatar1, avatar2
    public Image avatarImage; // ảnh của chính nó
    public capnhat manager;   // kéo script capnhat vào đây

    void Start()
    {
        if (manager == null)
        {
            manager = FindObjectOfType<capnhat>();
        }
    }

    public void Click()
    {
        Debug.Log("Manager hiện tại: " + manager);
        if (manager == null)
        {
            Debug.LogError("Chưa gán manager!");
            return;
        }

        if (avatarImage == null)
        {
            Debug.LogError("Chưa gán avatarImage!");
            return;
        }

        manager.ChonAvatar(avatarID, avatarImage.sprite);
    }
    
}
