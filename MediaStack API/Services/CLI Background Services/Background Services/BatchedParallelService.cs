using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace MediaStack_API.Services.CLI_Background_Services.Background_Services
{
    public abstract class BatchedParallelService<T> : IDisposable
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

        protected BatchedParallelService(ILogger logger)
        {
            this.Logger = logger;
            this.BatchedEntities = new ConcurrentDictionary<object, T>();
        }

        #endregion

        #region Methods

        public abstract void Execute();

        protected virtual void ExecuteWithData(IEnumerable<object> data)
        {
            this.toProcess = data.Count();
            bool shouldWait = this.toProcess > 0;

            foreach (var aData in data)
            {
                this.QueueExecuteProcess(aData);
            }

            if (shouldWait)
            {
                this.finishEvent.WaitOne();
                this.Save();
            }
        }

        protected virtual void QueueExecuteProcess(object data)
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