using UniRx;
using UnityEngine;

namespace Sandbox.PolyLabel
{
    public class GameCamera : MonoBehaviour
    {
        [field: SerializeField]
        public Camera MainCamera { get; private set; }

        [Space, SerializeField]
        private float zoomOutMin = 10;

        [SerializeField]
        private float zoomOutMax = 25;

        private GameInput gameInput;

        private Vector3 defaultPosition;
        private float defaultOrthographicSize;

        private float mapMinX, mapMaxX, mapMinY, mapMaxY;
        private Vector3 startTapPosition;

        private readonly CompositeDisposable disposables = new();

        private void Awake()
        {
            defaultPosition = MainCamera.transform.position;
            defaultOrthographicSize = MainCamera.orthographicSize;
        }

        private void OnDisable()
        {
            disposables.Clear();
        }

        private void OnDestroy()
        {
            disposables.Dispose();
        }

        public void Initialize(GameInput gameInput, BoxCollider boxCollider)
        {
            this.gameInput = gameInput;

            disposables.Clear();

            var bounds = boxCollider.bounds;
            var position = boxCollider.center;

            mapMinX = position.x - bounds.size.x / 2f;
            mapMaxX = position.x + bounds.size.x / 2f;

            mapMinY = position.y - bounds.size.y / 2f;
            mapMaxY = position.y + bounds.size.y / 2f;

            Subscribe();
        }

        public void SetDefaultPosition()
        {
            MainCamera.transform.position = defaultPosition;
            MainCamera.orthographicSize = defaultOrthographicSize;
        }

        private void Subscribe()
        {
            gameInput.OnDown.Subscribe(HandleDown).AddTo(disposables);
            gameInput.OnDrag.Subscribe(HandleDrag).AddTo(disposables);
            gameInput.OnZoom.Subscribe(HandleZoom).AddTo(disposables);
        }

        private Vector3 ClampCamera(Vector3 targetPosition)
        {
            var orthographicSize = MainCamera.orthographicSize;
            var cameraWidth = orthographicSize * MainCamera.aspect;

            var minX = mapMinX + cameraWidth;
            var maxX = mapMaxX - cameraWidth;
            var minY = mapMinY + orthographicSize;
            var maxY = mapMaxY - orthographicSize;

            var newX = Mathf.Clamp(targetPosition.x, minX, maxX);
            var newY = Mathf.Clamp(targetPosition.y, minY, maxY);

            return new Vector3(newX, newY, targetPosition.z);
        }

        // Events

        private void HandleDown(Vector2 screenPoint)
        {
            startTapPosition = MainCamera.ScreenToWorldPoint(screenPoint);
        }

        private void HandleDrag(Vector2 screenPoint)
        {
            var direction = startTapPosition - MainCamera.ScreenToWorldPoint(screenPoint);
            MainCamera.transform.position = ClampCamera(MainCamera.transform.position + direction);
        }

        private void HandleZoom((float increment, Vector2 center) data)
        {
            if (Application.isEditor)
            {
                var startPoint = MainCamera.ScreenToWorldPoint(Input.mousePosition);
                MainCamera.orthographicSize = Mathf.Clamp(MainCamera.orthographicSize - data.increment * 10, zoomOutMin, zoomOutMax);
                var newPoint = startPoint - MainCamera.ScreenToWorldPoint(Input.mousePosition);
                MainCamera.transform.position = ClampCamera(MainCamera.transform.position + newPoint);
            }
            else
            {
                var startPoint = MainCamera.ScreenToWorldPoint(data.center);
                MainCamera.orthographicSize = Mathf.Clamp(MainCamera.orthographicSize - data.increment, zoomOutMin, zoomOutMax);
                var newPoint = startPoint - MainCamera.ScreenToWorldPoint(data.center);
                MainCamera.transform.position = ClampCamera(MainCamera.transform.position + newPoint);
            }
        }
    }
}