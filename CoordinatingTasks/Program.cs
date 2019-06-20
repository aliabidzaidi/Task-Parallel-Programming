using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoordinatingTasks
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Coordinating Tasks");

            //Barrier();
            //BarrierExceptions();
            //SemaphoreSlim();
            //ProducerConsumerPattern();
            //MultipleBlockingCollection();
            ReuseObjectInConsumer();

            Console.WriteLine("Coordinating Tasks program finished");
            Console.ReadKey();
        }

        /// <summary>
        /// Listing 4-1
        /// </summary>
        static void TaskContinuation()
        {
            Task<BankAccount> task = new Task<BankAccount>(() =>
            {
                // create a new bank account
                BankAccount account = new BankAccount();
                // enter a loop
                for (int i = 0; i < 1000; i++)
                {
                    // increment the account total
                    account.Balance++;
                }
                // return the bank account
                return account;
            });

            task.ContinueWith((Task<BankAccount> antecedent) =>
            {
                Console.WriteLine("Final Balance: {0}", antecedent.Result.Balance);
            });

            // start the task
            task.Start();

            Console.WriteLine("Press enter to finish");
            Console.ReadLine();
        }


        /// <summary>
        /// Listing 4-12 Coordinating multiple tasks cooperatively in multiple phases
        /// </summary>
        static void Barrier()
        {
            Console.WriteLine("Barrier program started");
            BankAccount[] accounts = new BankAccount[5];
            for (int i = 0; i < accounts.Length; i++)
            {
                accounts[i] = new BankAccount();
            }
            // create the total balance counter
            int totalBalance = 0;

            Barrier barrier = new Barrier(5, (myBarrier) =>
            {
                // zero the balance
                totalBalance = 0;
                // sum the account totals
                foreach (BankAccount account in accounts)
                {
                    totalBalance += account.Balance;
                }
                // write out the balance
                Console.WriteLine("Total balance: {0}", totalBalance);
            });
            // define the tasks array
            Task[] tasks = new Task[5];

            for (int i = 0; i < 5; i++)
            {
                tasks[i] = new Task((stateObject) =>
                {
                    BankAccount account = (BankAccount)stateObject;

                    Random rnd = new Random();
                    for (int j = 0; j < 1000; j++)
                    {
                        account.Balance += 1;
                    }

                    Console.WriteLine("Task {0} - Phase No {1}", Task.CurrentId, barrier.CurrentPhaseNumber);
                    barrier.SignalAndWait();

                    account.Balance -= (totalBalance - account.Balance) / 10;

                    Console.WriteLine("Task {0} - Phase No {1}", Task.CurrentId, barrier.CurrentPhaseNumber);
                    barrier.SignalAndWait();
                }, accounts[i]);
            }

            foreach (Task t in tasks)
            {
                t.Start();
            }

            Task.WaitAll();
            Console.WriteLine("Barrier program ended");

        }

        /// <summary>
        /// Listing 4-13 & 4-14
        /// </summary>
        static void BarrierExceptions()
        {
            Barrier barrier = new Barrier(2);

            //4-14
            CancellationTokenSource source = new CancellationTokenSource();

            Task.Factory.StartNew(() =>
            {
                Console.WriteLine("Good task starting phase 0");
                //barrier.SignalAndWait();
                barrier.SignalAndWait(source.Token);//4-14
                Console.WriteLine("Good task starting phase 1");
                //barrier.SignalAndWait();//4-14
                barrier.SignalAndWait(source.Token);
                Console.WriteLine("Good task complete");
            });

            Task.Run(() =>
            {
                Console.WriteLine("Bad task about to throw exception");
                throw new Exception();
            }).ContinueWith(antecedent =>
            {
                //Console.WriteLine("Reducing number of participant count");
                //barrier.RemoveParticipant();
                Console.WriteLine("All participants cancelling");
                source.Cancel();//4-14
            }, TaskContinuationOptions.OnlyOnFaulted);

            Console.WriteLine("Barrier Exceptions finished");
        }

        /// <summary>
        /// Listing 4-15 Count Down Event
        /// Listing 4-16 Manual Reset Event
        /// Listing 4-17 Auto Reset Event
        /// </summary>
        static void CountDownEvent()
        {

        }

        /// <summary>
        /// Listing 4-18
        /// </summary>
        static void SemaphoreSlim()
        {
            // create the primtive
            SemaphoreSlim semaphore = new SemaphoreSlim(2);
            // create the cancellation token source
            CancellationTokenSource tokenSource
            = new CancellationTokenSource();
            for (int i = 0; i < 10; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        semaphore.Wait(tokenSource.Token);
                        // print out a message when we are released
                        Console.WriteLine("Task {0} released", Task.CurrentId);
                    }
                }, tokenSource.Token);
            }
            // create and start the signalling task
            Task signallingTask = Task.Factory.StartNew(() =>
            {
                // loop while the task has not been cancelled
                while (!tokenSource.Token.IsCancellationRequested)
                {
                    // go to sleep for a random period
                    tokenSource.Token.WaitHandle.WaitOne(500);
                    // signal the semaphore
                    semaphore.Release(2);
                    Console.WriteLine("Semaphore released");
                }
                // if we reach this point, we know the task has been cancelled
                tokenSource.Token.ThrowIfCancellationRequested();
            }, tokenSource.Token);
            // ask the user to press return before we cancel
            // the token and bring the tasks to an end
            Console.WriteLine("Press enter to cancel tasks");
            Console.ReadLine();
            // cancel the token source and wait for the tasks
            tokenSource.Cancel();
        }

        /// <summary>
        /// One Task group create items processsed by another Tasks
        /// processing is made by using a queue, producer adds and consumer removes
        /// synchronization primitive is used to signal consumers of available work items
        /// 
        /// </summary>
        static void ProducerConsumerPattern()
        {
            BlockingCollection<Deposit> blockingCollection = new BlockingCollection<Deposit>();

            CancellationTokenSource source = new CancellationTokenSource();

            Task[] producers = new Task[3];
            for (int i = 0; i < 3; i++)
            {
                producers[i] = Task.Factory.StartNew(() =>
                {
                    for (int j = 0; j < 20; j++)
                    {
                        Deposit depo = new Deposit() { Amount = 100 };
                        blockingCollection.Add(depo);
                    }
                });
            }

            Task.Factory.ContinueWhenAll(producers, antecedant =>
            {
                Console.WriteLine("All Tasks Added!");
                blockingCollection.CompleteAdding();
            });

            BankAccount account = new BankAccount();

            Task consumer = Task.Factory.StartNew(() =>
            {
                while (!blockingCollection.IsCompleted)
                {
                    Deposit depo;
                    if (blockingCollection.TryTake(out depo))
                    {
                        account.Balance += depo.Amount;
                    }
                }
                Console.WriteLine("Deposit Successful, Final Balance is " + account.Balance);
            });

            consumer.Wait();
            Console.WriteLine("Deposit Successful, Final Balance is " + account.Balance);


        }

        /// <summary>
        /// Listing 4-20
        /// Example of ProducerConsumerPattern in which different and multiple producers are used.
        /// </summary>
        static void MultipleBlockingCollection()
        {
            BlockingCollection<string> bc1 = new BlockingCollection<string>();
            BlockingCollection<string> bc2 = new BlockingCollection<string>();
            BlockingCollection<string> bc3 = new BlockingCollection<string>();

            BlockingCollection<string>[] bc1and2 = { bc1, bc2 };
            BlockingCollection<string>[] bcAll = { bc1, bc2, bc3 };

            CancellationTokenSource source = new CancellationTokenSource();

            for (int i = 0; i < 5; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    while (!source.IsCancellationRequested)
                    {
                        string message = String.Format("Message from Task{0}", Task.CurrentId);
                        BlockingCollection<string>.AddToAny(bc1and2, message);

                        source.Token.WaitHandle.WaitOne(1000);
                    }
                }, source.Token);
            }

            for (int i = 0; i < 3; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    if (!source.IsCancellationRequested)
                    {
                        string warning = String.Format("Warning from Task{0}", Task.CurrentId);
                        bc3.Add(warning);
                        source.Token.WaitHandle.WaitOne(500);
                    }

                }, source.Token);
            }

            //consumer
            for (int i = 0; i < 2; i++)
            {
                Task consumer = Task.Factory.StartNew(() =>
                {
                    string item;
                    while (!source.IsCancellationRequested)
                    {
                        int bcid = BlockingCollection<string>.TakeFromAny(bcAll, out item, source.Token);

                        Console.WriteLine("From collection {0} : {1}", bcid, item);
                    }
                }, source.Token);
            }

            Console.WriteLine("Enter to cancel ");
            Console.ReadKey();
            source.Cancel();


        }

        static void ReuseObjectInConsumer()
        {
            BlockingCollection<BankAccount> blockingCollection = new BlockingCollection<BankAccount>();
            CancellationTokenSource source = new CancellationTokenSource();
            Task consumer = Task.Factory.StartNew(() =>
            {
                BankAccount account = new BankAccount();

                while (!blockingCollection.IsCompleted)
                {
                    if (blockingCollection.TryTake(out account))
                    {
                        Console.WriteLine("Account balance {0}", account.Balance);
                        account.Balance++;
                        source.Token.WaitHandle.WaitOne(100);
                    }
                }
            });

            Task producer = Task.Run(() =>
            {
                BankAccount account = new BankAccount();

                for (int i = 0; i < 100; i++)
                {
                    account.Balance++;
                    blockingCollection.Add(account);
                    source.Token.WaitHandle.WaitOne(100);
                }
                blockingCollection.CompleteAdding();
            });

            consumer.Wait();
        }
    }

    class BankAccount
    {
        public int Balance { get; set; }
    }

    class Deposit
    {
        public int Amount { get; set; }
    }
}
