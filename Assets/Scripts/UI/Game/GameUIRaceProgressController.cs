
using UnityEngine;
using R3;
using System.Collections.Generic;
using System.Linq;
using System;
using StarMessage.Models;
using UnityEngine.UI;

public class GameUIRaceProgressController : MonoBehaviour
{
    private class MarkPoint
    {
        public int PlayerId;
        public Transform PlayerTransform;
        public RectTransform Marker;
        public void Release()
        {
            if(Marker != null)
            {
                Destroy(Marker.gameObject);
            }
        }
    }
    [SerializeField] RectTransform RootRect;
    [SerializeField] RectTransform PlayerTranPrefab;
    private List<MarkPoint> _markers = new();
    private void Awake()
    {
        PlayerTranPrefab.gameObject.SetActive(false);
        MatchModel.GetInstance().OnPlayerCtrlSpawnedObservable()
        .Subscribe(RegistryPlayer)
        .AddTo(this);

        RoomModel.GetInstance().OnPlayerLeaveObservable()
        .Subscribe(OnPlayerLeave)
        .AddTo(this);
    }
    private void RegistryPlayer(MatchPlayerModel playerModel)
    {
        var mark = _markers.FirstOrDefault(x => x.PlayerId == playerModel.PlayerId);
        
        if(mark != null)
        {
            Debug.LogError($"duplicated id found {playerModel.PlayerId}");
        }
        else
        {
            mark = new();
        }

        if(mark.Marker == null)
        {
            mark.Marker = Instantiate(PlayerTranPrefab, RootRect.transform);
            mark.Marker.anchoredPosition = Vector3.zero;
            mark.Marker.gameObject.SetActive(true);
        }

        if(!GameCoreModel.Instance.IsAdminUser && RoomModel.GetInstance().SelfPlayerRef.PlayerId == playerModel.PlayerId)
        {
            var image = mark.Marker.GetComponentInChildren<Image>();
            image.color = Color.red; 
            mark.Marker.transform.localScale = Vector3.one * 1.2f;
        }

        mark.PlayerTransform = playerModel.FieldPlayerController.transform;
        
        var progress = MatchModel.GetInstance().TranslatePlayerProgress(mark.PlayerTransform.position.z);
        UpdatePlayerTran(progress, mark.Marker);
        _markers.Add(mark);
    }

    private void LateUpdate()
    {
        foreach (var mark in _markers)
        {
            if (mark.PlayerTransform == null)
            {
                continue;
            }

            var progress = MatchModel.GetInstance().TranslatePlayerProgress(mark.PlayerTransform.position.z);
            UpdatePlayerTran(progress, mark.Marker);
        }
    }

    private void OnPlayerLeave(int playerId)
    {
        var mark = _markers.FirstOrDefault(x => x.PlayerId == playerId);

        if (mark == null)
        {
            Debug.LogError($"duplicated id found {playerId}");
            return;
        }

        mark.Release();
        _markers.Remove(mark);
    }
    public void UpdatePlayerTran(float progress, RectTransform markerRect)
    {
        progress = Mathf.Clamp01(progress);

        // レイアウトの変更の直後のフレームwidthが0の可能性がある
        if(RootRect.rect.width == 0)
        {
            return;
        }

        var xFromLeft = (1 - progress) * RootRect.rect.width - RootRect.rect.width / 2f;
        var pos = markerRect.anchoredPosition;
        pos.x = xFromLeft;
        pos.y = 0f;            // 縦は中央に置くなら0
        markerRect.anchoredPosition = pos;
    }

    public void UpdatePlayerTranEditor(float progress)
    {
#if UNITY_EDITOR
        UpdatePlayerTran(progress, PlayerTranPrefab);
#endif
    }
}
