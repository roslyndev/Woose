using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Woose.Builder.Popup;
using Woose.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Woose.Builder
{
    public partial class MainWindow : Window
    {
        private bool IsConnection = false;

        protected SqliteRepository db { get; set; }

        public MainViewModel viewModel { get; set; }

        protected DbContext context { get; set; }

        protected BindOption option { get; set; } = new BindOption();

        public MainWindow()
        {
            InitializeComponent();
            this.db = new SqliteRepository();
            this.db.Init();

            viewModel = new MainViewModel();

            DataContext = viewModel;

            Enables(false);
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

        private DbEntity selectedEntityName;

        public DbEntity SelectedEntityName
        {

            get { return selectedEntityName; }
            set
            {
                if (selectedEntityName != value)
                {
                    selectedEntityName = value;
                    OnPropertyChanged(nameof(SelectedEntityName));

                    if (SelectedEntityName != null && !string.IsNullOrWhiteSpace(SelectedEntityName.name))
                    {

                    }
                }
            }
        }

        #endregion [ Table Changed ]

        #region [ SP Changed ]

        private DbEntity selectedSPName;

        public DbEntity SelectedSPName
        {
            get { return selectedSPName; }
            set
            {
                if (selectedSPName != value)
                {
                    selectedSPName = value;

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

                Enables(true);
            }
        }

        private void TableListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TableListView.SelectedItem != null)
            {
                SelectedEntityName = (DbEntity)TableListView.SelectedItem;
                option.target = SelectedEntityName;
                option.targetType = "TABLE";
            }
        }

        private void SPListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SpListView.SelectedItem != null)
            {
                SelectedSPName = (DbEntity)SpListView.SelectedItem;
                option.target = SelectedSPName;
                option.targetType = "SP";
            }
        }

        private void Btn_Apply_Click(object sender, RoutedEventArgs e)
        {
            TabItem languageTab = Languages.SelectedItem as TabItem;
            if (languageTab != null)
            {
                option.Language = languageTab.Header.ToString();
            }

            TabItem selectedTab;
            string text = string.Empty;

            

            switch ((this.option?.Language ?? "").Trim().ToUpper())
            {
                case "ASP.NET":
                    selectedTab = AspNetOptions.SelectedItem as TabItem;
                    if (selectedTab != null)
                    {
                        option.Category = selectedTab.Header.ToString();
                    }
                    text = option.Binder.Serialize(this.context);

                    switch ((this.option?.Category ?? "").Trim().ToUpper())
                    {
                        case "ENTITY":
                            AspNetEntity.Document = CreateRichText(text);
                            break;
                        case "CONTROLLER":
                            AspNetController.Document = CreateRichText(text);
                            break;
                        case "ABSTRACT":
                            AspNetAbstract.Document = CreateRichText(text);
                            break;
                        case "REPOSITORY":
                            AspNetRepository.Document = CreateRichText(text);
                            break;
                    }
                    break;
                case "DATABASE":
                    selectedTab = SQL.SelectedItem as TabItem;
                    if (selectedTab != null)
                    {
                        option.Category = selectedTab.Header.ToString();
                    }
                    text = option.Binder.Serialize(this.context);

                    switch ((this.option?.Category ?? "").Trim().ToUpper())
                    {
                        case "MSSQL":
                            MsSqlQuery.Document = CreateRichText(text);
                            break;
                        case "MYSQL":
                            MySqlQuery.Document = CreateRichText(text);
                            break;
                        case "MONGODB":
                            MongoDbQuery.Document = CreateRichText(text);
                            break;
                    }
                    break;
                case "TYPESCRIPT":
                    selectedTab = TypeOptions.SelectedItem as TabItem;
                    if (selectedTab != null)
                    {
                        option.Category = selectedTab.Header.ToString();
                    }
                    text = option.Binder.Serialize(this.context);

                    switch ((this.option?.Category ?? "").Trim().ToUpper())
                    {
                        case "ENTITY":
                            TsEntity.Document = CreateRichText(text);
                            break;
                        case "CONTROLLER":
                            TsController.Document = CreateRichText(text);
                            break;
                        case "ABSTRACT":
                            TsController.Document = CreateRichText(text);
                            break;
                        case "REPOSITORY":
                            TsRepository.Document = CreateRichText(text);
                            break;
                    }
                    break;
                case "NODE.JS":
                    selectedTab = NodeOptions.SelectedItem as TabItem;
                    if (selectedTab != null)
                    {
                        option.Category = selectedTab.Header.ToString();
                    }
                    text = option.Binder.Serialize(this.context);

                    switch ((this.option?.Category ?? "").Trim().ToUpper())
                    {
                        case "ENTITY":
                            JsEntity.Document = CreateRichText(text);
                            break;
                        case "CONTROLLER":
                            JsController.Document = CreateRichText(text);
                            break;
                        case "ABSTRACT":
                            JsController.Document = CreateRichText(text);
                            break;
                        case "REPOSITORY":
                            JsRepository.Document = CreateRichText(text);
                            break;
                    }
                    break;
            }
        }

        private FlowDocument CreateRichText(string text)
        {
            return new FlowDocument(new Paragraph(new Run(text)));
        }

        private void Btn_CodeCopy_Click(object sender, RoutedEventArgs e)
        {
            TabItem languageTab = Languages.SelectedItem as TabItem; // 현재 선택된 탭을 가져옵니다.

            if (languageTab != null)
            {
                switch (languageTab.Header.ToString())
                {
                    case "ASP.NET":
                        TabItem selectedTab = AspNetOptions.SelectedItem as TabItem;
                        if (selectedTab != null)
                        {
                            RichTextBox selectedEditor = selectedTab.Content as RichTextBox; // 선택한 탭의 RichTextBox를 가져옵니다.

                            if (selectedEditor != null)
                            {
                                // RichTextBox의 내용을 클립보드에 복사합니다.
                                Clipboard.SetText(new TextRange(selectedEditor.Document.ContentStart, selectedEditor.Document.ContentEnd).Text);
                                MessageBox.Show("복사했습니다.", "Success", MessageBoxButton.OK);
                            }
                        }
                        break;
                    default:
                        MessageBox.Show("대상을 선택해 주세요.", "Warning", MessageBoxButton.OK);
                        break;
                }
            }


        }

        private void Btn_NameCopy_Click(object sender, RoutedEventArgs e)
        {
            if (option != null && option.target != null && !string.IsNullOrWhiteSpace(option.target.name))
            {
                Clipboard.SetText(option.target.name);
                MessageBox.Show("복사했습니다.", "Success", MessageBoxButton.OK);
            }
        }

        private void Btn_Reload_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_AlterProject_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsConnection)
            {
                string selectedFolderPath = ShowFolderDialog();

                if (!string.IsNullOrEmpty(selectedFolderPath))
                {
                    if (!Directory.Exists(selectedFolderPath) || !File.Exists(selectedFolderPath + "\\appsettings.json"))
                    {
                        MessageBox.Show($"프로젝트 설정파일을 찾을 수 없습니다.");
                    }
                    else
                    {
                        try
                        {
                            option.ProjectName = GetProjectName(selectedFolderPath);
                            option.MethodName = GetMethodName(selectedFolderPath);
                            string jsonConfig = File.ReadAllText($"{selectedFolderPath}\\appsettings.json");
                            AppSettings app = new AppSettings();
                            if (!string.IsNullOrWhiteSpace(jsonConfig))
                            {
                                app = JsonConvert.DeserializeObject<AppSettings>(jsonConfig);
                                if (app == null)
                                {
                                    app = new AppSettings();
                                }
                            }

                            app.AppName = option.ProjectName;
                            SqlConnectionStringBuilder sql = new SqlConnectionStringBuilder(this.context.GetConnectionString);
                            sql.ApplicationName = option.ProjectName.Replace(".", "");
                            app.Database.ConnectionString = sql.ConnectionString;
                            app.Config.AppID = option.ProjectName.Replace(".","");
                            app.Config.CookieVar = $"{option.ProjectName}Token";
                            File.WriteAllText($"{selectedFolderPath}\\appsettings.json", JsonConvert.SerializeObject(app));
                            
                            CSharpCreater creater = new CSharpCreater();
                            File.WriteAllText($"{selectedFolderPath}\\Program.cs", creater.CreateProgram(option));

                            if (!Directory.Exists($"{selectedFolderPath}\\Entities"))
                            {
                                Directory.CreateDirectory($"{selectedFolderPath}\\Entities");
                            }

                            if (!Directory.Exists($"{selectedFolderPath}\\Controllers"))
                            {
                                Directory.CreateDirectory($"{selectedFolderPath}\\Controllers");
                            }

                            if (!Directory.Exists($"{selectedFolderPath}\\Repositories"))
                            {
                                Directory.CreateDirectory($"{selectedFolderPath}\\Repositories");
                            }

                            if (!Directory.Exists($"{selectedFolderPath}\\Abstracts"))
                            {
                                Directory.CreateDirectory($"{selectedFolderPath}\\Abstracts");
                            }

                            if (!Directory.Exists($"{selectedFolderPath}\\Models"))
                            {
                                Directory.CreateDirectory($"{selectedFolderPath}\\Models");
                            }

                            if (!Directory.Exists($"{selectedFolderPath}\\Models\\Parameters"))
                            {
                                Directory.CreateDirectory($"{selectedFolderPath}\\Models\\Parameters");
                            }

                            if (File.Exists($"{selectedFolderPath}\\Controllers\\WeatherForecastController.cs"))
                            {
                                File.Delete($"{selectedFolderPath}\\Controllers\\WeatherForecastController.cs");
                            }

                            if (File.Exists($"{selectedFolderPath}\\WeatherForecast.cs"))
                            {
                                File.Delete($"{selectedFolderPath}\\WeatherForecast.cs");
                            }

                            using (var rep = new SqlServerRepository(context))
                            {
                                foreach (var entity in this.viewModel.entities)
                                {
                                    var list = rep.GetTableProperties(entity.name);
                                    if (!File.Exists($"{selectedFolderPath}\\Entities\\{entity.name}.cs"))
                                    {
                                        File.Create($"{selectedFolderPath}\\Entities\\{entity.name}.cs").Close();
                                    }
                                    File.WriteAllText($"{selectedFolderPath}\\Entities\\{entity.name}.cs", creater.CreateEntity(option, entity, list, true));

                                    if (!File.Exists($"{selectedFolderPath}\\Controllers\\{entity.name}Controllers.cs"))
                                    {
                                        File.Create($"{selectedFolderPath}\\Controllers\\{entity.name}Controllers.cs").Close();
                                    }
                                    File.WriteAllText($"{selectedFolderPath}\\Controllers\\{entity.name}Controllers.cs", creater.CreateController(option, entity, list, true));

                                    if (!File.Exists($"{selectedFolderPath}\\Abstracts\\I{entity.name}Repository.cs"))
                                    {
                                        File.Create($"{selectedFolderPath}\\Abstracts\\I{entity.name}Repository.cs").Close();
                                    }
                                    File.WriteAllText($"{selectedFolderPath}\\Abstracts\\I{entity.name}Repository.cs", creater.CreateAbstract(option, entity, list, true));

                                    if (!File.Exists($"{selectedFolderPath}\\Repositories\\{entity.name}Repository.cs"))
                                    {
                                        File.Create($"{selectedFolderPath}\\Repositories\\{entity.name}Repository.cs").Close();
                                    }
                                    File.WriteAllText($"{selectedFolderPath}\\Repositories\\{entity.name}Repository.cs", creater.CreateRepository(option, entity, list, true));
                                }

                                foreach (var sp in this.viewModel.sps)
                                {
                                    var inputs = rep.GetSpProperties(sp.name);

                                    if (!File.Exists($"{selectedFolderPath}\\Models\\Parameters\\Input{GetNameFromSP(sp.name)}Parameter.cs"))
                                    {
                                        File.Create($"{selectedFolderPath}\\Models\\Parameters\\Input{GetNameFromSP(sp.name)}Parameter.cs").Close();
                                    }
                                    File.WriteAllText($"{selectedFolderPath}\\Models\\Parameters\\Input{GetNameFromSP(sp.name)}Parameter.cs", creater.CreateParameter(option, sp, inputs, true));

                                    var outputs = rep.GetSpOutput(sp.name);
                                    if (outputs != null && !(outputs.Where(x => x.name.Equals("IsError", StringComparison.OrdinalIgnoreCase)).Count() > 0))
                                    {
                                        if (!File.Exists($"{selectedFolderPath}\\Models\\Parameters\\Output{GetNameFromSP(sp.name)}Parameter.cs"))
                                        {
                                            File.Create($"{selectedFolderPath}\\Models\\Parameters\\Output{GetNameFromSP(sp.name)}Parameter.cs").Close();
                                        }
                                        File.WriteAllText($"{selectedFolderPath}\\Models\\Parameters\\Output{GetNameFromSP(sp.name)}Parameter.cs", creater.CreateParameter(option, sp, outputs, true));
                                    }
                                }
                            }

                            if (!File.Exists($"{selectedFolderPath}\\Abstracts\\I{option.MethodName}Repository.cs"))
                            {
                                File.Create($"{selectedFolderPath}\\Abstracts\\I{option.MethodName}Repository.cs").Close();
                            }
                            File.WriteAllText($"{selectedFolderPath}\\Abstracts\\I{option.MethodName}Repository.cs", creater.CreateAbstract(option, this.context, this.viewModel.sps.ToList(), true));

                            if (!File.Exists($"{selectedFolderPath}\\Repositories\\{option.MethodName}Repository.cs"))
                            {
                                File.Create($"{selectedFolderPath}\\Repositories\\{option.MethodName}Repository.cs").Close();
                            }
                            File.WriteAllText($"{selectedFolderPath}\\Repositories\\{option.MethodName}Repository.cs", creater.CreateRepository(option, this.context, this.viewModel.sps.ToList(), true));


                            MessageBox.Show($"프로젝트가 수정되었습니다.");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message.ToString());
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show($"Database 연결을 먼저 진행해 주세요.");
            }
        }

        private string ShowFolderDialog()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Select Folder",
                FileName = "FolderSelection", // Default file name
                Filter = "Folders|*.thisDoesNotExist", // Forces the dialog to open in folder selection mode
                CheckFileExists = false,
                CheckPathExists = true,
                RestoreDirectory = true,
            };

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                return System.IO.Path.GetDirectoryName(dialog.FileName);
            }

            return string.Empty;
        }
        
        public void Enables(bool enable)
        {
            Tabs_Db.IsEnabled = enable;
            Languages.IsEnabled = enable;
            this.IsConnection = enable;

            if (this.IsConnection)
            {
                Btn_Apply.Style = (Style)FindResource("MintButton");
                Btn_AlterProject.Style = (Style)FindResource("OrangeButton");
            }
            else
            {
                Btn_Apply.Style = (Style)FindResource("GrayButton");
                Btn_AlterProject.Style = (Style)FindResource("GrayButton");
            }
        }

        public string GetProjectName(string folderPath)
        {
            string result = folderPath;

            if (!string.IsNullOrWhiteSpace(folderPath))
            {
                if (folderPath.IndexOf('\\') > -1)
                {
                    string[] arr = folderPath.Trim().Split('\\');
                    result = arr[arr.Length - 1].Trim();
                }
            }

            return result;
        }

        public string GetMethodName(string folderPath)
        {
            string result = this.GetProjectName(folderPath);

            if (!string.IsNullOrWhiteSpace(folderPath))
            {
                if (folderPath.IndexOf('.') > -1)
                {
                    string[] arr = folderPath.Trim().Split('.');
                    result = arr[arr.Length - 1].Trim();
                }
            }

            return result;
        }

        private string GetNameFromSP(string spname)
        {
            string result = string.Empty;

            if (!string.IsNullOrWhiteSpace(spname))
            {
                if (spname.IndexOf("_") > -1)
                {
                    string[] arr = spname.Split('_');
                    if (arr.Length > 1)
                    {
                        result = "";
                        for (int i = 1; i < arr.Length; i++)
                        {
                            result += arr[i];
                        }
                    }
                    else
                    {
                        result = spname.Replace("_", "");
                    }
                }
            }

            return result;
        }

        private void CheckNoModel_Checked(object sender, RoutedEventArgs e)
        {
            option.IsNoModel = true;
        }

        private void CheckNoModel_Unchecked(object sender, RoutedEventArgs e)
        {
            option.IsNoModel = false;
        }

        private void ReturnType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedItem != null)
            {
                ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;
                option.ReturnType = (selectedItem != null && selectedItem.Content != null) ? selectedItem.Content.ToString() : "";
            }
        }

        private void BindModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedItem != null)
            {
                ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;
                option.BindModel = (selectedItem != null && selectedItem.Content != null) ? selectedItem.Content.ToString() : "";
            }
        }

        private void CheckUseCustomModel_Checked(object sender, RoutedEventArgs e)
        {
            option.UsingCustomModel = true;
        }

        private void CheckUseCustomModel_Unchecked(object sender, RoutedEventArgs e)
        {
            option.UsingCustomModel = false;
        }

        private void MethodType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedItem != null)
            {
                ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;
                option.MethodType = (selectedItem != null && selectedItem.Content != null) ? selectedItem.Content.ToString() : "";
            }
        }
    }
}