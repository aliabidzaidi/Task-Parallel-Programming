using System;
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
            ParallelLoopUsingTLS();

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

        }
    }
}
