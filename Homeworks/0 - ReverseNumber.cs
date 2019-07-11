using System;

namespace wxr
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            int n = 123;
            int temp = ReverseNumber(n); //321
            Console.WriteLine(temp);

            n = -123;
            temp = ReverseNumber(n); //-321
            Console.WriteLine(temp);

            n = 1023456789;
            temp = ReverseNumber(n); //overflow
            Console.WriteLine(temp);
        }

        static int ReverseNumber(int score)
        {
            bool isNegative = false;
            if (score < 0)
            {
                isNegative = true;
                score = -score;
            }

            string str = score.ToString().Reverse();
            string maxInt = int.MaxValue.ToString();

            if ((str.Length > maxInt.Length)
                || (str.Length == maxInt.Length && string.Compare(str, maxInt) > 0))
            {
                throw OverflowException();
            }

            score = int.Parse(str);

            if (isNegative)
            {
                score = -score;
            }

            return score;
        }

        private static Exception OverflowException()
        {
            throw new NotImplementedException();
        }
    }

    static class ExtendedMethods
    {
        public static string Reverse(this string str)
        {
            char[] charArray = str.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
