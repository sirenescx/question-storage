using System;
using Fare;

namespace QuestionStorage.Utils
{
    public class QRandom : Random
    {
        public string StringFromRegex(string pattern) => new Xeger(pattern).Generate();

        public double NextDouble(int upperBound) => Next(upperBound) + NextDouble();

        public double NextDouble(int lowerBound, int upperBound) => Next(lowerBound, upperBound) + NextDouble();

        private double GetPrecision(int precision) => Math.Pow(10, precision);
        
        public double NextDouble(double lowerBound, double upperBound) =>
            NextDouble() * (upperBound - lowerBound) + lowerBound;

        // public double NextDouble(int lowerBound, int upperBound, int precision) => 
        //     Next(lowerBound, upperBound) * GetPrecision(precision) / GetPrecision(precision);

        public double NextDouble(double lowerBound, double upperBound, int digits)
        {
            var precision = Math.Pow(10, digits);
            return (int)(NextDouble(lowerBound, upperBound) * precision) % (precision * 10) / precision;
        }
    }
}