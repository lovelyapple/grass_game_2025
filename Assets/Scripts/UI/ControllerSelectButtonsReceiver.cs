using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
using UnityEngine.UI;
namespace Sytem.Controller
{
    public enum ControllerAxis
    {
        Up,
        Down,
    }
    public class ControllerSelectButtonsReceiver : MonoBehaviour
    {
        public bool UsingController { get; private set; }
        [SerializeField] private List<Button> CurrentUIButtons;
        private Image _selectedFrameImage;
        private Button _selectingButton;
        private Subject<float> _inputVerticalSubject = new Subject<float>();
        private Subject<ControllerAxis> _inputAxisReceivedSubject = new Subject<ControllerAxis>();
        public Observable<ControllerAxis> InputAxisOvservable() => _inputAxisReceivedSubject;
        private Subject<Button> _onTapButtonSubject = new Subject<Button>();
        public Observable<Button> OnTapButtonObservable() => _onTapButtonSubject;
        private const float Margin = 20f;
        private void Awake()
        {
            _inputVerticalSubject
                .ThrottleFirst(TimeSpan.FromSeconds(0.5))
                .Subscribe(v =>
                {
                    var outPut = v > 0 ? ControllerAxis.Up : ControllerAxis.Down;
                    _inputAxisReceivedSubject.OnNext(outPut);
                    UsingController = true;

                    UpdateFrame();
                    SoundManager.PlayOneShot(SeType.Button_Common_Se);
                })
                .AddTo(this);

            if(UsingController)
            {
                UpdateFrame();
            }
        }
        public void Update()
        {
            var axis = Input.GetAxis("Vertical");

            if (axis != 0)
            {
                _inputVerticalSubject.OnNext(axis);
            }

            if(Input.GetKeyDown(KeyCode.Joystick1Button0) && _selectingButton != null)
            {
                _onTapButtonSubject.OnNext(_selectingButton);
                SoundManager.PlayOneShot(SeType.Button_Confirm_Se);
            }
        }
        private void UpdateFrame()
        {
            if (_selectingButton == null)
            {
                _selectingButton = CurrentUIButtons.FirstOrDefault();
            }
            else
            {
                if (_selectedFrameImage == null)
                {
                    _selectedFrameImage = Instantiate(ResourceContainer.Instance.GetButtonSelectFrame(), transform, false);
                }

                var index = CurrentUIButtons.IndexOf(_selectingButton);
                var nextIndex = (index + 1) % CurrentUIButtons.Count;

                _selectingButton = CurrentUIButtons[nextIndex];
                _selectedFrameImage.transform.SetParent(_selectingButton.transform);
                var frameRect = _selectedFrameImage.rectTransform;
                frameRect.anchoredPosition = Vector2.zero;
                frameRect.offsetMin = new Vector2(-Margin, -Margin);
                frameRect.offsetMax = new Vector2(Margin, Margin);
                frameRect.anchorMin = Vector2.zero;
                frameRect.anchorMax = Vector2.one;

                frameRect.SetSiblingIndex(0);
            }
        }
    }
}
