using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MainDialog.Models
{
    public class ChangeItem : INotifyPropertyChanged
    {

        private string filePath;

        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; NotifyPropertyChanged(); }
        }


        private DateTimeOffset timestamp = DateTimeOffset.UtcNow;

        public DateTimeOffset Timestamp
        {
            get { return timestamp; }
            set
            {
                timestamp = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(DisplayStamp));
            }
        }

        public string DisplayStamp
        {
            get
            {
                return Timestamp.LocalDateTime.ToString();
            }
        }

        private WatcherChangeTypes changeType;

        public WatcherChangeTypes ChangeType
        {
            get { return changeType; }
            set
            {
                changeType = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(DisplayChange));
            }
        }

        public string DisplayChange
        {
            get
            {
                //if (changeMap.ContainsKey(ChangeType)) return changeMap[ChangeType];
                if (changeMap.ContainsKey(ChangeType))
                {
                    return Enum.GetName(ChangeType);
                }

                return ChangeType == default ? "" : ChangeType.ToString();
            }
        }

        private string changeDetails = String.Empty;

        public string ChangeDetails
        {
            get { return changeDetails; }
            set { changeDetails = value; NotifyPropertyChanged(); }
        }


        private static Dictionary<WatcherChangeTypes, string> changeMap => new Dictionary<WatcherChangeTypes, string>
        {
            { WatcherChangeTypes.Created, "created" },
            { WatcherChangeTypes.Deleted, "deleted" },
            { WatcherChangeTypes.Changed, "changed" },
            { WatcherChangeTypes.Renamed, "renamed" },
        };
        
        public ChangeItem()
        {

        }

        public ChangeItem(string FilePath)
        {
            this.FilePath = FilePath;
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
