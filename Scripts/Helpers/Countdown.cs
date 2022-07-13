using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Countdown : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeUI;
    [SerializeField] private Image image;
    
    public void Begin(int time, Action action)
    {
        StartCoroutine(DoCountDown(time, action));
    }

    private void Awake()
    {
        timeUI.gameObject.SetActive(false);
        image.enabled = false;
    }

    private IEnumerator DoCountDown(int time, Action action)
    {
        timeUI.gameObject.SetActive(true);
        image.enabled = true;
        for (int i = time - 1; i >= 0; i--)
        {
            timeUI.text = i.ToString();
            yield return new WaitForSeconds(1);
        }
        timeUI.gameObject.SetActive(false);
        image.enabled = false;
        action();
    }

    public void SetTimeInUI(int time)
    {
        timeUI.gameObject.SetActive(true);
        image.enabled = true;
        timeUI.text = time.ToString();
    }
}
