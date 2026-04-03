using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return; // hoặc "Enemy"

        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            player.StopMoving(); // dừng hẳn
            Debug.Log("Zoombie chạm lâu đài, dừng lại!");
            ManagerGamer.Instance.GameOver();
            int lessonID1 = GameData.currentLessonID;
            ManagerGamer.Instance.EndSession(lessonID1, 0);
           
        }
    }
}
