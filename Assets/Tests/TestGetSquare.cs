using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class TestGetSquare
    {
        private void AssertResults(HashSet<Vector2> area, float expectedSquare)
        {
            Assert.AreEqual(expectedSquare, GeometryUtils.GetSquare(area));
        }

        [Test]
        public void RectangleNormalized() //прямоугольник с отсортированными вершинами
        {
            HashSet<Vector2> area = new HashSet<Vector2>
            {
                new Vector2(-15, -15),
                new Vector2(-15, 15),
                new Vector2(15, 15),
                new Vector2(15, -15)
            };
            float expectedSquare = 900;
            AssertResults(area, expectedSquare);
        }

        [Test]
        public void RectangleNotNormalized() //прямоугольник с неотсортированными вершинами
        {
            HashSet<Vector2> area = new HashSet<Vector2>
            {
                new Vector2(-15, -15),
                new Vector2(15, 15),
                new Vector2(-15, 15),
                new Vector2(15, -15)
            };
            float expectedSquare = 900;
            AssertResults(area, expectedSquare);
        }

        [Test]
        public void Triangle() //треугольник
        {
            HashSet<Vector2> area = new HashSet<Vector2>
            {
                new Vector2(0, 0),
                new Vector2(0, 10),
                new Vector2(15, 0)
            };
            float expectedSquare = 75;
            AssertResults(area, expectedSquare);
        }

        [Test]
        public void TriangleNegativeCoordinates() //треугольник с отрицательными координатами
        {
            HashSet<Vector2> area = new HashSet<Vector2>
            {
                new Vector2(0, -10),
                new Vector2(-15, 0),
                new Vector2(0, 0)
            };
            float expectedSquare = 75;
            AssertResults(area, expectedSquare);
        }

        [Test]
        public void TriangleFlipped() //треугольник с другим расположением
        {
            HashSet<Vector2> area = new HashSet<Vector2>
            {
                new Vector2(15, 14),
                new Vector2(14, 15),
                new Vector2(15, 15)
            };
            float expectedSquare = 0.5f;
            AssertResults(area, expectedSquare);
        }
    }
}
