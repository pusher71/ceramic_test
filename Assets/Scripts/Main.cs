using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 618, 649
public class Main : MonoBehaviour
{
    //поля ввода
    [SerializeField] private InputField _inputAreaWidth;
    [SerializeField] private InputField _inputAreaHeight;
    [SerializeField] private InputField _inputTileWidth;
    [SerializeField] private InputField _inputTileHeight;
    [SerializeField] private InputField _inputSeamRows;
    [SerializeField] private InputField _inputSeamInRow;
    [SerializeField] private InputField _inputRowsOffset;
    [SerializeField] private InputField _inputAngle;

    //текущие значения полей ввода
    private float _areaWidth;
    private float _areaHeight;
    private float _tileWidth;
    private float _tileHeight;
    private float _seamRows;
    private float _seamInRow;
    private float _rowsOffset;
    private float _angle;

    //поля вывода
    [SerializeField] private Text _textUpdating;
    [SerializeField] private Text _textSquare;
    [SerializeField] private Text _textError;

    [SerializeField] private RectTransform _area; //видимая рабочая область (далее - рабочая область)
    [SerializeField] private RectTransform _areaFill; //квадратная область, полностью заполняющаяся плитками (далее - квадрат)
    [SerializeField] private GameObject _tilePrefab; //заготовка плитки

    //параметры точности
    [SerializeField] private float _epsilonPointRightOfVector = 0.05f;
    [SerializeField] private float _epsilonSegmentsIntersect = 0.000001f;

    void Start()
    {
        OnInputValueChanged();
    }

    //очистить рабочую область
    private void ClearArea()
    {
        while (_areaFill.transform.childCount > 0)
            DestroyImmediate(_areaFill.transform.GetChild(0).gameObject);
    }

    //вводимое значение изменено
    public void OnInputValueChanged()
    {
        //скрыть предыдущее возможное сообщение об ошибке
        _textError.text = "";

        //считать входные данные и проверить их на корректность
        if (!float.TryParse(_inputAreaWidth.text, out _areaWidth)) { ShowError("Area width is incorrect!"); return; }
        if (!float.TryParse(_inputAreaHeight.text, out _areaHeight)) { ShowError("Area height is incorrect!"); return; }
        if (!float.TryParse(_inputTileWidth.text, out _tileWidth)) { ShowError("Tile width is incorrect!"); return; }
        if (!float.TryParse(_inputTileHeight.text, out _tileHeight)) { ShowError("Tile height is incorrect!"); return; }
        if (!float.TryParse(_inputSeamRows.text, out _seamRows)) { ShowError("Seam between rows is incorrect!"); return; }
        if (!float.TryParse(_inputSeamInRow.text, out _seamInRow)) { ShowError("Seam between tiles in a row is incorrect!"); return; }
        if (!float.TryParse(_inputRowsOffset.text, out _rowsOffset)) { ShowError("Rows offset is incorrect!"); return; }
        if (!float.TryParse(_inputAngle.text, out _angle)) { ShowError("Angle of rotation is incorrect!"); return; }

        //проверить размеры на отрицательные значения
        if (_areaWidth <= 0 || _areaHeight <= 0 || _tileWidth <= 0 || _tileHeight <= 0)
        {
            ShowError("All sizes must be positive!");
            return;
        }

        //показать текст обновления
        _textUpdating.enabled = true;

        //запустить процесс обновления
        StartCoroutine(UpdateView());
    }

    //сообщить об ошибке
    private void ShowError(string text)
    {
        _textError.text = text;
    }

    //обновить раскладку плитки
    private IEnumerator UpdateView()
    {
        yield return new WaitForSeconds(0.01f);

        //очистить старую раскладку
        ClearArea();
        _textSquare.text = "";

        //применить размеры к рабочей области
        _area.sizeDelta = new Vector2(_areaWidth, _areaHeight);

        //повернуть квадрат
        _areaFill.rotation = Quaternion.Euler(0, 0, _angle);

        /*
        Плитки заполняют весь квадрат, внешний по отношению к рабочей области.
        Сторона этого квадрата вдвое больше, чем большая сторона рабочей области.
        Таким образом, вся рабочая область будет гарантированно заполнена при любом повороте.
        */

        //определить размер квадрата для заполнения плитками
        float maxAreaSide = Mathf.Max(_areaWidth, _areaHeight); //большая сторона рабочей области
        float areaSetSide = maxAreaSide * 2; //сторона внешнего квадрата, заполняемого плитками
        _areaFill.sizeDelta = new Vector2(areaSetSide, areaSetSide); //применить

        //определить Y нижней и X левой границы рабочей области по отношению к квадрату
        float areaYMin = (areaSetSide - _areaHeight) / 2; //Y нижней границы
        float areaXMin = (areaSetSide - _areaWidth) / 2; //X левой границы

        float periodY = _tileHeight + _seamRows; //период вертикального заполнения
        float periodX = _tileWidth + _seamInRow; //период горизонтального заполнения

        //заполнить квадрат плитками
        float totalSquare = 0; //суммарная площадь видимых частей плиток
        float firstRowY = areaYMin - (Mathf.Floor(areaYMin / periodY) + 1) * periodY; //координата Y крайнего нижнего ряда
        float lastRowY = areaYMin + (Mathf.Floor((areaSetSide - areaYMin) / periodY) + 1) * periodY; //координата Y крайнего верхнего ряда
        for (float y = firstRowY; y <= lastRowY; y += periodY) //для каждого ряда...
        {
            float currentOffset = _rowsOffset * Mathf.Floor((y - areaYMin) / periodY) % periodX; //смещение текущего ряда относительного нижнего в рабочей области
            float firstX = areaXMin - (Mathf.Floor(areaXMin / periodX) + 1) * periodX + currentOffset; //координата X крайней левой плитки
            float lastX = areaXMin + (Mathf.Floor((areaSetSide - areaXMin) / periodX) + 1) * periodX + currentOffset; //координата X крайней правой плитки
            for (float x = firstX; x <= lastX; x += periodX) //для каждой плитки...
            {
                GameObject tile = Instantiate(_tilePrefab, _areaFill.transform); //поставить плитку на квадрат
                RectTransform rect = tile.GetComponent<RectTransform>(); //получить её RectTransform
                rect.anchoredPosition = new Vector2(x, y); //задать позицию на квадрате
                rect.sizeDelta = new Vector2(_tileWidth, _tileHeight); //применить размеры

                //получить массивы координат углов рабочей области и углов плитки, и преобразовать из Vector3 в Vector2
                Vector3[] areaCorners3 = new Vector3[4];
                Vector2[] areaCorners2 = new Vector2[4];
                Vector3[] tileCorners3 = new Vector3[4];
                Vector2[] tileCorners2 = new Vector2[4];
                _area.GetWorldCorners(areaCorners3);
                rect.GetWorldCorners(tileCorners3);
                for (int i = 0; i < 4; i++)
                {
                    areaCorners2[i] = areaCorners3[i];
                    tileCorners2[i] = tileCorners3[i];
                }

                //вычислить площадь видимой части плитки
                float square = GetIntersectSquare(areaCorners2, tileCorners2);

                //добавить эту площадь к общей площади
                totalSquare += square;
            }
        }

        //вывести площадь на экран
        _textSquare.text = "Square: " + totalSquare;

        //скрыть текст обновления
        _textUpdating.enabled = false;
    }

    //вычислить площадь пересечения двух многоугольников
    public float GetIntersectSquare(Vector2[] vertices1, Vector2[] vertices2)
    {
        HashSet<Vector2> verticesIntersect = new HashSet<Vector2>(); //массив вершин пересечения

        //добавить те вершины каждого многоугольника в массив, которые находятся внутри чужого к ним многоугольника
        for (int i = 0; i < vertices1.Length; i++)
            if (IsPointInPolygon(vertices2, vertices1[i]))
                verticesIntersect.Add(vertices1[i]);
        for (int i = 0; i < vertices2.Length; i++)
            if (IsPointInPolygon(vertices1, vertices2[i]))
                verticesIntersect.Add(vertices2[i]);


        //добавить все точки пересечения рёбер многоугольников в массив
        for (int i = 0; i < vertices1.Length; i++)
        {
            Vector2 start1 = vertices1[i];
            Vector2 end1 = vertices1[(i + 1) % vertices1.Length];

            for (int j = 0; j < vertices2.Length; j++)
            {
                Vector2 start2 = vertices2[j];
                Vector2 end2 = vertices2[(j + 1) % vertices2.Length];

                if (IsSegmentsIntersect(start1, end1, start2, end2, out Vector2 pointIntersect))
                    verticesIntersect.Add(pointIntersect);
            }
        }

        //вычислить площадь пересечения
        float square = GetSquare(verticesIntersect);

        return square;
    }

    //вычислить площадь многоугольника по множеству вершин
    public float GetSquare(HashSet<Vector2> vertices)
    {
        //если пересечение есть
        if (vertices.Count >= 3)
        {
            //отсортировать вершины
            List<Vector2> verticesSorted = SortVertices(vertices);

            //замкнуть видимую область плитки
            verticesSorted.Add(verticesSorted[0]);

            //рассчитать площадь этой области
            float square = 0;
            for (int i = 0; i < verticesSorted.Count - 1; i++)
            {
                Vector2 vertex = verticesSorted[i];
                Vector2 vertexNext = verticesSorted[i + 1];
                square += 0.5f * (vertex.x + vertexNext.x) * (vertex.y - vertexNext.y);
            }
            return square;
        }
        else
            return 0;
    }

    //определить, находится ли точка внутри многоугольника
    public bool IsPointInPolygon(Vector2[] vertices, Vector2 point)
    {
        bool result = true;
        for (int i = 0; i < vertices.Length; i++) //для каждого ребра
            if (!IsPointRightOfVector(vertices[i], vertices[(i + 1) % vertices.Length], point))
                result = false;
        return result;
    }

    //определить, справа ли от вектора находится точка
    private bool IsPointRightOfVector(Vector2 start, Vector2 end, Vector2 point)
    {
        return (end.x - start.x) * (point.y - start.y) - (point.x - start.x) * (end.y - start.y) <= _epsilonPointRightOfVector;
    }

    //отсортировать вершины многоугольника, чтобы он был выпуклым
    private List<Vector2> SortVertices(HashSet<Vector2> verticesSet)
    {
        List<Vector2> vertices = new List<Vector2>();

        //определить среднюю точку многоугольника
        float minX = 10000;
        float maxX = -10000;
        float minY = 10000;
        float maxY = -10000;
        foreach (Vector2 current in verticesSet)
        {
            if (current.x < minX) minX = current.x;
            if (current.x > maxX) maxX = current.x;
            if (current.y < minY) minY = current.y;
            if (current.y > maxY) maxY = current.y;
        }
        Vector2 middlePoint = new Vector2((minX + maxX) / 2, (minY + maxY) / 2);

        //определить углы отклонения каждой вершины от средней точки
        List<float> angles = new List<float>();
        foreach (Vector2 current in verticesSet)
        {
            float dx = current.x - middlePoint.x;
            float dy = current.y - middlePoint.y;

            //угол отклонения = argtg (dx / dy)
            float angle;
            if (dx == 0)
                angle = dy > 0 ? 90 : 270;
            else
            {
                angle = Mathf.Atan(dy / dx) * Mathf.Rad2Deg;

                //нормализовать угол [0; 360)
                if (dx < 0) //II или III четверть
                    angle += 180;
                else if (dx > 0 && dy < 0) //IV четверть
                    angle += 360;
            }

            vertices.Add(current);
            angles.Add(angle);
        }

        //отсортировать вершины в соответствии с углами по убыванию (направление обхода - по часовой стрелке)
        for (int j = 0; j < vertices.Count; j++)
            for (int i = 0; i < vertices.Count - j - 1; i++)
                if (angles[i] < angles[i + 1])
                {
                    //поменять местами вершины
                    Vector2 v = vertices[i];
                    vertices[i] = vertices[i + 1];
                    vertices[i + 1] = v;

                    //поменять местами углы
                    float f = angles[i];
                    angles[i] = angles[i + 1];
                    angles[i + 1] = f;
                }

        return vertices;
    }

    //определить, пересекаются ли 2 отрезка, и найти точку пересечения
    public bool IsSegmentsIntersect(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2, out Vector2 pointIntersect)
    {
        pointIntersect = Vector2.zero;

        // Параметрическое уравнение отрезка
        // x = x0 + vt
        // y = y0 + wt
        // где v = x1 - x0
        //     w = y1 - y0
        // при 0 <= t <= 1

        // Координаты направления вектора первого отрезка
        float v1 = end1.x - start1.x;
        float w1 = end1.y - start1.y;

        // Координаты направления вектора второго отрезка
        float v2 = end2.x - start2.x;
        float w2 = end2.y - start2.y;

        // ===== Частные случаи не пересечения =====

        // Отрезки не должны быть точками
        if ((v1 == 0 && w1 == 0) || (v2 == 0 && w2 == 0))
        {
            return false;
        }

        // Для вычисления параллельности отрезка необходимо сравнить направления их векторов

        // Вычисляем длины векторов
        float len1 = Mathf.Sqrt(v1 * v1 + w1 * w1);
        float len2 = Mathf.Sqrt(v2 * v2 + w2 * w2);

        // Нормализация векторов - создание единичного вектора направления
        float x1 = v1 / len1;
        float y1 = w1 / len1;
        float x2 = v2 / len2;
        float y2 = w2 / len2;

        // Проверка на совпадение
        if (start1.x == start2.x && start1.y == start2.y && end1.x == end2.x && end1.y == end2.y)
        {
            return false;
        }

        // Проверка на параллельность
        if (Mathf.Abs(x1 - x2) < _epsilonSegmentsIntersect && Mathf.Abs(y1 - y2) < _epsilonSegmentsIntersect)
        {
            return false;
        }

        // ===== Вычисление точки пересечения =====

        // Проверка факта пересечения
        // x = start2.x + v2t2
        // y = start2.y + w2t2

        // start1.x + vt = start2.x + v2t2 => vt = start2.x - start1.x + v2t2 =>
        // t = (start2.x - start1.x + v2t2) / v - (у.1) соотношение t-параметров
        //
        // Вычисление одного параметра с заменой соотношением другого
        // start1.y + wt = start2.y + w2t2 => wt = start2.y - start1.y + w2t2 => t = (start2.y - start1.y + w2t2) / w
        // (start2.x - start1.x + v2t2) / v = (start2.y - start1.y + w2t2) / w =>
        // (start2.x - start1.x + v2t2) * w = (start2.y - start1.y + w2t2) * v =>
        // w * start2.x - w * start1.x + w * v2t2 = v * start2.y - v * start1.y + v * w2t2 =>
        // w * v2t2 - v * w2t2 = -w * start2.x + w * start1.x + v * start2.y - v * start1.y =>
        // (w * v2 - v * w2) * t2 = -w * start2.x + w * start1.x + v * start2.y - v * start1.y =>
        // t2 = (-w * start2.x + w * start1.x + v * start2.y - v * start1.y) / (w * v2 - v * w2) - (у.2)
        float t2 = (-w1 * start2.x + w1 * start1.x + v1 * start2.y - v1 * start1.y) / (w1 * v2 - v1 * w2);

        // t = (start2.x - start1.x + v2t2) / v - (у.1)
        float t1 = v1 != 0
            ? (start2.x - start1.x + v2 * t2) / v1
            : (start2.y - start1.y + w2 * t2) / w1;

        // Если один из параметров является NaN, то пересечения нет
        if (float.IsNaN(t1) || float.IsNaN(t2))
        {
            return false;
        }

        // Если один из параметров не входит в интервал (0; 1) то пересечения нет
        if (t1 <= _epsilonSegmentsIntersect || t1 >= 1 - _epsilonSegmentsIntersect || t2 <= _epsilonSegmentsIntersect || t2 >= 1 - _epsilonSegmentsIntersect)
        {
            return false;
        }

        // Координаты точки пересечения
        pointIntersect = new Vector2(start2.x + v2 * t2, start2.y + w2 * t2);

        return true;
    }
}
