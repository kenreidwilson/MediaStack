using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace MediaStack_Importer.Services.ScannerService.ScannerJobs
{
    public abstract class BatchScannerJob<T> : IDisposable
    {
        #region Data members

        protected ILogger Logger;

        protected int BatchSize = 500;

        protected readonly IDictionary<object, T> BatchedEntities;

        private int toProcess;

        private readonly ManualResetEvent finishEvent = new(false);

        private readonly object writeLock = new();

        #endregion

        #region Constructors

        protected BatchScannerJob(ILogger logger)
        {
            this.Logger = logger;
            this.BatchedEntities = new ConcurrentDictionary<object, T>();
        }

        #endregion

        #region Methods

        protected virtual void Execute(IEnumerable<object> data)
        {
            this.toProcess = data.Count();
            foreach (var aData in data)
            {
                this.ExecuteProcess(aData);
            }

            if (data.Any())
            {
                this.finishEvent.WaitOne();
                this.Save();
            }
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
                    this.Logger.LogError(e.ToString());
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

        protected abstract void ProcessData(object data);

        protected abstract void Save();

        public void Dispose()
        {
            this.finishEvent.Dispose();
        }

        #endregion
    }
}