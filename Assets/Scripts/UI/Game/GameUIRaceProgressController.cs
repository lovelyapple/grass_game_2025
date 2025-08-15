
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
        public RectTransform Marker;
        public IDisposable Disposable;
        public void Release()
        {
            Disposable?.Dispose();
            Disposable = null;

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
        .Subscribe(player => RegsitryPlayer(player))
        .AddTo(this);

        RoomModel.GetInstance().OnPlayerLeaveObservable()
        .Subscribe(id => OnPlayerLeave(id))
        .AddTo(this);
    }
    private void RegsitryPlayer(MatchPlayerModel playerModel)
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

        mark.Disposable = playerModel.FieldPlayerController.OnZPosUpdatedObservable()
        .Subscribe(x =>
        {
            var progress = MatchModel.GetInstance().TranslatePlayerProgress(x.transform.position.z);
            UpdatePlayerTran(progress, mark.Marker);
        })
        .AddTo(this);
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
