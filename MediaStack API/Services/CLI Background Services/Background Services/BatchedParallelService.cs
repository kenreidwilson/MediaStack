using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MediaStack_API.Services.CLI_Background_Services.Background_Services
{
    public abstract class BatchedParallelService<T>
    {
        #region Data members

        protected ILogger Logger;

        protected int BatchSize = 500;

        protected readonly IDictionary<object, T> BatchedEntities;

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

        public abstract Task Execute(CancellationToken cancellationToken);

        protected virtual async Task ExecuteWithData(IEnumerable<object> data, CancellationToken cancellationToken)
        {
            List<Task> taskList = new List<Task>();

            foreach (var aData in data)
            {
                taskList.Add(this.RunProcessDataTask(aData, cancellationToken));
            }

            await Task.WhenAll(taskList);
            this.OnFinish();
        }

        protected virtual async Task RunProcessDataTask(object data, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            try
            {
                await this.ProcessData(data);
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
        }

        protected abstract Task ProcessData(object data);

        protected abstract void Save();

        protected virtual void OnFinish() => this.Save();

        #endregion
    }
}