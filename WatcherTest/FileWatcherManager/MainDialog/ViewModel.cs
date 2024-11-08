﻿using MainDialog.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MainDialog
{
    public class ViewModel : INotifyPropertyChanged, IDisposable
    {
        private const int eventDelay = 500;
        private DateTimeOffset? lastEvent { get; set; }

        private FileSystemWatcher? watcher;

        public FileSystemWatcher Watcher
        {
            get { return watcher; }
            set
            {
                if (value == null)
                {
                    watcher?.Dispose();
                    WatchedPath = "";
                }
                if (watcher != null)
                {
                    Unsubscribe(watcher);
                }
                watcher = value;
                NotifyPropertyChanged();
                if (watcher == null) return;
                Subscribe(watcher);
            }
        }

        private string watchedPath;

        public string WatchedPath
        {
            get { return watchedPath; }
            set { watchedPath = value; NotifyPropertyChanged(); }
        }

        //public string WatchedPath => WatchedFile?.FullName ?? "";

        //public bool IsWatchedPathValid => !String.IsNullOrWhiteSpace(watchedPath) && Path.Exists(WatchedPath);
        public bool IsWatchedPathValid => !String.IsNullOrWhiteSpace(WatchedPath) && File.Exists(WatchedPath);

        public ObservableCollection<ChangeItem> ChangeItemList { get; set; } = new ObservableCollection<ChangeItem>();

        public ViewModel()
        {
            if (ChangeItemList != null) ChangeItemList.CollectionChanged += OnChangeItemListModified;
        }

        private void CreateWatcher(string initPath)
        {
            try
            {
                Watcher?.Dispose();

                if (String.IsNullOrWhiteSpace(initPath)) return;

                if (File.Exists(initPath))
                {

                    var pathInfo = new FileInfo(initPath);

                    Environment.CurrentDirectory = pathInfo.DirectoryName;

                    Watcher = new FileSystemWatcher(pathInfo.DirectoryName)
                    {
                        //NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Attributes,
                        NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                        Filter = pathInfo.Name,
                        EnableRaisingEvents = true,
                    };

                }
                else if (Path.Exists(initPath))
                {
                    var pathInfo = new DirectoryInfo(initPath);

                    Environment.CurrentDirectory = pathInfo.FullName;

                    Watcher = new FileSystemWatcher(pathInfo.FullName)
                    {
                        NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Attributes,
                        Filter = "*.*",
                        IncludeSubdirectories = true,
                        EnableRaisingEvents = true,
                    };

                }

                
            }
            catch(Exception ex)
            {
                var flag = 0;
            }
        }

        public void WatchPath(string newPath)
        {
            try
            {
                Watcher?.Dispose();
                WatchedPath = "";

                if (!(File.Exists(newPath) || Directory.Exists(newPath))) return;

                CreateWatcher(newPath);
                WatchedPath = newPath;

                if(Debounce()) return;

                lastEvent = DateTimeOffset.UtcNow;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ChangeItemList.Add(new ChangeItem
                    {
                        Timestamp = DateTimeOffset.UtcNow,
                        ChangeDetails = "Begin Monitoring",
                    });
                });

            }
            catch(Exception ex)
            {

            }

        }
        private void Subscribe(FileSystemWatcher? sysWatcher)
        {
            if (sysWatcher == null) return;

            sysWatcher.Changed += OnChanged;
            sysWatcher.Deleted += OnChanged;
            sysWatcher.Renamed += OnRenamed;
        }

        private void Unsubscribe(FileSystemWatcher? sysWatcher)
        {
            if (sysWatcher == null) return;

            sysWatcher.Changed -= OnChanged;
            sysWatcher.Deleted -= OnChanged;
            sysWatcher.Renamed -= OnRenamed;
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            //if (Debounce()) return;

            lastEvent = DateTimeOffset.UtcNow;
            Application.Current.Dispatcher.Invoke(() =>
            {
                ChangeItemList.Add(new ChangeItem()
                {
                    Timestamp = DateTimeOffset.UtcNow,
                    ChangeType = e.ChangeType,
                    ChangeDetails = $"Monitoring: {e.OldName} -> {e.Name}",
                });
            });

            WatchPath(e.FullPath);
            //WatchedPath = e.FullPath;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            //if (ChangeItemList?.LastOrDefault()?.Timestamp.AddMilliseconds(eventDelay) > DateTimeOffset.UtcNow) return;
            if (Debounce()) return;

            if(e.ChangeType == WatcherChangeTypes.Deleted)
            {
                WatchPath("");
            }
            else
            {
                lastEvent = DateTimeOffset.UtcNow;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ChangeItemList.Add(new ChangeItem()
                    {
                        Timestamp = DateTimeOffset.UtcNow,
                        ChangeType = e.ChangeType,
                    });
                });
                
            }
        }

        private bool Debounce()
        {
            return lastEvent?.AddMilliseconds(eventDelay) > DateTimeOffset.UtcNow;
        }

        private void ChangeItemModified(object? sender, PropertyChangedEventArgs e)
        {
            if(sender is ChangeItem changedItem)
            {

            }
        }

        private void OnChangeItemListModified(object? sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(ChangeItemList));
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                e.NewItems?.OfType<ChangeItem>().ToList().ForEach(i => i.PropertyChanged += ChangeItemModified);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                e.OldItems?.OfType<ChangeItem>().ToList().ForEach(i => i.PropertyChanged -= ChangeItemModified);
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                e.OldItems?.OfType<ChangeItem>().ToList().ForEach(i => i.PropertyChanged -= ChangeItemModified);
                e.NewItems?.OfType<ChangeItem>().ToList().ForEach(i => i.PropertyChanged += ChangeItemModified);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset && sender is ObservableCollection<ChangeItem> coll)
            {
                coll?.ToList().ForEach(i => i.PropertyChanged += ChangeItemModified);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public void Dispose()
        {
            Unsubscribe(Watcher);
        }
    }
}
