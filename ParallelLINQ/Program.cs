using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelLINQ
{
    class Program
    {
        static void Main(string[] args)
        {

            ParallelLINQ();

        }


        /// <summary>
        /// Listing 6-1, 6-2, 6-3, 6-4, 6-5, 6-6, 6-7, 6-8
        /// </summary>
        static void ParallelLINQ()
        {
            int[] sourceData = new int[20];

            for (int i = 0; i < sourceData.Length; i++)
            {
                sourceData[i] = i + 1;
            }

            //Using Basic PLINQ
            IEnumerable<int> resultData =
                from item in sourceData.AsParallel()
                where item % 2 == 0
                select item;
            foreach (int result in resultData)
                Console.Write(result + ",");
            Console.WriteLine("\n---------------------------------");

            //Using PLINQ with extension methods
            IEnumerable<int> resultData2 =
                sourceData.AsParallel()
                .Where(item => item % 2 == 0)
                .Select(item => item);
            foreach (int result in resultData2)
                Console.Write(result + ",");
            Console.WriteLine("\n---------------------------------");


            //Making a Filtering Query with keywords
            IEnumerable<double> squaredResult =
                from item in sourceData.AsParallel()
                where item % 2 == 0
                select Math.Pow(item, 2);
            foreach (double result in squaredResult)
                Console.Write(result + ",");
            Console.WriteLine("\n---------------------------------");

            //Making a Filtering Query with Extension Methods
            IEnumerable<double> squaredResult2 =
                sourceData.AsParallel()
                .Where(item => item % 2 == 0)
                .Select(item => Math.Pow(item, 2));
            foreach (double result in squaredResult2)
                Console.Write(result + ",");
            Console.WriteLine("\n---------------------------------");


            //Preserving Order in a Parallel Query
            IEnumerable<int> resultOrdered =
                sourceData.AsParallel().AsOrdered()
                .Where(item => item % 2 == 0)
                .Select(item => item);
            foreach (double result in resultOrdered)
                Console.Write(result + ",");
            Console.WriteLine("\n---------------------------------");


            #region PreservingOrder

            sourceData = new int[25];
            for (int i = 0; i < sourceData.Length; i++)
            {
                sourceData[i] = i + 1;
            }

            int index = 0;
            IEnumerable<int> resultData5 =
                sourceData.AsParallel().AsOrdered()
                .Where(item => item % 2 == 0)
                .Select(item => item);

            foreach (int result in resultData5)
                Console.WriteLine("Bad Result:{0} -- index:{1}", result, index++);
            Console.WriteLine();

            index = 0;
            var resultData4 =
                sourceData.AsParallel()
                .Where(item => item % 2 == 0)
                .Select(item => new { source = item, index = index++ });
            foreach (var result in resultData4)
                Console.WriteLine("Good Result:{0} -- index:{1}", result.source, result.index);
            Console.WriteLine();
            #endregion PreservingOrder

            //AsOrdered and AsUnordered together
            var resultData3 = sourceData.AsParallel().AsOrdered()
                .Take(10).AsUnordered().Select(item => new { source = item, result = Math.Pow(item, 2) });
            foreach (var result in resultData3)
                Console.WriteLine("Item:{0} -- Result:{1}", result.source, result.result);
            Console.WriteLine();

            //Using For All extension method
            sourceData.AsParallel().Where(item => item % 2 == 0)
                .ForAll(item => Console.WriteLine("Item:{0} -- Result:{1}", item, Math.Pow(item, 2)));

        }

        /// <summary>
        /// Listing 6-9, 6-10
        /// </summary>
        static void DeferredQueryExecution()
        {

        }

        /// <summary>
        /// Listing 6-11, 6-12, 6-13
        /// </summary>
        static void ControllingConcurrency()
        {

        }

        /// <summary>
        /// Listing 6-14
        /// </summary>
        static void HandlingExceptionsPLINQ()
        {

        }
    }
}
