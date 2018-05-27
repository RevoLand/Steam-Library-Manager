using System;
using System.Collections.ObjectModel;
using System.Threading;

// https://gist.github.com/thomaslevesque/10023516

namespace Steam_Library_Manager.Framework
{
    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current;

        private void ExecuteOnSyncContext(Action action)
        {
            try
            {
                if (SynchronizationContext.Current == _synchronizationContext)
                {
                    action();
                }
                else
                {
                    _synchronizationContext.Send(_ => action(), null);
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }

        protected override void InsertItem(int index, T item)
        {
            try
            {
                ExecuteOnSyncContext(() => base.InsertItem(index, item));
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }

        protected override void ClearItems() => ExecuteOnSyncContext(() => base.ClearItems());
    }
}