using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class TestIsSegmentsIntersect
    {
        private Main main = new Main();

        private void AssertResults(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2, bool expectedIntersect, Vector2? expectedPointIntersect = null)
        {
            Vector2 actualPointIntersect;

            Assert.AreEqual(expectedIntersect, main.IsSegmentsIntersect(start1, end1, start2, end2, out actualPointIntersect));
            if (expectedIntersect)
                Assert.AreEqual(expectedPointIntersect, actualPointIntersect);

            Assert.AreEqual(expectedIntersect, main.IsSegmentsIntersect(start2, end2, start1, end1, out actualPointIntersect));
            if (expectedIntersect)
                Assert.AreEqual(expectedPointIntersect, actualPointIntersect);
        }

        [Test]
        public void SegmentsIntersect() //отрезки пересекаются
        {
            Vector2 start1 = new Vector2(0, 0);
            Vector2 end1 = new Vector2(10, 0);
            Vector2 start2 = new Vector2(0, -5);
            Vector2 end2 = new Vector2(10, 5);
            Vector2 expectedPointIntersect = new Vector2(5, 0);
            AssertResults(start1, end1, start2, end2, true, expectedPointIntersect);
        }

        [Test]
        public void SegmentsIntersectRightAngle() //отрезки пересекаются под прямым углом
        {
            Vector2 start1 = new Vector2(0, 0);
            Vector2 end1 = new Vector2(10, 0);
            Vector2 start2 = new Vector2(5, -5);
            Vector2 end2 = new Vector2(5, 5);
            Vector2 expectedPointIntersect = new Vector2(5, 0);
            AssertResults(start1, end1, start2, end2, true, expectedPointIntersect);
        }

        [Test]
        public void SegmentsNotIntersect() //отрезки не пересекаются
        {
            Vector2 start1 = new Vector2(0, 30);
            Vector2 end1 = new Vector2(10, 30);
            Vector2 start2 = new Vector2(0, -5);
            Vector2 end2 = new Vector2(10, 5);
            AssertResults(start1, end1, start2, end2, false);
        }

        [Test]
        public void SegmentsAreParallel() //отрезки параллельны
        {
            Vector2 start1 = new Vector2(0, 30);
            Vector2 end1 = new Vector2(10, 40);
            Vector2 start2 = new Vector2(0, -5);
            Vector2 end2 = new Vector2(10, 5);
            AssertResults(start1, end1, start2, end2, false);
        }

        [Test]
        public void SegmentsAtOneLine() //отрезки находятся на одной прямой
        {
            Vector2 start1 = new Vector2(0, 0);
            Vector2 end1 = new Vector2(10, 0);
            Vector2 start2 = new Vector2(20, 0);
            Vector2 end2 = new Vector2(30, 0);
            AssertResults(start1, end1, start2, end2, false);
        }

        [Test]
        public void SegmentInOther() //один отрезок лежит во втором
        {
            Vector2 start1 = new Vector2(0, 0);
            Vector2 end1 = new Vector2(10, 0);
            Vector2 start2 = new Vector2(3, 0);
            Vector2 end2 = new Vector2(5, 0);
            AssertResults(start1, end1, start2, end2, false);
        }

        [Test]
        public void SegmentInOtherReversed() //один отрезок лежит во втором, координаты второго поменяты местами
        {
            Vector2 start1 = new Vector2(0, 0);
            Vector2 end1 = new Vector2(10, 0);
            Vector2 start2 = new Vector2(5, 0);
            Vector2 end2 = new Vector2(3, 0);
            AssertResults(start1, end1, start2, end2, false);
        }

        [Test]
        public void SegmentIntersectLineOfOther() //отрезок пересекает прямую, содержащую другой отрезок
        {
            Vector2 start1 = new Vector2(5, -10);
            Vector2 end1 = new Vector2(5, -5);
            Vector2 start2 = new Vector2(0, 0);
            Vector2 end2 = new Vector2(10, 0);
            AssertResults(start1, end1, start2, end2, false);
        }

        [Test]
        public void SegmentEndAtOtherMiddle() //конец одного отрезка лежит на середине другого
        {
            Vector2 start1 = new Vector2(0, 0);
            Vector2 end1 = new Vector2(10, 0);
            Vector2 start2 = new Vector2(3, 0);
            Vector2 end2 = new Vector2(5, 5);
            AssertResults(start1, end1, start2, end2, false);
        }

        [Test]
        public void SegmentsHasOnePoint() //отрезки имеют одну общую точку
        {
            Vector2 start1 = new Vector2(0, 0);
            Vector2 end1 = new Vector2(10, 0);
            Vector2 start2 = new Vector2(10, 0);
            Vector2 end2 = new Vector2(10, 5);
            AssertResults(start1, end1, start2, end2, false);
        }

        [Test]
        public void SegmentsAreEqual() //отрезки полностью совпадают
        {
            Vector2 start = new Vector2(0, 0);
            Vector2 end = new Vector2(10, 0);
            AssertResults(start, end, start, end, false);
        }

        [Test]
        public void SegmentsAreEqualReversed() //отрезки полностью совпадают, координаты второго поменяты местами
        {
            Vector2 start1 = new Vector2(0, 0);
            Vector2 end1 = new Vector2(10, 0);
            Vector2 start2 = new Vector2(10, 0);
            Vector2 end2 = new Vector2(0, 0);
            AssertResults(start1, end1, start2, end2, false);
        }
    }
}
