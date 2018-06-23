using System;

namespace SpringHello.utillty
{
    public class Utillty
    {
        public static int GetInt32Number()
        {
            int number = 0;
            while (true)
            {
                try
                {
                    number = Int32.Parse(Console.ReadLine());
                    break;
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Please enter a number.");
                }
            }
            return number;
        }

        public static decimal GetDecimalNumber()
        {
            decimal number;
            while (true)
            {
                try
                {
                    number = Decimal.Parse(Console.ReadLine());
                    break;
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Please enter a number");
                }
            }
            return number;
        }
    }
}