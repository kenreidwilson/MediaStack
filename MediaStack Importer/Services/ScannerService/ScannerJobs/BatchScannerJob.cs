using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace MediaStack_Importer.Services.ScannerService.ScannerJobs
{
    public abstract class BatchScannerJob<T> : IDisposable
    {
        #region Data members

        protected int BatchSize = 100;

        protected readonly IDictionary<string, T> BatchedEntities;

        private int toProcess;

        private readonly ManualResetEvent finishEvent = new(false);

        private readonly object writeLock = new();

        #endregion

        #region Constructors

        protected BatchScannerJob()
        {
            this.BatchedEntities = new ConcurrentDictionary<string, T>();
        }

        #endregion

        #region Methods

        protected virtual void Execute(ICollection<string> data)
        {
            this.toProcess = data.Count;
            foreach (var aData in data)
            {
                this.ExecuteProcess(aData);
            }
            this.finishEvent.WaitOne();
            this.Save();
        }

        protected virtual void ExecuteProcess(object data)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    this.ProcessData(data);
                    lock (this.writeLock)
                    {
                        if (this.BatchedEntities.Count >= this.BatchSize)
                        {
                            this.Save();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    if (Interlocked.Decrement(ref this.toProcess) == 0)
                    {
                        this.finishEvent.Set();
                    }
                }
            });
        }

        protected abstract void Save();

        protected abstract void ProcessData(object data);

        public void Dispose()
        {
            this.finishEvent.Dispose();
        }

        #endregion
    }
}