using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadScenes : MonoBehaviour
{
    public Button[] selectgane;
    void Start()
    {
        loadScence();
    }
    public void loadScence()
    {
        for (int i = 0; i < selectgane.Length; i++)
        {
            int index = i;
            Debug.Log("chay thanh cong");
            selectgane[i].onClick.AddListener(() => SelectGame(index));
        }
    }
    public void SelectGame(int index)
    {
        if (index == 0)
        {
            SceneManager.LoadScene("GameZoombie");
        }
        else if (index == 1)
        {
            Debug.Log("Vao man choi 2");
        }
        else
        {
            Debug.Log("Vao man choi 3");
        }
    }
}
