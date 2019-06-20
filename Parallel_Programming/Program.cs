using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Drawing;

namespace Parallel_Programming
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Main Program has Started");

            //SimpleTask();

            //DifferentWaysToCreateTask();

            //SettingStateOfTasks();

            //CreateMultipleTasks();

            //GetTaskResult();

            //CancellingTaskByPolling();

            //CancellingMultipleTasks();

            //CompositeCancellation();

            //CheckIfTaskIsCancelledTask();

            //WaitingForTimeToPass();

            //WaitingForTask();

            //HandlingBasicException();

            HandlingExceptionViaIterativeHandler();

            //HandlingExceptionViaEventHandler();

            //ExecutingTaskLazily();

            Console.WriteLine("Main Program Has Exited!");

            Console.ReadKey();
        }

        /// <summary>
        /// Listing 2-1, 2-6
        /// </summary>
        public static void SimpleTask()
        {
            Task.Factory.StartNew(() => { Console.WriteLine("I am from another ThreadPlanet"); });

            //Create a task using factory + Setting Task State + Returning Result
            Task<int> T1 = Task.Factory.StartNew<int>(obj =>
            {
                int a = (int)obj;
                return a * a;
            }, 10);
            Console.WriteLine("(Not)Simple Task of Factory" + T1.Result);
        }

        /// <summary>
        /// Listing 2-2
        /// </summary>
        public static void DifferentWaysToCreateTask()
        {
            Task t1 = new Task(new Action(printMessage)); //Use Action Delegate With Named Method
            t1.Start();

            Task t2 = new Task(delegate { Console.WriteLine("Hello Alien!"); }); //Use an anonymous delegate
            t2.Start();

            Task t3 = new Task(() => printMessage()); //Use Lambda Expression and named method
            t3.Start();

            Task t4 = new Task(() => { Console.WriteLine("Hello Alien!"); }); //Use Lambda Expression and an anonymous method
            t4.Start();

        }

        /// <summary>
        /// Listing 2-3
        /// </summary>
        public static void SettingStateOfTasks()
        {
            Task t1 = new Task(new Action<object>(printMessage), "Task 1"); //Use Action Delegate with Named Method + Setting State
            t1.Start();

            Task t2 = new Task(delegate (Object obj) { Console.WriteLine("Hello Alien! I belong from {0}", obj); }, "Task 2"); //Use an anonymous delegate + Setting State
            t2.Start();

            Task t3 = new Task((obj) => printMessage(obj), "Task 3"); //Use Lambda Expression and named method + Setting State
            t3.Start();

            Task t4 = new Task((obj) => { Console.WriteLine("Hello Alien! I belong from {0}", obj); }, "Task 4"); //Use Lambda Expression and an anonymous method + Setting Task State
            t4.Start();

        }

        /// <summary>
        /// Listing 2-4
        /// </summary>
        public static void CreateMultipleTasks()
        {

            string[] strArray = new string[] { "T1", "T2", "T3" };

            foreach (string str in strArray)
            {
                Task tempTask = new Task(obj => { printMessage(obj); }, str);
                tempTask.Start();
            }

        }

        /// <summary>
        /// Listing 2-5
        /// </summary>
        public static void GetTaskResult()
        {
            //Return Result of a task in Result property when 
            Task<int> T1 = new Task<int>(() =>
            {
                int a = 10;

                return a * a;
            });
            T1.Start();
            Console.WriteLine("Task 1 =" + T1.Result);

            int number = 10;
            //Return Result of a Task with anonymous method and state
            Task<int> T2 = new Task<int>(obj =>
            {
                int square = (int)obj;
                return square * square;
            }, number);

            T2.Start();

            Console.WriteLine("Task 2 =" + T2.Result);
        }

        /// <summary>
        ///3 ways for Monitoring a Cancelled Task 
        /// Listing 2-7 & 2-8 & 2-9
        /// </summary>
        public static void CancellingTaskByPolling()
        {
            try
            {
                CancellationTokenSource cancelSource = new CancellationTokenSource();

                CancellationToken cancelToken = cancelSource.Token;

                Task T1 = new Task(() =>
                {
                    for (int i = 0; i < int.MaxValue; i++)
                    {
                        //1 Monitoring Cancellation with polling use cancelToken.IfCancellationRequested
                        cancelToken.ThrowIfCancellationRequested();
                        if (cancelToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException(cancelToken);
                        }
                        Console.Write(i + ".");
                    }
                }, cancelToken);
                
                //2 Registering a delegate on cancellation (for Monitoring)
                cancelToken.Register(() =>
                {
                    Console.WriteLine("Task Cancelled... *Do Something with Delegate*");
                });

                //3 Using a Wait Handle to Monitor a task
                Task waitingTask = new Task(() =>
                {
                    Console.WriteLine(">>>Waiting for Task to be Cancelled *WaitHandle");

                    cancelToken.WaitHandle.WaitOne();

                    Console.WriteLine(">>>Wait Handle Released");
                });

                Console.WriteLine("Enter to Start CancellingTaskByPolling");
                Console.ReadKey();
                T1.Start();
                waitingTask.Start();

                Console.WriteLine("Enter to Cancel Task *");
                Console.ReadKey();
                cancelSource.Cancel();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Thrown " + ex.ToString());
            }

            Console.WriteLine("Method CancellingTaskByPolling finished Executing");
        }

        /// <summary>
        /// Cancelling Multiple Tasks together Listing 2-10
        /// </summary>
        public static void CancellingMultipleTasks()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            Task T1 = new Task(() =>
            {
                for (int i = 0; i < 10000000; i++)
                {
                    token.ThrowIfCancellationRequested();
                    Console.WriteLine("T1 " + i);
                }
            }, token);

            Task T2 = new Task(() =>
            {
                for (int j = 0; j < 10000000; j++)
                {
                    token.ThrowIfCancellationRequested();
                    Console.WriteLine("T2 " + j);
                }
            });

            Console.WriteLine("Enter to start task, then press enter to stop them");
            T1.Start(); T2.Start();

            Console.ReadKey();

            tokenSource.Cancel();
            Console.WriteLine("Tasks Cancelled");
        }

        /// <summary>
        /// Composite Cancellation Token Listing 2-11
        /// </summary>
        public static void CompositeCancellation()
        {
            CancellationTokenSource source1 = new CancellationTokenSource();
            CancellationTokenSource source2 = new CancellationTokenSource();
            CancellationTokenSource source3 = new CancellationTokenSource();

            CancellationTokenSource compositeSource = CancellationTokenSource.CreateLinkedTokenSource(source1.Token, source2.Token, source3.Token);

            //pass all token sources to different tasks then cancel task using a wait handle

            Task waitHandleTask = new Task(() =>
            {
                Console.WriteLine("Waiting for task to be cancelled.........(Press any key)");
                compositeSource.Token.WaitHandle.WaitOne();
                Console.WriteLine("Task Cancelled");
            }, compositeSource.Token);

            waitHandleTask.Start();

            Console.ReadKey();

            //Cancel any token source
            source3.Cancel();

        }

        /// <summary>
        /// Checking if task is cancelled Listing 2-12
        /// </summary>
        public static void CheckIfTaskIsCancelledTask()
        {
            CancellationTokenSource source1 = new CancellationTokenSource();
            CancellationTokenSource source2 = new CancellationTokenSource();

            CancellationToken token1 = source1.Token;
            CancellationToken token2 = source2.Token;

            Task T1 = new Task(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    token1.ThrowIfCancellationRequested();
                    Console.WriteLine("T1 " + i);
                }
            }, token1);

            Task T2 = new Task(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    token2.ThrowIfCancellationRequested();
                    Console.WriteLine("T2 " + i);
                }
            }, token2);

            T1.Start();
            T2.Start();

            source2.Cancel();

            Thread.Sleep(2000);
            Console.WriteLine("T1 Cancel = " + T1.IsCanceled);
            Console.WriteLine("source1 Cancel = " + source1.IsCancellationRequested);

            Console.WriteLine("T2 Cancel = " + T2.IsCanceled);
            Console.WriteLine("source2 Cancel = " + source2.IsCancellationRequested);


        }

        /// <summary>
        /// Waiting for Time To Pass Listing 2-13, 2-14, 2-15
        /// Using CancellationToken.WaitHandle.Wait(xx) & Legacy ThreadSleep & ThreadSpinWait(which uses CPU)
        /// </summary>
        public static void WaitingForTimeToPass()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            Task T1 = new Task(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    Console.WriteLine("Task Loop.. Sleeping for 1 seconds");

                    bool cancelled = token.WaitHandle.WaitOne(1000);

                    Console.WriteLine("i={0} .. cancelled={1}", i, cancelled);

                    if (cancelled)
                    {
                        Console.WriteLine("Task Cancelled");
                        throw new OperationCanceledException();

                    }
                }

            }, token);

            Task ClassicThreadSleepAndSpinWait = new Task(() =>
            {
                for (int i = 0; i < Int32.MaxValue; i++)
                {
                    // put the task to sleep for 10 seconds
                    Thread.Sleep(10000);

                    //Thread doesn't give up its turn on the CPU 
                    Thread.SpinWait(10000);

                    // print out a message
                    Console.WriteLine("Task 1 - Int value {0}", i);
                    // check for task cancellation
                    token.ThrowIfCancellationRequested();
                }
            }, token);

            T1.Start();

            Console.ReadKey();

            tokenSource.Cancel();

        }

        /// <summary>
        /// Waiting for Task Listing 2-16, 2-17, 2-18
        /// </summary>
        public static void WaitingForTask()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            Task T1 = new Task(() => { Console.WriteLine("Task 1"); }, token);

            Task T2 = new Task(() => { Console.WriteLine("Task 2"); }, token);

            T1.Start();
            T2.Start();

            //Number of ways to achieve wait 
            T1.Wait();

            //Wait for Number of milliseconds
            T1.Wait(2000);

            //Wait for Task to complete execution or terminates if task is cancelled
            T1.Wait(token);

            //Wait for milliseconds or terminates if task is cancelled 
            T1.Wait(2000, token);

            // Wait For All Tasks
            Task.WaitAll(new Task[] { T1, T2 });
            Task.WaitAll(T1, T2);

            //Waits for One or Many Tasks
            int completedTask = Task.WaitAny(T1, T2);

            Console.WriteLine("program finished");
        }

        /// <summary>
        /// Handling Exceptions in TPL Listing 2-19
        /// Exception that is not handled is known as an unhandled exception which is dangerous
        /// Leave Exceptions unhandled and override the default policy for dealing exceptions
        /// </summary>
        public static void HandlingBasicException()
        {
            Task T1 = new Task(() => { throw new ArgumentOutOfRangeException() { Source = "T1"}; });

            Task T2 = new Task(() => { throw new NullReferenceException(); });

            Task T3 = new Task(() => { Console.WriteLine("Artificial Cancellation"); throw new OperationCanceledException(); });

            T1.Start(); T2.Start(); T3.Start();

            try
            {
                Task.WaitAll(T1, T2, T3);
            }
            catch (AggregateException ex)
            {
                foreach (Exception inner in ex.InnerExceptions)
                {
                    Console.WriteLine("Exception {0} : from {1}", inner.GetType(), inner.Source);
                }

            }
        }

        /// <summary>
        /// 2-20 using Iterative Handler ,
        /// 2-21 Handling exception with Task Properties Listing
        /// Handling known exceptions that are expected Listing in Iterative Handler
        /// </summary>
        public static void HandlingExceptionViaIterativeHandler()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            Task T1 = new Task(() => {
                for (int i = 0; i < 1000000; i++)
                {
                    token.ThrowIfCancellationRequested();
                }
            }, token);

            Task T2 = new Task(() => { throw new IndexOutOfRangeException(); });
            Task T3 = new Task(() => { Console.WriteLine("Task 3 did work"); });

            T1.Start(); T2.Start(); T3.Start();
            source.Cancel();
            try
            {
                Task.WaitAll(T1, T2, T3);
            }
            catch (AggregateException ex){
                ex.Handle((inner) =>
                {
                    if (inner is OperationCanceledException)
                    {
                        Console.WriteLine(inner.Message);
                        return true; //Handled Exception
                    }
                    else
                    {
                        Console.WriteLine(inner.Message);
                        return false; //Unhandled Exception
                    }
                }
                );
            }

            Console.WriteLine("Task1 Completed:{0}  Faulted:{1}  IsCancelled:{2}", T1.IsCompleted, T1.IsFaulted, T1.IsCanceled);
            //Console.WriteLine(T1.Exception);

            Console.WriteLine("Task2 Completed:{0}  Faulted:{1}  IsCancelled:{2}", T2.IsCompleted, T2.IsFaulted, T2.IsCanceled);
            //Console.WriteLine(T2.Exception);

            Console.WriteLine("Task3 Completed:{0}  Faulted:{1}  IsCancelled:{2}", T3.IsCompleted, T3.IsFaulted, T3.IsCanceled);
            //Console.WriteLine(T3.Exception);

        }

        /// <summary>
        /// Handling exception with Custom Escalation Policy Listing 2-22
        /// 
        static void HandlingExceptionViaEventHandler()
        {
            TaskScheduler.UnobservedTaskException += (object sender, UnobservedTaskExceptionEventArgs args) =>
            {
                Console.WriteLine("Unobserved Exception Caught");
                args.SetObserved();
                ((AggregateException)args.Exception).Handle((ex) =>
                {
                    Console.WriteLine("Exception type : {0}", ex.GetType());
                    return true;
                });
            };

            Task T1 = new Task(() =>
            {
                Console.WriteLine("I am T1");
                throw new IndexOutOfRangeException();
            });

            Task T2 = new Task(() =>
            {
                Console.WriteLine("I am T2");
                throw new NullReferenceException();
            });

            T1.Start(); T2.Start();


            while (!T1.IsCompleted || !T2.IsCompleted)
            {
                Thread.Sleep(100);
            }

            //Need to do this because the exception won't be unobserved until garbage collector collects it
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine("All Tasks Finished");

        }

        /// <summary>
        /// Executing task lazily  Listing 2-23
        /// </summary>
        static void ExecutingTaskLazily()
        {
            Func<string> taskBody = new Func<string>(() =>
            {
                Console.WriteLine("Task Body Working .....");
                return "TaskBodyResult";
            });

            Lazy<Task<string>> lazyTask = new Lazy<Task<string>>(() => Task<string>.Factory.StartNew(taskBody));

            Lazy<Task<string>> lazyTask1 = new Lazy<Task<string>>(() =>
                Task<string>.Factory.StartNew(() =>
                {
                    Console.WriteLine("Task Body Working .....");
                    return "TaskBody Result";
                })
            );

            Console.WriteLine("Accessing lazy Task Result {0}", lazyTask.Value.Result);

            Console.WriteLine("Not Accessing LazyTask1 ");
        }

        public static void printMessage()
        {
            Console.WriteLine("Hello Alien!");
        }

        public static void printMessage(Object obj)
        {
            Console.WriteLine("Hello Alien! I am from {0}", obj);
        }

    }
}
