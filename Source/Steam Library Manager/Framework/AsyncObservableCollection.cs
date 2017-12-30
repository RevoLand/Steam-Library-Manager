using System;
using System.Collections.ObjectModel;
using System.Threading;

// https://gist.github.com/thomaslevesque/10023516

namespace Steam_Library_Manager.Framework
{
    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
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
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, ex.ToString());
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
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, ex.ToString());
            }
        }

        protected override void ClearItems() => ExecuteOnSyncContext(() => base.ClearItems());
    }
}
