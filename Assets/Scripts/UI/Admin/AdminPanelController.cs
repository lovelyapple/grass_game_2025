using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using StarMessage.Models;
using UnityEngine;
using R3;
using UnityEngine.UI;

public class AdminPanelController : MonoBehaviour
{
    [SerializeField] GameObject MainUIRoot;
    [SerializeField] List<AdminPlayerCellView> PlayerViews;
    [SerializeField] Button ShowHideButton;
    [SerializeField] Button RefreshButton;
    private static AdminPanelController _instance;
    private bool _isShow = false;
    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public static void Open()
    {
        _instance.Setup();
    }
    public void Setup()
    {
        _isShow = true;
        MainUIRoot.SetActive(true);
        ModelCache.Admin.RequestUpateAdminViewObservable()
        .Subscribe(x => RequestUpdate())
        .AddTo(this);

        ShowHideButton.OnClickAsObservable()
        .Subscribe(_ =>
        {
            _isShow = !_isShow;
            MainUIRoot.SetActive(_isShow);
        })
        .AddTo(this);

        RefreshButton.OnClickAsObservable()
        .Subscribe(_ => RequestUpdate())
        .AddTo(this);
    }
    private void RequestUpdate()
    {
        var players = RoomModel.GetInstance().PlayerInfos.ToArray();
        var curCnt = players.Length;

        for(int i = 0; i < GameConstant.MaxPlayerPerRoom; i++)
        {
            if(i < curCnt)
            {
                PlayerViews[i].gameObject.SetActive(true);
                PlayerViews[i].Setup(players[i].PlayerId);
            }
            else
            {
                PlayerViews[i].gameObject.SetActive(false);
            }
        }
    }
}
