using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelLINQ
{
    class Program
    {
        static void Main(string[] args)
        {

            //ParallelLINQ();
            //DeferredQueryExecution();
            //ControllingConcurrency();
            //HandlingExceptionsPLINQ();
            //CancellingPLINQ();
            //SettingUpMergeOptions();
            //CustomAggregation();
            GeneralParallelRanges();

            Console.ReadKey();
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
            int[] sourceData = new int[20];
            for (int i = 0; i < sourceData.Length; i++)
            {
                sourceData[i] = i + 1;
            }
            var resultData = sourceData.AsParallel().Where(item => item % 2 == 0).Select(item =>
            {
                Console.WriteLine("Processing Value {0}...", item);
                return Math.Pow(item, 2);
            });

            Console.WriteLine("Checking Deffered Query By Sleeping for 1 second.....");
            Thread.Sleep(1000);
            Console.WriteLine("Sleep Complete");
            foreach (var result in resultData)
            {
                Console.WriteLine("Resutl = " + result);
            }
            Console.WriteLine();


        }

        /// <summary>
        /// Listing 6-11, 6-12, 6-13
        /// </summary>
        static void ControllingConcurrency()
        {
            int[] sourceData = new int[15];
            for (int i = 0; i < sourceData.Length; i++)
            {
                sourceData[i] = i + 1;
            }


            //Forcing Parallelism
            var result1 = sourceData
                .AsParallel()
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .Where(item => item % 2 == 0)
                .Select(item => item);

            //Limiting Parallelism
            var result2 = sourceData
                .AsParallel()
                .WithDegreeOfParallelism(2)
                .Select(item => Math.Pow(item, 2));

            //Forcing Sequential Execution
            var result3 = sourceData
                .AsParallel()
                .WithDegreeOfParallelism(2)
                .Where(item => item % 2 == 0)
                .Select(item => Math.Pow(item, 2))
                .AsSequential()
                .Select(item => item * item);

            Console.WriteLine("Result 1");
            foreach (var result in result1)
            {
                Console.WriteLine(result);
            }
            Console.WriteLine();

            Console.WriteLine("Result 2");
            foreach (var result in result2)
            {
                Console.WriteLine(result);
            }
            Console.WriteLine();

            Console.WriteLine("Result 3");
            foreach (var result in result3)
            {
                Console.WriteLine(result);
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Listing 6-14
        /// </summary>
        static void HandlingExceptionsPLINQ()
        {
            int[] sourceData = new int[1000];
            for (int i = 0; i < sourceData.Length; i++)
            {
                sourceData[i] = i + 1;
            }

            var result1 = sourceData
                .AsParallel()
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .WithDegreeOfParallelism(3)
                .Where(item => item % 2 == 0)
                .Select(item =>
                {
                    if (item == 86)
                    {
                        Console.WriteLine("Throwwwwwwwwwwwwwwwwwwwwwing....");
                        throw new NullReferenceException();
                    }
                    return item;
                });

            try
            {
                foreach (var result in result1)
                {
                    Console.WriteLine(result);
                }

            }
            catch (AggregateException aggException)
            {
                aggException.Handle(ex =>
                {
                    Console.WriteLine("Exception Handled " + ex.ToString());
                    return true;
                });
            }
        }

        /// <summary>
        /// Listing 6-15
        /// </summary>
        static void CancellingPLINQ()
        {
            int[] sourceData = new int[10000];
            for (int i = 0; i < sourceData.Length; i++)
            {
                sourceData[i] = i + 1;
            }

            CancellationTokenSource source = new CancellationTokenSource();

            var result1 = sourceData
                .AsParallel()
                .WithCancellation(source.Token)
                .Where(item => item % 2 == 0);

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2000);
                source.Cancel();
                Console.WriteLine("Token Cancelled!!");
            });

            try
            {
                foreach (var result in result1)
                {
                    Console.WriteLine(result);
                }
            }
            catch (OperationCanceledException opEx)
            {
                Console.WriteLine("Operation Cancelled!!!" + opEx.Message);
            }
        }

        /// <summary>
        /// Listing 6-16
        /// </summary>
        static void SettingUpMergeOptions()
        {
            int[] sourceData = new int[10];
            for (int i = 0; i < sourceData.Length; i++)
            {
                sourceData[i] = i + 1;
            }

            var result = sourceData.AsParallel().WithMergeOptions(ParallelMergeOptions.FullyBuffered).Select(item =>
            {
                Console.WriteLine("Processing Item: {0}", item);
                return Math.Pow(item, 2);
            });


            foreach (var value in result)
            {
                Console.WriteLine("Result:{0}", value);
            }
        }

        /// <summary>
        /// Listing 6-17, 6-18
        /// </summary>
        static void CustomPartitioning()
        {
            //Create a class for static partitioner 
            //Consume that class and run as parallel on linq queries
        }

        /// <summary>
        /// Listing 6-19
        /// </summary>
        static void CustomAggregation()
        {
            int[] sourceData = new int[10];
            for (int i = 0; i < sourceData.Length; i++)
            {
                sourceData[i] = i + 1;
            }
            var resultData = sourceData.AsParallel().Aggregate(
                0.0,
                (subTotal, item) => subTotal += Math.Pow(item, 2),
                (total, subTotal) => total += subTotal,
                total => total / 2);

            Console.WriteLine("Result = {0}", resultData);
        }

        /// <summary>
        /// Listing 6-20
        /// </summary>
        static void GeneralParallelRanges()
        {
            int[] sourceData = new int[10];
            for (int i = 0; i < sourceData.Length; i++)
            {
                sourceData[i] = i + 1;
            }

            IEnumerable<int> resultRange = ParallelEnumerable.Range(0, 10).Where(item => item % 2 == 0).Select(item => item);

            IEnumerable<double> resultRepeat = ParallelEnumerable.Repeat(10, 10).Where(item => item % 2 == 0).Select(item => Math.Pow(item, 2));


            Console.WriteLine("Range Result!!");
            foreach (int x in resultRange)
            {
                Console.WriteLine(x);
            }

            Console.WriteLine("\nRepeat Result!!");
            foreach(double x in resultRepeat)
            {
                Console.WriteLine(x);
            }
        }

    }
}
