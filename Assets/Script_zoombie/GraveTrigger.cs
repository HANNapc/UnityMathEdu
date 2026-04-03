using UnityEngine;
using System.Collections;

public class GraveTrigger : MonoBehaviour
{
    private GameObject currentGrave;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Chạy trigger ko đc");
        if (!collision.CompareTag("Grave")) return;
        Debug.LogWarning("Hir Grave");
        PlayerController player = GetComponent<PlayerController>();
        if (player == null) return;
        currentGrave = collision.gameObject;
        player.StopMoving();
        Debug.Log("2");
        var question = FirebaseManager.Instance.GetRandomQuestion();
        if (question == null)
        {
            Debug.Log("No question!");
            player.ResumeMoving();
            return;
        }
        Debug.Log("Question OK");

        // test UI
        Debug.Log("3");
        UIManager.Instance.ShowQuestion(question, player, this);
        Debug.Log("4");
    }

    public void OnAnswered(PlayerController player)
    {
        if (player != null)
        {
            player.ResumeMoving();
        }
        if (currentGrave != null)
        {
            Destroy(currentGrave); // ✅ xoá grave, không phải player
        }
    }
}