using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpenOverlay : MonoBehaviour
{
    [SerializeField] private Button friendButton;
    [SerializeField] private Canvas friendsCanvas;
    [SerializeField] private List<Button> friendButtons;


    private void OnEnable()
    {
        var activeFriends = Transport.Instance.facade.host.GetActiveFriends();
        foreach (var friend in activeFriends)
        {
            var instance = Instantiate(friendButton, friendsCanvas.transform);
            var text = instance.GetComponentInChildren<TextMeshProUGUI>();
            text.text = friend.friendName;
            instance.onClick.AddListener(() => ButtonClicked(friend.friendId));
            friendButtons.Add(instance);
        }
    }

    private void OnDisable()
    {
        for (var index = friendButtons.Count - 1; index >= 0; index--)
        {
            var button = friendButtons[index];
            button.onClick.RemoveListener(() => ButtonClicked(""));
            Destroy(button);
        }
    }
    private void ButtonClicked(string friendId)
    {
        Transport.Instance.facade.host.InviteFriend(friendId);
    }
}
