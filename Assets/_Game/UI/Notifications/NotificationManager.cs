using _Game.Car;
using _Game.Car.Player;
using _Game.Game_States_Logic;
using Assets._Game.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static _Game.Game_States_Logic.MatchManager;

public class NotificationManager : NetworkBehaviour
{
    [SerializeField] private Notification notificationPrefab;
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private float notificationDuration = 4;

    private void Start()
    {
        MatchManager.Singleton.onEvent += EventCallback;
        Debug.Log("Notification manager is invoked");
    }

    private void EventCallback(GameEventType eventType, CarPhysics player)
    {
        Debug.Log("HI");
        PlayerData playerData = player != null ? player.GetComponent<PlayerData>() : null;

        switch (eventType)
        {
            case GameEventType.Parked:
                NewNotificationClientRpc($"{playerData.Player} has parked!");
                break;
            case GameEventType.Start:
                Debug.Log("STARTING NOTIF");
                NewNotificationClientRpc("Starting game!");
                break;
        }
    }

    [ClientRpc]
    public void NewNotificationClientRpc(string text)
    {
        Debug.Log("NOTIFICATION RPC WORKING!");
        StartCoroutine(NewNotification(text));
    }
    
    private IEnumerator NewNotification(string text)
    {
        Debug.Log(text);
        Notification notification = Instantiate(notificationPrefab);
        notification.textObject.text = text;
        notification.transform.SetParent(notificationPanel.transform);

        yield return new WaitForSeconds(notificationDuration);
        notification.transform.DOMove(new Vector3(notification.transform.position.x - 400, notification.transform.position.y), 1);
        Debug.Log("Should be removing now");
        yield return new WaitForSeconds(0.8f);
        Destroy(notification.gameObject);
    }
}
