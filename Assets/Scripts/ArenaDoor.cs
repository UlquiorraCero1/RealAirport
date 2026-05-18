using UnityEngine;
using TMPro;
using System.Collections;

public class ArenaDoor : MonoBehaviour
{
    [Header("Rooms Required to be Cleared")]
    public Room[] requiredRooms;

    [Header("Door Visuals")]
    public GameObject leftDoor;
    public GameObject rightDoor;
    public float slideDistance = 3f;
    public float openSpeed = 4f;

    [Header("UI Message")]
    public TextMeshProUGUI popupText;

    private bool hasOpened = false;
    private Vector3 leftOpen, rightOpen;

    void Start()
    {
        if (leftDoor != null && rightDoor != null)
        {
            leftOpen = leftDoor.transform.position + Vector3.left * slideDistance;
            rightOpen = rightDoor.transform.position + Vector3.right * slideDistance;
        }

        if (popupText != null)
            popupText.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasOpened) return;
        if (!other.CompareTag("Player")) return;

        bool allClear = true;
        foreach (Room r in requiredRooms)
        {
            if (r != null && !r.isCleared)
                allClear = false;
        }

        if (allClear)
        {
            hasOpened = true;
            StartCoroutine(ShowMessage("ARENA UNLOCKED!", Color.green));
            StartCoroutine(OpenDoors());
        }
        else
        {
            StartCoroutine(ShowMessage("CLEAR ALL ENEMIES FIRST!", Color.red));
        }
    }

    IEnumerator ShowMessage(string message, Color color)
    {
        if (popupText == null) yield break;

        popupText.text = message;
        popupText.color = color;
        popupText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2.5f);
        
        popupText.gameObject.SetActive(false);
    }

    IEnumerator OpenDoors()
    {
        while (Vector3.Distance(leftDoor.transform.position, leftOpen) > 0.01f)
        {
            leftDoor.transform.position = Vector3.Lerp(leftDoor.transform.position, leftOpen, openSpeed * Time.deltaTime);
            rightDoor.transform.position = Vector3.Lerp(rightDoor.transform.position, rightOpen, openSpeed * Time.deltaTime);
            yield return null;
        }
    }
}