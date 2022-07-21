using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class TestIsPointInPolygon
    {
        private Main main = new Main();
        private Vector2[] sharedRectangle = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(0, 10),
            new Vector2(15, 10),
            new Vector2(15, 0)
        };
        private Vector2[] sharedTriangle = new Vector2[3]
        {
            new Vector2(0, 0),
            new Vector2(0, 10),
            new Vector2(15, 0)
        };

        private void AssertResults(Vector2[] vertices, Vector2 point, bool expectedPointInPolygon)
        {
            Assert.AreEqual(expectedPointInPolygon, main.IsPointInPolygon(vertices, point));
        }

        [Test]
        public void PointIn() //точка внутри
        {
            Vector2 point = new Vector2(3, 3);
            AssertResults(sharedRectangle, point, true);
        }

        [Test]
        public void PointOut() //точка снаружи
        {
            Vector2 point = new Vector2(13, 26);
            AssertResults(sharedRectangle, point, false);
        }

        [Test]
        public void PointOnEdge() //точка на ребре
        {
            Vector2 point = new Vector2(15, 7);
            AssertResults(sharedRectangle, point, true);
        }

        [Test]
        public void PointOnCorner() //точка на углу
        {
            Vector2 point = new Vector2(15, 10);
            AssertResults(sharedRectangle, point, true);
        }

        [Test]
        public void PointOnImaginaryEdgeExtension() //точка на продолжении ребра
        {
            Vector2 point = new Vector2(15, 30);
            AssertResults(sharedRectangle, point, false);
        }

        [Test]
        public void PointInTriangle() //точка внутри треугольника
        {
            Vector2 point = new Vector2(3, 3);
            AssertResults(sharedTriangle, point, true);
        }

        [Test]
        public void PointOutTriangle() //точка снаружи треугольника
        {
            Vector2 point = new Vector2(14, 9);
            AssertResults(sharedTriangle, point, false);
        }
    }
}
