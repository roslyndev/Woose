using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Woose.Builder.Popup;
using Woose.Data;

namespace Woose.Builder
{
    public partial class MainWindow : Window
    {
        protected SqliteRepository db { get; set; }

        public MainViewModel viewModel { get; set; }

        protected DbContext context { get; set; }


        public MainWindow()
        {
            InitializeComponent();
            this.db = new SqliteRepository();
            this.db.Init();

            viewModel = new MainViewModel();

            DataContext = viewModel;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        #region [ Database Changed ]


        private string selectedDatabaseName = string.Empty;

        public string SelectedDatabaseName
        {
            get { return selectedDatabaseName; }
            set
            {
                if (selectedDatabaseName != value)
                {
                    selectedDatabaseName = value;
                    OnPropertyChanged(nameof(SelectedDatabaseName));
                    OnSelectedDatabaseNameChanged();
                }
            }
        }

        private void OnSelectedDatabaseNameChanged()
        {
            if (!string.IsNullOrWhiteSpace(selectedDatabaseName))
            {
                onLoad();
            }
        }

        #endregion [ Database Changed ]

        #region [ Table Changed ]
        private DbEntity selectedEntity = new DbEntity();

        public DbEntity SelectedEntityName
        {
            get { return selectedEntity; }
            set
            {
                if (selectedEntity != value)
                {
                    selectedEntity = value;
                    OnPropertyChanged(nameof(SelectedEntityName));

                    if (SelectedEntityName != null && !string.IsNullOrWhiteSpace(SelectedEntityName.name))
                    {

                    }
                }
            }
        }

        #endregion [ Table Changed ]

        #region [ SP Changed ]

        private DbEntity selectedSP = new DbEntity();

        public DbEntity SelectedSPName
        {
            get { return selectedSP; }
            set
            {
                if (selectedSP != value)
                {
                    selectedSP = value;
                    OnPropertyChanged(nameof(SelectedSPName));

                    if (SelectedSPName != null && !string.IsNullOrWhiteSpace(SelectedSPName.name))
                    {
                    }
                }
            }
        }


        #endregion [ SP Changed ]


        private void Btn_NewConn_Click(object sender, RoutedEventArgs e)
        {
            ConnectionForm customPopup = new ConnectionForm(this);
            customPopup.Owner = this; // 부모 창 설정 (선택적)
            customPopup.ShowDialog();
        }

        private void Btn_EditConn_Click(object sender, RoutedEventArgs e)
        {
            Database? selectedDatabase = viewModel.SelectedDatabase;
            if (selectedDatabase == null)
            {
                MessageBox.Show("수정할 대상을 선택해 주세요.", "Alert", MessageBoxButton.OK);
            }
            else
            {
                ConnectionForm editForm = new ConnectionForm(this, selectedDatabase);
                editForm.ShowDialog();
            }
        }

        private void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            Database? selectedDatabase = viewModel.SelectedDatabase;
            if (selectedDatabase == null)
            {
                MessageBox.Show("삭제할 대상을 선택해 주세요.", "Alert", MessageBoxButton.OK);
            }
            else
            {
                var result = MessageBox.Show("정말로 삭제하시겠습니까?", "Alert", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    this.db.Deleteatabase(selectedDatabase);
                    this.viewModel.RefreshDatabases();
                }
            }
        }

        private void Btn_Import_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "파일 선택"; // 대화 상자 제목
            openFileDialog.Filter = "JSON 파일|*.json|모든 파일|*.*"; // 파일 필터 (JSON 파일만 또는 모든 파일)
            openFileDialog.CheckFileExists = true; // 파일이 실제로 존재하는지 확인
            openFileDialog.CheckPathExists = true; // 경로가 유효한지 확인

            // 사용자가 파일을 선택하고 확인을 클릭하면
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                try
                {
                    string fileContents = File.ReadAllText(filePath, Encoding.UTF8);
                    if (!string.IsNullOrWhiteSpace(fileContents))
                    {
                        List<Database> list = JsonConvert.DeserializeObject<List<Database>>(fileContents);
                        if (list != null && list.Count > 0)
                        {
                            foreach (var item in list)
                            {
                                this.db.InsertDatabase(item);
                            }

                            MessageBox.Show($"총 {list.Count}개의 Database가 등록되었습니다.", "성공", MessageBoxButton.OK);
                            this.viewModel.RefreshDatabases();
                        }
                        else
                        {
                            MessageBox.Show($"올바른 형식이 아닙니다.", "오류", MessageBoxButton.OK);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"내용이 없습니다.", "오류", MessageBoxButton.OK);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일 읽기 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK);
                }
            }
        }

        private void Btn_Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // 다이얼로그에 표시될 파일 필터를 설정할 수 있습니다.
            saveFileDialog.Filter = "텍스트 파일 (*.json)|*.json|모든 파일 (*.*)|*.*";

            // 초기 디렉터리를 설정할 수 있습니다.
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // 사용자가 파일을 선택하고 확인 버튼을 누르면 저장 경로 및 파일명을 가져옵니다.
            if (saveFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = saveFileDialog.FileName;
                var list = this.db.GetDatabases();
                string jsonString = JsonConvert.SerializeObject(list);
                try
                {
                    File.WriteAllText(selectedFilePath, jsonString, Encoding.UTF8);

                    MessageBox.Show("파일이 성공적으로 저장되었습니다.", "성공", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일 저장 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK);
                }
            }
        }

        private void Btn_Connection_Click(object sender, RoutedEventArgs e)
        {
            Database? selectedDatabase = viewModel.SelectedDatabase;
            if (selectedDatabase == null)
            {
                MessageBox.Show("대상을 선택해 주세요.", "Alert", MessageBoxButton.OK);
            }
            else
            {
                this.context = new DbContext(selectedDatabase.ConnectionString);
                this.onLoad();
            }
        }

        protected void onLoad()
        {
            viewModel.entities.Clear();
            viewModel.sps.Clear();
            
            using (var rep = new SqlServerRepository(this.context))
            {
                var tmp = rep.GetTableEntities();
                if (tmp != null && tmp.Count > 0)
                {
                    foreach (var item in tmp)
                    {
                        viewModel.entities.Add(item);
                    }
                }
                var tmp2 = rep.GetSpEntities();
                if (tmp2 != null && tmp2.Count > 0)
                {
                    foreach (var item in tmp2)
                    {
                        viewModel.sps.Add(item);
                    }
                }
            }
        }

        private void TableListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TableListView.SelectedItem != null)
            {
                SelectedEntityName = (DbEntity)TableListView.SelectedItem;
            }
        }

        private void SPListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SpListView.SelectedItem != null)
            {
                SelectedSPName = (DbEntity)SpListView.SelectedItem;


            }
        }
    }
}