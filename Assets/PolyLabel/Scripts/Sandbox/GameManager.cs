using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

namespace Sandbox.PolyLabel
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private DebugView debugView;

        [Space, SerializeField]
        private GameObject pictureObject;

        [Space, SerializeField]
        private GameObject outlineObject;

        [SerializeField]
        private GameObject numberObject;

        [Space, SerializeField]
        private GameInput gameInput;

        [SerializeField]
        private GameCamera gameCamera;

        [SerializeField]
        private BoxCollider gameCollider;

        [Header("Settings"), SerializeField]
        private float widthOutline = 0.2f;

        [SerializeField]
        private bool isCreateOutlines;

        [SerializeField]
        private bool isCreateCircles;

        [SerializeField]
        private bool isCreateNumbers = true;

        private RaycastHit rayHit;

        private int shapesCount;
        private int verticesCount;
        private int trianglesCount;

        private readonly List<GameObject> outlines = new();
        private readonly List<GameObject> circles = new();
        private readonly List<GameObject> numbers = new();

        private Material shapeColorUnlit;

        private void Awake()
        {
            shapeColorUnlit = Resources.Load<Material>($"Materials/Pictures/ShapeColorUnlit");

            gameInput.SetEnabled(true);
            gameCamera.Initialize(gameInput, gameCollider);
        }

        private void OnEnable()
        {
            debugView.OnGenerate += OnGenerate;
            debugView.OnZoom += OnZoom;
        }

        private void OnDisable()
        {
            debugView.OnGenerate -= OnGenerate;
            debugView.OnZoom -= OnZoom;
        }

        private void Update()
        {
            Tick();
        }

        private void Generate()
        {
            Clear();
            GenerateShapes();
            CreateOutlines();
            CreateCircles();
            CreateNumbers();
            PrintInfo();
        }

        private void Tick()
        {
            if (!Input.GetMouseButtonUp(0))
            {
                return;
            }

            var position = Input.mousePosition;
            Physics.Raycast(gameCamera.MainCamera.ScreenPointToRay(position), out rayHit);
            var rayHitCollider = rayHit.collider;

            if (rayHitCollider == null)
            {
                return;
            }

            var shape = rayHitCollider.GetComponent<ShapeView>();

            if (shape == null)
            {
                return;
            }

            shape.FillColor(GetColor());
        }

        private void GenerateShapes()
        {
            shapesCount = 0;
            verticesCount = 0;
            trianglesCount = 0;

            foreach (Transform child in pictureObject.transform)
            {
                var shape = child.gameObject.GetComponent<ShapeView>();

                if (shape == null)
                {
                    shape = child.gameObject.AddComponent<ShapeView>();
                }

                shape.Initialize(shapeColorUnlit);

                verticesCount += shape.GetVerticesCount();
                trianglesCount += shape.GetTrianglesCount();

                shapesCount++;
            }
        }

        private void CreateOutlines()
        {
            if (!isCreateOutlines)
            {
                return;
            }

            foreach (Transform child in pictureObject.transform)
            {
                var mesh = child.GetComponent<MeshFilter>().mesh;
                var vertices = mesh.vertices;
                var edges = EdgeHelpers.GetEdges(mesh.triangles).FindBoundary().SortEdges();
                var points = new List<Vector3>();

                for (var i = 0; i < edges.Count; i++)
                {
                    points.Add(new Vector3(vertices[edges[i].V1].x, vertices[edges[i].V1].y));
                }

                var outline = Instantiate(outlineObject, child, false);
                var ln = outline.GetComponent<LineRenderer>();
                ln.positionCount = points.Count;
                ln.startWidth = widthOutline;
                ln.endWidth = widthOutline;
                ln.SetPositions(points.ToArray());

                outlines.Add(outline);
            }
        }

        private void CreateCircles()
        {
            if (!isCreateCircles)
            {
                return;
            }

            foreach (Transform child in pictureObject.transform)
            {
                var mesh = child.GetComponent<MeshFilter>().mesh;
                var vertices = mesh.vertices;
                var edges = EdgeHelpers.GetEdges(mesh.triangles).FindBoundary().SortEdges();
                var edgePoints = new List<Vector2>();

                for (var i = 0; i < edges.Count; i++)
                {
                    var v3 = vertices[edges[i].V1];
                    edgePoints.Add(new Vector2(v3.x, v3.y));
                }

                var data = PolyLabelNet.FindPoleOfIsolation(edgePoints);
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.GetComponent<Renderer>().material.color = Color.yellow;
                sphere.transform.SetParent(child);
                sphere.transform.localScale = new Vector3(data.radius * 2, data.radius * 2, 1);
                sphere.transform.localPosition = new Vector3(data.pole.x, data.pole.y);
                sphere.name = $"Sphere: {child.name} [{data.radius}]";

                circles.Add(sphere);
            }
        }

        private void CreateNumbers()
        {
            if (!isCreateNumbers)
            {
                return;
            }

            var index = 1;

            foreach (Transform child in pictureObject.transform)
            {
                var mesh = child.GetComponent<MeshFilter>().mesh;
                var vertices = mesh.vertices;
                var edges = EdgeHelpers.GetEdges(mesh.triangles).FindBoundary().SortEdges();
                var edgePoints = new List<Vector2>();

                for (var i = 0; i < edges.Count; i++)
                {
                    var v3 = vertices[edges[i].V1];
                    edgePoints.Add(new Vector2(v3.x, v3.y));
                }

                var data = PolyLabelNet.FindPoleOfIsolation(edgePoints);
                var number = Instantiate(numberObject, child, false);
                var rect = number.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(data.radius, data.radius);
                number.transform.localPosition = new Vector3(data.pole.x, data.pole.y, -3);
                number.GetComponent<TMP_Text>().text = $"{index}";
                numbers.Add(number);

                index++;
            }
        }

        private void Clear()
        {
            foreach (var outline in outlines)
            {
                Destroy(outline);
            }

            foreach (var label in circles)
            {
                Destroy(label);
            }

            foreach (var number in numbers)
            {
                Destroy(number);
            }

            outlines.Clear();
            circles.Clear();
            numbers.Clear();
        }

        private static Color GetColor()
        {
            var r = Random.Range(0.0f, 1.0f);
            var g = Random.Range(0.0f, 1.0f);
            var b = Random.Range(0.0f, 1.0f);
            return new Color(r, g, b);
        }

        private void PrintInfo()
        {
            debugView.SetInfoText(string.Empty);

            var message = $"<color=green>PICTURE INFO</color>\n" +
                          $"<color=orange>ID:</color> World Map\n" +
                          $"<color=orange>VERTICES:</color> {verticesCount}\n" +
                          $"<color=orange>TRIANGLES:</color> {trianglesCount}\n" +
                          $"<color=orange>SHAPES:</color> {shapesCount}\n";

            debugView.SetInfoText(message);
        }

        // Events

        private void OnGenerate()
        {
            Generate();
        }

        private void OnZoom()
        {
            gameCamera.SetDefaultPosition();
        }
    }
}