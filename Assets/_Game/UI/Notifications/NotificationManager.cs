using _Game.Car;
using _Game.Game_States_Logic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static _Game.Game_States_Logic.MatchManager;

public class NotificationManager : MonoBehaviour
{
    [SerializeField] private Notification notificationPrefab;
    public static NotificationManager Singleton { get; private set; }

    private void Start()
    {
        Singleton = this;
        MatchManager.Singleton.onEvent += EventCallback;
    }

    public void EventCallback(GameEventType eventType, CarPhysics player)
    {
        switch (eventType)
        {

        }
    }
    public void NewNotification(string text)
    {
        Notification notif = Instantiate(notificationPrefab);
        notif.textObject.text = text;
        notif.transform.SetParent(transform);
    }
}
