using System.Collections.ObjectModel;
using System.ComponentModel;
using Woose.Core;

namespace Woose.Builder
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public ObservableCollection<DbEntity> entities { get; set; } = new ObservableCollection<DbEntity>();

        public ObservableCollection<DbEntity> sps { get; set; } = new ObservableCollection<DbEntity>();


        private List<Database> _databases;

        public List<Database> Databases
        {
            get { return _databases; }
            set
            {
                if (_databases != value)
                {
                    _databases = value;
                    OnPropertyChanged(nameof(Databases));
                }
            }
        }

        private Database? _selectedDatabase;

        public Database? SelectedDatabase
        {
            get { return _selectedDatabase; }
            set
            {
                if (_selectedDatabase != value)
                {
                    _selectedDatabase = value;
                    OnPropertyChanged(nameof(SelectedDatabase));
                }
            }
        }

        public MainViewModel()
        {
            using (var db = new SqliteRepository())
            {
                var rtn = db.GetDatabases();
                if (rtn.Check)
                {
                    this.Databases = rtn.Data;
                }
            }
        }

        public void RefreshDatabases()
        {
            using (var db = new SqliteRepository())
            {
                var rtn = db.GetDatabases();
                if (rtn.Check)
                {
                    this.Databases = rtn.Data;
                }
            }
        }

        
    }

}
