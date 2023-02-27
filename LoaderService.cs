using Mavericks.Catalogue.Client.Models;
using System;
using System.Collections.Generic;

namespace Mavericks.Catalogue.Client.Services
{
    public class LoaderService : ILoaderService
    {
        private int loaderId;
        private readonly List<int> shownLoaders = new();
        public event Action<bool>? OnChange;

        public void Hide(int loaderId)
        {
            RemoveLoaderId(loaderId);
        }

        public IDisposable Show()
        {
            loaderId++;
            AddLoaderId(loaderId);
            return new LoaderInstance(this, loaderId);
        }

        private void AddLoaderId(int loaderId)
        {
            shownLoaders.Add(loaderId);
            if (shownLoaders.Count == 1)
            {
                OnChange?.Invoke(true);
            }
        }

        private void RemoveLoaderId(int loaderId)
        {
            shownLoaders.Remove(loaderId);
            if (shownLoaders.Count == 0)
            {
                OnChange?.Invoke(false);
                this.loaderId = 0;
            }
        }
    }
}
