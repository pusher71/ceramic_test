using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class TestGetIntersectSquare
    {
        private Vector2[] sharedArea1 = new Vector2[4]
        {
            new Vector2(-15, -15),
            new Vector2(-15, 15),
            new Vector2(15, 15),
            new Vector2(15, -15)
        };

        private void AssertResults(Vector2[] area1, Vector2[] area2, float expectedSquare)
        {
            Assert.AreEqual(expectedSquare, GeometryUtils.GetIntersectSquare(area1, area2));
            Assert.AreEqual(expectedSquare, GeometryUtils.GetIntersectSquare(area2, area1));
        }

        [Test]
        public void TestOffset() //второй многоугольник сдвинут вверх и направо
        {
            Vector2[] area2 = new Vector2[4]
            {
                new Vector2(0, 0),
                new Vector2(0, 30),
                new Vector2(30, 30),
                new Vector2(30, 0)
            };
            float expectedSquare = 225;
            AssertResults(sharedArea1, area2, expectedSquare);
        }

        [Test]
        public void TestPlus() //многоугольники образуют форму плюса
        {
            Vector2[] area2 = new Vector2[4]
            {
                new Vector2(-30, -5),
                new Vector2(-30, 5),
                new Vector2(30, 5),
                new Vector2(30, -5)
            };
            float expectedSquare = 300;
            AssertResults(sharedArea1, area2, expectedSquare);
        }

        [Test]
        public void Test8Vertex() //пересечение - восьмиугольник
        {
            Vector2[] area2 = new Vector2[4]
            {
                new Vector2(0, 20),
                new Vector2(20, 0),
                new Vector2(0, -20),
                new Vector2(-20, 0)
            };
            float expectedSquare = 700;
            AssertResults(sharedArea1, area2, expectedSquare);
        }

        [Test]
        public void TestCornersOnEdges() //вершины одного многоугольника лежат на рёбрах другого
        {
            Vector2[] area2 = new Vector2[4]
            {
                new Vector2(15, 16),
                new Vector2(16, 15),
                new Vector2(15, 14),
                new Vector2(14, 15)
            };
            float expectedSquare = 0.5f;
            AssertResults(sharedArea1, area2, expectedSquare);
        }

        [Test]
        public void TestEqual() //многоугольники совпадают
        {
            Vector2[] area2 = new Vector2[4]
            {
                new Vector2(-15, 15),
                new Vector2(15, 15),
                new Vector2(15, -15),
                new Vector2(-15, -15)
            };
            float expectedSquare = 900;
            AssertResults(sharedArea1, area2, expectedSquare);
        }

        [Test]
        public void Test1In2() //первый многоугольник внутри второго
        {
            Vector2[] area2 = new Vector2[4]
            {
                new Vector2(0, 200),
                new Vector2(200, 0),
                new Vector2(0, -200),
                new Vector2(-200, 0)
            };
            float expectedSquare = 900;
            AssertResults(sharedArea1, area2, expectedSquare);
        }

        [Test]
        public void Test2In1() //второй многоугольник внутри первого
        {
            Vector2[] area2 = new Vector2[4]
            {
                new Vector2(0, 2),
                new Vector2(2, 0),
                new Vector2(0, -2),
                new Vector2(-2, 0)
            };
            float expectedSquare = 8;
            AssertResults(sharedArea1, area2, expectedSquare);
        }

        [Test]
        public void Test2In1TouchesEdges() //второй многоугольник внутри первого и касается рёбер первого
        {
            Vector2[] area2 = new Vector2[4]
            {
                new Vector2(0, 15),
                new Vector2(15, 0),
                new Vector2(0, -15),
                new Vector2(-15, 0)
            };
            float expectedSquare = 450;
            AssertResults(sharedArea1, area2, expectedSquare);
        }

        [Test]
        public void TestNotIntersect() //многоугольники удалены друг от друга
        {
            Vector2[] area2 = new Vector2[4]
            {
                new Vector2(200, 200),
                new Vector2(200, 201),
                new Vector2(201, 201),
                new Vector2(201, 200)
            };
            float expectedSquare = 0;
            AssertResults(sharedArea1, area2, expectedSquare);
        }

        [Test]
        public void TestIntersectByEdge() //у многоугольников одно общее ребро
        {
            Vector2[] area2 = new Vector2[4]
            {
                new Vector2(-15, 15),
                new Vector2(-15, 45),
                new Vector2(15, 45),
                new Vector2(15, 15)
            };
            float expectedSquare = 0;
            AssertResults(sharedArea1, area2, expectedSquare);
        }

        [Test]
        public void TestIntersectByPoint() //у многоугольников одна общая точка
        {
            Vector2[] area2 = new Vector2[4]
            {
                new Vector2(15, 15),
                new Vector2(15, 45),
                new Vector2(45, 45),
                new Vector2(45, 15)
            };
            float expectedSquare = 0;
            AssertResults(sharedArea1, area2, expectedSquare);
        }
    }
}
