using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelLoops
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Parallel Loops Listing");

            //ParallelLoop();
            //ParallelInvoke();
            //ParallelLoopBreakStop();
            //CancellingParallelLoop();
            //ParallelLoopUsingTLS();
            //SynchronizingParallelLoop();
            ParallelLoopBodyAnalysis();
            ChunkingPartitionStrategy();

            Console.WriteLine("Program has Ended, Press Enter to Exit");
            Console.ReadKey();
        }

        /// <summary>
        /// Listing 5-1, 5-3, 5-4, 5-5, 5-6
        /// </summary>
        static void ParallelLoop()
        {
            int[] data = new int[10];
            double[] result = new double[10];

            for (int i = 0; i < 10; i++)
            {
                data[i] = i;
            }

            Parallel.For(0, data.Length, (index) =>
            {
                result[index] = Math.Pow(data[index], 2);
                Console.WriteLine("index:{0} - result:{1} (performed by {2})", index, result[index], Task.CurrentId);
            });

            Console.WriteLine("-------------------------------------");
            Parallel.ForEach(data, (intIndex) => { Console.WriteLine("Value:{0} - Square:{1} (Performed By:{2})", intIndex, Math.Pow(intIndex, 2), Task.CurrentId); });

            Console.WriteLine("-------------------------------------");
            Parallel.ForEach(SteppedIterator(0, 10, 2), (intIndex) => { Console.WriteLine("Value:{0} - Square:{1} (Performed By:{2})", intIndex, Math.Pow(intIndex, 2), Task.CurrentId); });

            Console.WriteLine("-------------------------------------");
            //Setting Parallel Options
            ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 3 };
            Parallel.ForEach(data, (intIndex) => { Console.WriteLine("Value:{0} - Square:{1} (Performed By:{2})", intIndex, Math.Pow(intIndex, 2), Task.CurrentId); });

        }

        //Stepped Iterator
        static IEnumerable<int> SteppedIterator(int startIndex, int endIndex, int stepSize)
        {
            for (int i = startIndex; i < endIndex; i += stepSize)
            {
                yield return i;
            }
        }

        /// <summary>
        /// Listing 5-2
        /// </summary>
        static void ParallelInvoke()
        {
            Action[] actionArray = new Action[3];
            actionArray[0] = new Action(() => { Console.WriteLine("Action Called of Id:0"); });
            actionArray[1] = new Action(() => { Console.WriteLine("Action Called of Id:1"); });
            actionArray[2] = new Action(() => { Console.WriteLine("Action Called of Id:2"); });
            Parallel.Invoke(actionArray);


        }

        /// <summary>
        /// Listing 5-7, 5-8, 5-9
        /// </summary>
        static void ParallelLoopBreakStop()
        {
            List<string> strings = new List<string>() { "abid", "hasan", "zeerak", "batool", "fatima", "kumail", "shami" };

            Parallel.ForEach(strings, (string item, ParallelLoopState parallelLoopState) =>
            {
                if (item.Contains("o"))
                {
                    Console.WriteLine("Hit: {0}", item);
                    parallelLoopState.Break();
                }
                else
                {
                    Console.WriteLine("Miss: {0}", item);
                }
            });

            Console.WriteLine("--------------------------------------------------");

            ParallelLoopResult loopResult = Parallel.ForEach(strings, (string item, ParallelLoopState state) =>
            {
                if (item.Contains("o"))
                {
                    Console.WriteLine("Hit: {0}", item);
                    state.Stop();
                }
                else
                {
                    Console.WriteLine("Miss: {0}", item);
                }
            });

            Console.WriteLine("IsCompleted {0}", loopResult.IsCompleted);
            Console.WriteLine("Break value {0}", loopResult.LowestBreakIteration);
        }

        /// <summary>
        /// Listing 5-10 Cancelling a Parallel Loop
        /// </summary>
        static void CancellingParallelLoop()
        {
            CancellationTokenSource source = new CancellationTokenSource();

            ParallelOptions options = new ParallelOptions() { CancellationToken = source.Token };
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2000);
                source.Cancel();
            });
            try
            {
                Parallel.For(1, Int32.MaxValue, options, index =>
                {
                    double result = Math.Pow(index, 3);
                    Console.WriteLine("{0} Cube is equal to :{1}", index, result);
                    Thread.Sleep(500);
                });

            }
            catch (OperationCanceledException oex)
            {
                Console.WriteLine("Canceled Exception {0}", oex.Message);
            }

            Console.WriteLine("Press Enter to cancel task");
            Console.ReadKey();

            source.Cancel();
            Console.WriteLine("Cancel Called");
        }

        /// <summary>
        /// Listing 5-11 Using Thread Local Storage
        /// </summary>
        static void ParallelLoopUsingTLS()
        {
            int total = 0;

            Parallel.For(0, 1000, () => 0,
                (int index, ParallelLoopState state, int tIsValue) => { tIsValue += index; return tIsValue; },
                value => Interlocked.Add(ref total, value));

            Console.WriteLine("Total = {0}", total);

            int newTotal = 0;
            object lockObj = new object();
            Parallel.For(0, 1000, () => 0,
                (int index, ParallelLoopState state, int tIsValue) => { tIsValue += index; return tIsValue; },
                value => { lock (lockObj) { newTotal += value; } });

            Console.WriteLine("New Total: {0}", newTotal);
        }

        /// <summary>
        /// Listing 5-13
        /// </summary>
        static void SynchronizingParallelLoop()
        {
            Account account = new Account();
            int itemsPerMonth = 100;

            Account[] transactions = new Account[12 * itemsPerMonth];
            for (int i = 0; i < 12 * itemsPerMonth; i++)
            {
                transactions[i] = new Account() { Amount = 10 };
            }
            int[] monthlyBalance = new int[12];
            for (int currentMonth = 0; currentMonth < 12; currentMonth++)
            {
                Parallel.For(currentMonth * itemsPerMonth, (currentMonth + 1) * itemsPerMonth, new ParallelOptions(), () => 0, (index, loopState, sumBalance) =>
                        {
                            return sumBalance += transactions[index].Amount;
                        }, sumBalance => monthlyBalance[currentMonth] += sumBalance);

                if (currentMonth > 0)
                {
                    monthlyBalance[currentMonth] += monthlyBalance[currentMonth - 1];
                }
            }
            string[] monthNames = Enum.GetNames(typeof(Months));

            for (int i = 0; i < monthlyBalance.Length; i++)
            {
                Console.WriteLine("Sum of Monthly Balance {0} : {1}", monthNames[i], monthlyBalance[i]);
            }
        }

        delegate void ProcessValue(int value);
        static double[] resultData = new double[10000000];
        static void ComputeSquare(int indexValue)
        {
            resultData[indexValue] = Math.Pow(indexValue, 2);
        }

        /// <summary>
        /// Listing 5-14
        /// </summary>
        static void ParallelLoopBodyAnalysis()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            //Perform simple Parallel Loop
            Parallel.For(0, resultData.Length, (index) =>
            {
                resultData[index] = Math.Pow(index, 2);
                //Console.WriteLine("Value:{0} -- Square: {1}", index, Math.Pow(index, 2));
            });
            watch.Stop();
            Console.WriteLine("Simple Parallel Loop took {0} Milliseconds", watch.ElapsedMilliseconds);

            //Perform Parallel Loop But make delegate explicit
            watch = System.Diagnostics.Stopwatch.StartNew();
            Parallel.For(0, resultData.Length, delegate (int index) { resultData[index] = Math.Pow(index, 2); });
            watch.Stop();
            Console.WriteLine("Parallel Loop But make delegate explicit took {0} Milliseconds", watch.ElapsedMilliseconds);


            //Perform Loop with Declared Delegate and Action
            watch = System.Diagnostics.Stopwatch.StartNew();
            ProcessValue pdel = new ProcessValue(ComputeSquare);
            Action<int> paction = new Action<int>(pdel);
            Parallel.For(0, resultData.Length, paction);
            watch.Stop();
            Console.WriteLine("Parallel Loop with Declared Delegate and Action took {0} Milliseconds", watch.ElapsedMilliseconds);

        }


        /// <summary>
        /// Listing 5-15, 5-16 Default Chunking formula = (n/(3*p)) | {n=no.of.items & p=no.of.processors}
        /// </summary>
        static void ChunkingPartitionStrategy()
        {
            //By breaking 10,000,000 index values into chunks of 10,000, we reduce the number of times that the
            //delegate is invoked to 1,000 and improve the ratio of overhead versus index processing
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            OrderablePartitioner<Tuple<int, int>> partition = Partitioner.Create(0, resultData.Length, 10000);

            Parallel.ForEach(partition, partitionRange =>
            {
                for (int i = partitionRange.Item1; i < partitionRange.Item2; i++)
                {
                    resultData[i] = Math.Pow(i, 2);
                }
            });
            stopWatch.Stop();
            Console.WriteLine("Parallel Loop using Chunking Partitions took {0} Milliseconds", stopWatch.ElapsedMilliseconds);


            IList<string> sourceData = new List<string>() { "an", "apple", "a", "day", "keeps", "the", "doctor", "away" };
            string[] adviceData = new string[sourceData.Count];
            OrderablePartitioner<string> part = Partitioner.Create(sourceData);
            
            Parallel.ForEach(part, (string item, ParallelLoopState state, long index) => {
                if (item == "apple")
                    item = "apricot";
                adviceData[index] = item;
            });

            StringBuilder sB = new StringBuilder();
            foreach(string s in adviceData)
            {
                sB.Append(s + " ");
            }
            Console.WriteLine(sB);
        }

    }

    class Account
    {
        public int Amount { get; set; }
    }

    public enum Months
    {
        January = 0,
        February = 1,
        March = 2,
        April = 3,
        May = 4,
        June = 5,
        July = 6,
        August = 7,
        September = 8,
        October = 9,
        November = 10,
        December = 11
    }

}
