using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Data;

namespace HelpClasses
{
    public class Statistic
    {
        public string Name = "";
        public decimal Mean { get; set; }
        public decimal Std  { get; set; }

        public Statistic(decimal mean, decimal standardDeviation)
        {
            Mean = mean;
            Std = standardDeviation;
        }
        public Statistic(decimal mean, decimal standardDeviation, string name)
        {
            Mean = mean;
            Std = standardDeviation;
            Name = name;
        }
    }

    public static class Statistics
    {

        public static double Mean(this List<double> values)
        {
            return values.Count == 0 ? 0 : values.Mean(0, values.Count);
        }

        public static double Mean(this List<double> values, int start, int end)
        {
            double s = 0;

            for (int i = start; i < end; i++)
            {
                s += values[i];
            }

            return s / (end - start);
        }

        public static double Variance(this List<double> values)
        {
            return values.Variance(values.Mean(), 0, values.Count);
        }

        public static double Variance(this List<double> values, double mean)
        {
            return values.Variance(mean, 0, values.Count);
        }

        public static double Variance(this List<double> values, double mean, int start, int end)
        {
            double variance = 0;

            for (int i = start; i < end; i++)
            {
                variance += Math.Pow((values[i] - mean), 2);
            }

            int n = end - start;
            if (start > 0) n -= 1;

            return variance / (n);
        }

        public static double StandardDeviation(this List<double> values)
        {
            return values.Count == 0 ? 0 : values.StandardDeviation(0, values.Count);
        }

        public static double StandardDeviation(this List<double> values, int start, int end)
        {
            double mean = values.Mean(start, end);
            double variance = values.Variance(mean, start, end);

            return Math.Sqrt(variance);
        }
        public static decimal Mean(this List<decimal> values)
        {
            return values.Count == 0 ? 0 : values.Mean(0, values.Count);
        }

        public static decimal Mean(this List<decimal> values, int start, int end)
        {
            decimal s = 0;

            for (int i = start; i < end; i++)
            {
                s += values[i];
            }

            return s / (end - start);
        }

        public static decimal Variance(this List<decimal> values)
        {
            return values.Variance(values.Mean(), 0, values.Count);
        }

        public static decimal Variance(this List<decimal> values, decimal mean)
        {
            return values.Variance(mean, 0, values.Count);
        }

        public static decimal Variance(this List<decimal> values, decimal mean, int start, int end)
        {
            decimal variance = 0;

            for (int i = start; i < end; i++)
            {
                variance +=new decimal(Math.Pow((double) (values[i] - mean), 2));
            }

            int n = end - start;
            if (start > 0) n -= 1;

            return variance / (n);
        }

        public static decimal StandardDeviation(this List<decimal> values)
        {
            return values.Count == 0 ? 0 : values.StandardDeviation(0, values.Count);
        }

        public static decimal StandardDeviation(this List<decimal> values, int start, int end)
        {
            decimal mean = values.Mean(start, end);
            decimal variance = values.Variance(mean, start, end);

            return new decimal(Math.Sqrt((double)variance));
        }

    }
}
