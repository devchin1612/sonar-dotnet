﻿namespace Net7.features
{
    internal class CheckedAndUncheckedUserDefinedOperators
    {
        public void Method()
        {
            int a = int.MaxValue;
            int b = 3;

            Console.WriteLine(unchecked(a + b));  // output: -2147483646
            try
            {
                int d = checked(a + b);
            }
            catch (OverflowException)
            {
                Console.WriteLine($"Overflow occurred when adding {a} to {b}.");
            }
        }
    }

    internal record struct Point(int X, int Y)
    {
        public static Point operator checked +(Point left, Point right)
        {
            checked
            {
                return new Point(left.X + right.X, left.Y + right.Y);
            }
        }

        public static Point operator +(Point left, Point right)
        {
            return new Point(left.X + right.X, left.Y + right.Y);
        }
    }
}
