using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharingData
{
    class Program
    {

        static void Main(string[] args)
        {
            // Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.
            Console.WriteLine("-----Sharing Data Listings-----");

            //RaceCondition();
            //RaceConditionInIsolation();
            //IsolationUsingTLS();
            //UsingLock();
            //SerializeAccess();
            //MutexLock();
            //AcquiringMultipleMutex();
            //InterprocessMutex();
            //DeclarativeSynchronization();
            //ReaderWriterLock();
            ReaderWriterUpgradeableLock();

            Console.ReadKey();
        }


        /// <summary>
        /// Data Race Listing 3-1
        /// </summary>
        static void RaceCondition()
        {
            int Balance = 0;
            Console.WriteLine("Calculating Balance");


            Task[] tarray = new Task[10];

            for (int i = 0; i < tarray.Length; i++)
            {
                tarray[i] = new Task(() =>
                {
                    for (int j = 0; j < 10000; j++)
                    {
                        Balance++;
                    }
                });
                tarray[i].Start();
            }

            Task.WaitAll(tarray);

            Console.WriteLine("Balance Is = " + Balance);


        }

        /// <summary>
        /// Isolation By Convention Listing 3-3
        /// </summary>
        static void RaceConditionInIsolation()
        {
            //Solving Data Race By Isolation
            Console.WriteLine("Solving Data Race By Isolation");
            int FinalBalance = 0;

            Task<int>[] tasks = new Task<int>[10];

            for (int i = 0; i < 10; i++)
            {
                tasks[i] = new Task<int>(() =>
                {
                    int isolatedBalance = 0;

                    for (int j = 0; j < 10000; j++)
                    {
                        isolatedBalance++;
                    }

                    return isolatedBalance;

                });
                tasks[i].Start();
            }


            Task.WaitAll(tasks);
            foreach (Task<int> tempTask in tasks)
            {
                FinalBalance += tempTask.Result;
            }

            Console.WriteLine("Balance Is = " + FinalBalance);
        }

        /// <summary>
        /// Listing 3-4, 3-5 Previously isolation was by convention now we want .net runtime to take care to ensure data is not shared by other threads
        /// </summary>
        static void IsolationUsingTLS()
        {
            BankAccount account = new BankAccount();

            Task<int>[] tasks = new Task<int>[10];
            ThreadLocal<int> tls = new ThreadLocal<int>();
            //ThreadLocal<int> tls = new ThreadLocal<int>(() => {
            //    Console.WriteLine("Value factory called for value: {0}",
            //    account.Balance);
            //    return account.Balance;
            //});

            for (int i = 0; i < 10; i++)
            {
                tasks[i] = new Task<int>((balance) =>
                {
                    //tls.Value = (int)balance;
                    for (int j = 0; j < 10000; j++)
                    {
                        tls.Value++;
                    }
                    return tls.Value;
                }, account.Balance);
                tasks[i].Start();
            }


            Task.WaitAll(tasks);
            foreach (Task<int> tempTask in tasks)
            {
                account.Balance += tempTask.Result;
            }

            Console.WriteLine("Balance Is = " + account.Balance);
        }

        /// <summary>
        /// Listing 3-6 Using the lock keyword to the critical region, lock is a shortcut of the monitor class which is a heavy weight primitive
        /// </summary>
        static void UsingLock()
        {
            Console.WriteLine("Calculating Balance Using Locks");
            BankAccount bank = new BankAccount();

            Task[] tasks = new Task[10];

            object lockObject = new object();

            for (int i = 0; i < 10; i++)
            {
                tasks[i] = new Task(() =>
                {
                    for (int j = 0; j < 10000; j++)
                    {
                        lock (lockObject)
                            bank.Balance = bank.Balance + 1;
                    }
                });

                tasks[i].Start();
            }

            Task.WaitAll(tasks);

            Console.WriteLine("Final Calculated Balance ={0}", bank.Balance);
        }

        /// <summary>
        /// Listing 3-7, 3-8 
        /// Sharing lock between two different critical regions
        /// Increment & Decrement using interlocked class
        /// For Convergent Isolation using compare exchange see Listing 3-9
        /// </summary>
        static void SerializeAccess()
        {
            Console.WriteLine("Increment and Decrement Tasks");
            BankAccount account = new BankAccount();

            Task[] incrementTasks = new Task[5];
            Task[] decrementTasks = new Task[5];

            object lockObj = new object();

            for (int i = 0; i < 5; i++)
            {
                incrementTasks[i] = new Task(() =>
                {
                    for (int j = 0; j < 10000; j++)
                    {
                        //lock (lockObj)
                        //    account.Balance = account.Balance + 1;
                        Interlocked.Increment(ref account.BalanceRef);
                    }
                });

                incrementTasks[i].Start();
            }

            for (int i = 0; i < 5; i++)
            {
                decrementTasks[i] = new Task(() =>
                {
                    for (int j = 0; j < 10000; j++)
                    {
                        //lock (lockObj)
                        //    account.Balance = account.Balance - 1;
                        Interlocked.Decrement(ref account.BalanceRef);
                    }
                });

                decrementTasks[i].Start();
            }


            Task.WaitAll(incrementTasks);
            Task.WaitAll(decrementTasks);

            Console.WriteLine("Final Account Balance is {0}", account.Balance);

        }

        /// <summary>
        /// Listing 3-10 Synchronizing Data using spinlock primitive
        /// </summary>
        static void SpinLocking()
        {
            BankAccount account = new BankAccount();
            SpinLock spinLock = new SpinLock();

            Task[] tasks = new Task[10];

            for (int i = 0; i < 10; i++)
            {
                tasks[i] = new Task(() =>
                {
                    for (int j = 0; j < 10000; j++)
                    {
                        bool lockAcquired = false;
                        try
                        {
                            spinLock.Enter(ref lockAcquired);
                            account.Balance = account.Balance + 1;
                        }
                        finally
                        {
                            if (lockAcquired)
                                spinLock.Exit();
                        }
                    }
                });

                tasks[i].Start();
            }

            Task.WaitAll(tasks);


            Console.WriteLine("Task Expected Value {0} and Resulting Value {1}", 10000, account.Balance);
        }

        /// <summary>
        /// Listing 3-11 Using Mutex in data race condition
        /// </summary>
        static void MutexLock()
        {
            BankAccount account = new BankAccount();

            Mutex mutex = new Mutex();

            Task[] tasks = new Task[10];

            for (int i = 0; i < 10; i++)
            {
                tasks[i] = new Task(() =>
                {
                    for (int j = 0; j < 10000; j++)
                    {

                        bool lockAcquired = mutex.WaitOne();
                        try
                        {
                            account.Balance = account.Balance + 1;
                        }
                        finally
                        {
                            if (lockAcquired)
                                mutex.ReleaseMutex();
                        }
                    }
                });

                tasks[i].Start();
            }


            Task.WaitAll(tasks);

            Console.WriteLine("Expected balance {0}", 100000);
            Console.WriteLine("Resulting balance {0}", account.Balance);
        }

        /// <summary>
        /// Listing 3-12 Acquiring Multiple Mutex Locks
        /// </summary>
        static void AcquiringMultipleMutex()
        {
            BankAccount account1 = new BankAccount();
            BankAccount account2 = new BankAccount();

            Mutex mutex1 = new Mutex();
            Mutex mutex2 = new Mutex();

            Task T1 = new Task(() =>
            {
                for (int i = 0; i < 100000; i++)
                {
                    bool lockAcquired = mutex1.WaitOne();

                    try
                    {
                        account1.Balance = account1.Balance + 1;
                    }
                    finally
                    {
                        if (lockAcquired)
                            mutex1.ReleaseMutex();
                    }
                }
            });

            Task T2 = new Task(() =>
            {
                for (int i = 0; i < 100000; i++)
                {
                    bool lockAcquired = mutex1.WaitOne();

                    try
                    {
                        account2.Balance = account2.Balance + 1;
                    }
                    finally
                    {
                        if (lockAcquired)
                            mutex1.ReleaseMutex();
                    }
                }
            });

            Task T3 = new Task(() =>
            {
                for (int i = 0; i < 100000; i++)
                {
                    bool lockAcquired = Mutex.WaitAll(new WaitHandle[] { mutex1, mutex2 });

                    try
                    {
                        account1.Balance++;
                        account2.Balance--;
                    }
                    finally
                    {
                        if (lockAcquired)
                        {
                            mutex1.ReleaseMutex();
                            mutex2.ReleaseMutex();
                        }
                    }
                }
            });


            T1.Start();
            T2.Start();
            T3.Start();

            Task.WaitAll(T1, T2, T3);

            Console.WriteLine("Account 1 Balance {0}", account1.Balance);
            Console.WriteLine("Account 2 Balance {0}", account2.Balance);
        }

        /// <summary>
        /// Listing 3-13 Inter process synchronization
        /// </summary>
        static void InterprocessMutex()
        {
            string mutexName = "mutex101";
            Mutex mutex;

            try
            {
                mutex = Mutex.OpenExisting(mutexName);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                mutex = new Mutex(false, mutexName);
            }

            Task T1 = new Task(() =>
            {
                while (true)
                {
                    Console.WriteLine("Waiting to acquire Lock");
                    mutex.WaitOne();

                    Console.WriteLine("Press Enter to Release Lock");
                    Console.ReadKey();

                    mutex.ReleaseMutex();
                }
            });
            T1.Start();

            T1.Wait();
            Console.WriteLine("Program Ended");
        }

        /// <summary>
        /// Listing 3-14
        /// </summary>
        static void DeclarativeSynchronization()
        {
            BankAccount2 account = new BankAccount2();
            Task[] tasks = new Task[10];

            for (int i = 0; i < 10; i++)
            {
                tasks[i] = new Task(() =>
                {
                    for (int j = 0; j < 1000; j++)
                    {
                        account.IncrementBalance();
                    }
                });
                tasks[i].Start();
            }

            Task.WaitAll(tasks);

            Console.WriteLine("Expected Balance {0}", 10000);
            Console.WriteLine("Resulting Balance {0}", account.GetBalance());

        }

        /// <summary>
        /// Listing 3-15 Working with ReaderWriterLockSlim class
        /// </summary>
        static void ReaderWriterLock()
        {
            Console.WriteLine("Simulation for ReaderWriterLockSlim Class");
            ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            Task[] tasks = new Task[5];

            for (int i = 0; i < 5; i++)
            {
                tasks[i] = new Task(() =>
                {
                    while (true)
                    {
                        rwLock.EnterReadLock();

                        Console.WriteLine("Read Lock Acquired .. Waiting for 1 second");

                        token.WaitHandle.WaitOne(1000);

                        rwLock.ExitReadLock();
                        Console.WriteLine("Read Lock Released with Current Read Count :{0}", rwLock.CurrentReadCount);

                        token.ThrowIfCancellationRequested();
                    }
                }, token);

                tasks[i].Start();
            }

            Console.WriteLine("Press Enter to acquire Write Lock");
            Console.ReadKey();
            rwLock.EnterWriteLock();


            Console.WriteLine("Press Enter to release Write Lock");
            Console.ReadKey();
            rwLock.ExitWriteLock();

            token.WaitHandle.WaitOne(2000);
            source.Cancel();

            try
            {
                Task.WaitAll(tasks);
            }
            catch (AggregateException ag)
            {
                Console.WriteLine(ag.Message);
            }

            Console.WriteLine("ReaderWriterLockSlim Class Ends here.......");
        }

        /// <summary>
        /// Listing 3-16 Working with UpgradeableReadLock
        /// </summary>
        static void ReaderWriterUpgradeableLock()
        {
            int sharedData = 0;
            CancellationTokenSource source = new CancellationTokenSource();
            ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

            Task[] readTasks = new Task[5];

            for (int i = 0; i < 5; i++)
            {
                readTasks[i] = new Task(() =>
                {
                    for (int j = 0; j < 10000; j++)
                    {
                        Console.WriteLine("Entering Read Lock with SHARED DATA{0}", sharedData);
                        rwLock.EnterReadLock();

                        source.Token.WaitHandle.WaitOne(1000);
                        Console.WriteLine("Releasing ReadLock {0}", rwLock.CurrentReadCount);

                        rwLock.ExitReadLock();

                        source.Token.ThrowIfCancellationRequested();
                    }
                }, source.Token);

                readTasks[i].Start();
            }

            Task[] upgradeableTasks = new Task[1];

            for (int i = 0; i < 1; i++)
            {
                upgradeableTasks[i] = new Task(() =>
                {
                    for (int j = 0; j < 1000; j++)
                    {
                        Console.WriteLine("Entering UpgradeableRead Lock");
                        rwLock.EnterUpgradeableReadLock();

                        if (true)//Condition on write
                        {
                            Console.WriteLine("Writers{0} \t Reader {1} " + "\t Upgraders {2}", rwLock.WaitingWriteCount, rwLock.WaitingReadCount, rwLock.WaitingUpgradeCount);

                            rwLock.EnterWriteLock();
                            sharedData++;
                            source.Token.WaitHandle.WaitOne(2000);
                            rwLock.ExitWriteLock();
                        }

                        rwLock.ExitUpgradeableReadLock();
                    }
                    source.Token.ThrowIfCancellationRequested();
                },source.Token);
                upgradeableTasks[i].Start();
            }

            Console.ReadKey();
            source.Cancel();

            try
            {
                Task.WaitAll();
            }
            catch (AggregateException ag)
            {
                ag.Handle(ex => true);
            }

        }

        /// <summary>
        /// Listing 3-17 Showing Data race in collection
        /// </summary>
        static void DataRaceInCollection()
        {

        }

        /// <summary>
        /// Listing 3-18 Using Concurrent Queue to tackle data race
        /// </summary>
        static void UsingConcurrentQueue()
        {

        }

        /// <summary>
        /// Listing 3-19 Using Concurrent Stack to tackle data race
        /// </summary>
        static void UsingConcurrentStack()
        {
            
        }
    }

    /// <summary>
    /// Listing 3-2 const should be declared in a single statement whereas readonly fields must be initialized in constructor
    /// </summary>
    class BankAccount
    {
        //private readonly int Balance;
        //public const string AccountNumber = "123456";

        public int BalanceRef;
        public int Balance { get; set; }

        public BankAccount()
        {
            Balance = 0;
        }

        public BankAccount(int balance)
        {
            Balance = balance;
        }
    }

    /// <summary>
    /// Listing 3-14 Changes in BankAccount was Declarative Synchronization
    /// </summary>
    [Synchronization]
    class BankAccount2 : ContextBoundObject
    {
        private int Balance = 0;

        public void IncrementBalance()
        {
            Balance++;
        }

        public void DecrementBalance()
        {
            Balance--;
        }

        public int GetBalance()
        {
            return Balance;
        }
    }
}
