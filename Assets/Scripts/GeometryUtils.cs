using System.Collections.Generic;
using UnityEngine;

public static class GeometryUtils
{
    //параметры точности
    private const float EPSILON_POINT_TO_VECTOR = 0.05f;
    private const float EPSILON_SEGMENTS_INTERSECT = 0.000001f;

    //вычислить площадь пересечения двух многоугольников
    public static float GetIntersectSquare(Vector2[] vertices1, Vector2[] vertices2)
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
    public static float GetSquare(HashSet<Vector2> vertices)
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
    public static bool IsPointInPolygon(Vector2[] vertices, Vector2 point)
    {
        bool result = true;
        for (int i = 0; i < vertices.Length; i++) //для каждого ребра
            if (!IsPointRightOfVector(vertices[i], vertices[(i + 1) % vertices.Length], point))
                result = false;
        return result;
    }

    //определить, справа ли от вектора находится точка
    private static bool IsPointRightOfVector(Vector2 start, Vector2 end, Vector2 point)
    {
        return (end.x - start.x) * (point.y - start.y) - (point.x - start.x) * (end.y - start.y) <= EPSILON_POINT_TO_VECTOR;
    }

    //отсортировать вершины многоугольника, чтобы он был выпуклым
    private static List<Vector2> SortVertices(HashSet<Vector2> verticesSet)
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
    public static bool IsSegmentsIntersect(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2, out Vector2 pointIntersect)
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
        if (Mathf.Abs(x1 - x2) < EPSILON_SEGMENTS_INTERSECT && Mathf.Abs(y1 - y2) < EPSILON_SEGMENTS_INTERSECT)
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
        if (t1 <= EPSILON_SEGMENTS_INTERSECT || t1 >= 1 - EPSILON_SEGMENTS_INTERSECT || t2 <= EPSILON_SEGMENTS_INTERSECT || t2 >= 1 - EPSILON_SEGMENTS_INTERSECT)
        {
            return false;
        }

        // Координаты точки пересечения
        pointIntersect = new Vector2(start2.x + v2 * t2, start2.y + w2 * t2);

        return true;
    }
}
