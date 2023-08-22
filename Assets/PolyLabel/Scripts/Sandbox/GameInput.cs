using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sandbox.PolyLabel
{
    public class GameInput : MonoBehaviour
    {
        private readonly ISubject<Vector2> onDown = new Subject<Vector2>();
        private readonly ISubject<Vector2> onUp = new Subject<Vector2>();
        private readonly ISubject<Vector2> onTap = new Subject<Vector2>();
        private readonly ISubject<Vector2> onDrag = new Subject<Vector2>();
        private readonly ISubject<Vector2> onStopDrag = new Subject<Vector2>();
        private readonly ISubject<(float, Vector2)> onZoom = new Subject<(float, Vector2)>();

        private bool isMouseDown;
        private bool isClickUi;

        public IObservable<Vector2> OnDown => onDown;
        public IObservable<Vector2> OnUp => onUp;
        public IObservable<Vector2> OnTap => onTap;
        public IObservable<Vector2> OnDrag => onDrag;
        public IObservable<Vector2> OnStopDrag => onStopDrag;
        public IObservable<(float, Vector2)> OnZoom => onZoom;

        public bool IsEnabled { get; private set; }

        private void Update()
        {
            Tick();
            Zoom();
        }

        public void SetEnabled(bool isEnabled)
        {
            if (IsEnabled == isEnabled)
            {
                return;
            }

            if (!isEnabled)
            {
                isMouseDown = false;
            }

            IsEnabled = isEnabled;
        }

        private void Tick()
        {
            if (Application.isEditor)
            {
                Mouse();
            }
            else
            {
                Touch();
            }
        }

        private void Mouse()
        {
            if (!IsEnabled)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (IsClickOnUI(Input.mousePosition))
                {
                    return;
                }

                isMouseDown = true;
                onDown.OnNext(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isMouseDown = false;
                onUp.OnNext(Input.mousePosition);
            }

            if (isMouseDown)
            {
                onDrag.OnNext(Input.mousePosition);
            }
        }

        private void Touch()
        {
            if (!IsEnabled)
            {
                return;
            }

            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        if (IsClickOnUI(touch.position))
                        {
                            isClickUi = true;
                            return;
                        }

                        isClickUi = false;
                        onDown.OnNext(touch.position);
                        break;
                    case TouchPhase.Moved:
                        if (isClickUi)
                        {
                            return;
                        }

                        onDrag.OnNext(touch.position);
                        break;
                    case TouchPhase.Ended:
                        if (isClickUi)
                        {
                            return;
                        }

                        onUp.OnNext(touch.position);
                        break;
                }
            }
        }

        private void Zoom()
        {
            if (Input.touchCount == 2)
            {
                var touchZero = Input.GetTouch(0);
                var touchOne = Input.GetTouch(1);
                var center = (touchOne.position + touchZero.position) / 2;

                var touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                var touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                var prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                var currentMagnitude = (touchZero.position - touchOne.position).magnitude;

                var difference = currentMagnitude - prevMagnitude;

                if (difference == 0)
                {
                    return;
                }

                onZoom.OnNext((difference * 0.05f, center));
            }
            else
            {
                var difference = Input.GetAxis("Mouse ScrollWheel");

                if (difference == 0)
                {
                    return;
                }

                onZoom.OnNext((difference, Input.mousePosition));
            }
        }

        private static bool IsClickOnUI(Vector2 screenPoint)
        {
            var pointerEventData = new PointerEventData(EventSystem.current) { position = screenPoint };
            var raycastResultsList = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResultsList);

            return raycastResultsList.Any(result => result.gameObject != null);
        }
    }
}