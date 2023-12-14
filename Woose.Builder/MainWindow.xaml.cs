using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Linq;
using Woose.Builder.Popup;
using Woose.Core;
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

        protected LoadingWindow? loading = null;

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

        private Thread? worker = null;


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

                Loading(true);

                worker = new Thread(new ParameterizedThreadStart((path) => {
                    Btn_Import_Proc(path);

                    Dispatcher.Invoke(() =>
                    {
                        worker = null;
                        Loading(false);
                    });
                }));

                worker.Start(filePath);
            }
        }

        private void Btn_Import_Proc(object paramData)
        {
            try
            {
                string filePath = Convert.ToString(paramData);
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
                Loading(true);

                worker = new Thread(new ParameterizedThreadStart((path) => {
                    string selectedFilePath = Convert.ToString(path);
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

                    Dispatcher.Invoke(() =>
                    {
                        worker = null;
                        Loading(false);
                    });
                }));

                worker.Start(saveFileDialog.FileName);
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
                this.option.Database = selectedDatabase;
                this.context = new DbContext(selectedDatabase.ConnectionString);
                this.onLoad();
            }
        }

        protected void onLoad()
        {
            Loading(true);

            viewModel.entities.Clear();
            viewModel.sps.Clear();

            worker = new Thread(new ParameterizedThreadStart((main) => {
                MainWindow target = main as MainWindow;
                if (target != null)
                {
                    var tableproperties = new List<DbTableInfo>();
                    var spproperties = new List<SPEntity>();
                    var spoutput = new List<SpOutput>();
                    var sptables = new List<SpTable>();

                    using (var rep = new SqlServerRepository(target.context))
                    {
                        target.option.tables = rep.GetTableEntities();
                        if (target.option.tables != null && target.option.tables.Count > 0)
                        {
                            foreach (var item in target.option.tables)
                            {
                                tableproperties = rep.GetTableProperties(item.name);
                                target.option.tableProperties.AddOrUpdate(item.name, tableproperties, (x, y) => tableproperties);

                                Dispatcher.Invoke(() =>
                                {
                                    viewModel.entities.Add(item);
                                });
                            }
                        }

                        target.option.sps = rep.GetSpEntities();
                        if (target.option.sps != null && target.option.sps.Count > 0)
                        {
                            foreach (var item in target.option.sps)
                            {
                                spproperties = rep.GetSpProperties(item.name);
                                spoutput = rep.GetSpOutput(item.name);
                                sptables = rep.GetSPTables(item.name);
                                
                                target.option.spProperties.AddOrUpdate(item.name, spproperties, (x, y) => spproperties);
                                target.option.spOutputs.AddOrUpdate(item.name, spoutput, (x, y) => spoutput);
                                target.option.spTables.AddOrUpdate(item.name, sptables, (x, y) => sptables);

                                Dispatcher.Invoke(() =>
                                {
                                    viewModel.sps.Add(item);
                                });
                            }
                        }
                    }
                }

                Dispatcher.Invoke(() =>
                {
                    worker = null;
                    MessageBox.Show($"연결구성이 완료되었습니다.");
                    Enables(true);
                    Loading(false);
                });
            }));

            worker.Start(this);
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
                case "JAVA":
                    selectedTab = JavaOptions.SelectedItem as TabItem;
                    if (selectedTab != null)
                    {
                        option.Category = selectedTab.Header.ToString();
                    }
                    text = option.Binder.Serialize(this.context);

                    switch ((this.option?.Category ?? "").Trim().ToUpper())
                    {
                        case "ENTITY":
                            JavaEntity.Document = CreateRichText(text);
                            break;
                        case "CONTROLLER":
                            JavaController.Document = CreateRichText(text);
                            break;
                        case "ABSTRACT":
                            JavaAbstract.Document = CreateRichText(text);
                            break;
                        case "REPOSITORY":
                            JavaRepository.Document = CreateRichText(text);
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
                        case "YAML":
                            YAMLBox.Document = CreateRichText(text);
                            break;
                    }
                    break;
                case "VUE.JS":
                    selectedTab = VueOptions.SelectedItem as TabItem;
                    if (selectedTab != null)
                    {
                        option.Category = selectedTab.Header.ToString();
                    }
                    text = option.Binder.Serialize(this.context);

                    switch ((this.option?.Category ?? "").Trim().ToUpper())
                    {
                        case "FORM":
                            VueForm.Document = CreateRichText(text);
                            break;
                        case "COMPONENT":
                            VueComponent.Document = CreateRichText(text);
                            break;
                    }
                    break;
                case "REACT.JS":
                    selectedTab = ReactOptions.SelectedItem as TabItem;
                    if (selectedTab != null)
                    {
                        option.Category = selectedTab.Header.ToString();
                    }
                    text = option.Binder.Serialize(this.context);

                    switch ((this.option?.Category ?? "").Trim().ToUpper())
                    {
                        case "FORM":
                            ReactForm.Document = CreateRichText(text);
                            break;
                        case "COMPONENT":
                            ReactComponent.Document = CreateRichText(text);
                            break;
                    }
                    break;
                case "HTML":
                    selectedTab = HtmlOptions.SelectedItem as TabItem;
                    if (selectedTab != null)
                    {
                        option.Category = selectedTab.Header.ToString();
                    }
                    text = option.Binder.Serialize(this.context);

                    switch ((this.option?.Category ?? "").Trim().ToUpper())
                    {
                        case "TAILWINDCSSFORM":
                            TailwindCssFormBox.Document = CreateRichText(text);
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
            RichTextBox selectedEditor = GetCurrentBox();
            if (selectedEditor != null)
            {
                Clipboard.SetText(new TextRange(selectedEditor.Document.ContentStart, selectedEditor.Document.ContentEnd).Text);
                MessageBox.Show("복사했습니다.", "Success", MessageBoxButton.OK);
            }
            else
            {
                MessageBox.Show("대상을 선택해 주세요.", "Warning", MessageBoxButton.OK);
            }
        }

        private void Btn_NameCopy_Click(object sender, RoutedEventArgs e)
        {
            if (option != null && option.target != null && !string.IsNullOrWhiteSpace(option.target.name))
            {
                TabItem languageTab = Languages.SelectedItem as TabItem;
                if (languageTab != null)
                {
                    option.Language = languageTab.Header.ToString();
                }

                TabItem selectedTab;
                string filename = string.Empty;

                switch ((this.option?.Language ?? "").Trim().ToUpper())
                {
                    case "ASP.NET":
                        filename = $"{option.target.name}";
                        break;
                    case "JAVA":
                        filename = $"{option.target.name}";
                        break;
                    case "DATABASE":
                        filename = $"USP_{option.target.name}_Save";
                        break;
                    case "TYPESCRIPT":
                        filename = $"{option.target.name}";
                        break;
                    case "NODE.JS":
                        filename = $"{option.target.name}";
                        break;
                    case "VUE.JS":
                        filename = $"{option.target.name}";
                        break;
                    case "REACT.JS":
                        filename = $"{option.target.name}";
                        break;
                    case "HTML":
                        filename = $"{option.target.name}";
                        break;
                }
                Clipboard.SetText(filename);
                MessageBox.Show("복사했습니다.", "Success", MessageBoxButton.OK);
            }
        }

        private void Btn_Reload_Click(object sender, RoutedEventArgs e)
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

        private void Btn_AlterProject_Click(object sender, RoutedEventArgs e)
        {
            if (worker != null && worker.ThreadState != ThreadState.Running)
            {
                MessageBox.Show($"현재 작업중입니다. 잠시만 기다려 주세요.");
            }
            else
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
                            Loading(true);

                            worker = new Thread(new ParameterizedThreadStart(async (path) => {
                                await Task.Run(() => Btn_AlterProject_Click1(path)).ConfigureAwait(false);
                                await Task.Run(() => Btn_AlterProject_Click2(path)).ConfigureAwait(false);
                                await Task.Run(() => Btn_AlterProject_Click3(path)).ConfigureAwait(false);

                                Dispatcher.Invoke(() =>
                                {
                                    worker = null;
                                    MessageBox.Show($"프로젝트가 수정되었습니다.");
                                    Loading(false);
                                });
                            }));

                            worker.Start(selectedFolderPath);
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"Database 연결을 먼저 진행해 주세요.");
                }
            }
        }

        private void Btn_CreateAllSpFile_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsConnection)
            {
                string selectedFolderPath = ShowFolderDialog();

                if (!string.IsNullOrEmpty(selectedFolderPath))
                {
                    if (!Directory.Exists(selectedFolderPath))
                    {
                        MessageBox.Show($"대상 위치를 찾을 수 없습니다.");
                    }
                    else
                    {
                        Loading(true);

                        worker = new Thread(new ParameterizedThreadStart((path) => {
                            Btn_CreateAllSpFile_Click_Async(path).Wait();

                            Dispatcher.Invoke(() =>
                            {
                                worker = null;
                                Loading(false);
                            });
                        }));

                        worker.Start(selectedFolderPath);
                    }
                }
            }
            else
            {
                MessageBox.Show($"Database 연결을 먼저 진행해 주세요.");
            }
        }

        private async Task Btn_CreateAllSpFile_Click_Async(object paramData)
        {
            try
            {
                string selectedFolderPath = Convert.ToString(paramData);
                MsSqlCreater creater = new MsSqlCreater();

                await Task.Factory.StartNew(() =>
                {
                    if (!Directory.Exists($"{selectedFolderPath}\\StoredProcedures"))
                    {
                        Directory.CreateDirectory($"{selectedFolderPath}\\StoredProcedures");
                    }
                }).ConfigureAwait(false);

                using (var rep = new SqlServerRepository(context))
                {
                    foreach (var entity in this.viewModel.entities)
                    {
                        var list = rep.GetTableProperties(entity.name);

                        await Task.Factory.StartNew(() =>
                        {
                            if (!File.Exists($"{selectedFolderPath}\\StoredProcedures\\USP_{entity.name}_Save.sql"))
                            {
                                File.Create($"{selectedFolderPath}\\StoredProcedures\\USP_{entity.name}_Save.sql").Close();
                            }
                            File.WriteAllText($"{selectedFolderPath}\\StoredProcedures\\USP_{entity.name}_Save.sql", creater.CreateSaveSP(option, list));
                        }).ConfigureAwait(false);
                    }
                }

                MessageBox.Show($"SP가 모두 작성되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }


        private void Btn_CreateSpFile_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsConnection)
            {
                if (option.target != null && !string.IsNullOrWhiteSpace(option.target.name))
                {
                    string selectedFolderPath = ShowFolderDialog();

                    if (!string.IsNullOrEmpty(selectedFolderPath))
                    {
                        if (!Directory.Exists(selectedFolderPath))
                        {
                            MessageBox.Show($"대상 위치를 찾을 수 없습니다.");
                        }
                        else
                        {
                            Loading(true);

                            worker = new Thread(new ParameterizedThreadStart((path) => {
                                try
                                {
                                    string FolderPath = Convert.ToString(path);
                                    MsSqlCreater creater = new MsSqlCreater();

                                    if (!Directory.Exists($"{FolderPath}\\StoredProcedures"))
                                    {
                                        Directory.CreateDirectory($"{FolderPath}\\StoredProcedures");
                                    }

                                    using (var rep = new SqlServerRepository(context))
                                    {
                                        var list = rep.GetTableProperties(option.target.name);

                                        if (!File.Exists($"{FolderPath}\\StoredProcedures\\USP_{option.target.name}_Save.sql"))
                                        {
                                            File.Create($"{FolderPath}\\StoredProcedures\\USP_{option.target.name}_Save.sql").Close();
                                        }
                                        File.WriteAllText($"{FolderPath}\\StoredProcedures\\USP_{option.target.name}_Save.sql", creater.CreateSaveSP(option, list));
                                    }

                                    MessageBox.Show($"SP가 작성되었습니다.");
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message.ToString());
                                }

                                Dispatcher.Invoke(() =>
                                {
                                    worker = null;
                                    Loading(false);
                                });
                            }));

                            worker.Start(selectedFolderPath);
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"대상을 선택해 주세요.");
                }
            }
            else
            {
                MessageBox.Show($"Database 연결을 먼저 진행해 주세요.");
            }
        }

        private void Loading(bool chk)
        {
            viewModel.IsProc = chk;
            AllEnables(!chk, this);
        }

        private void Btn_AlterProject_Click1(object paramData)
        {
            try
            {
                string selectedFolderPath = Convert.ToString(paramData);
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
                app.Config.AppID = option.ProjectName.Replace(".", "");
                app.Config.CookieVar = $"{option.ProjectName}Token";
                app.ServerToken = CryptoHelper.SHA256.Encrypt($"W{option.ProjectName.Trim().ToUpper()}O{DateTime.Now.ToString("YYMMDD")}S");
                File.WriteAllText($"{selectedFolderPath}\\appsettings.json", JsonConvert.SerializeObject(app));

                CSharpCreater creater = new CSharpCreater();
                File.WriteAllText($"{selectedFolderPath}\\Program.cs", creater.CreateProgram(option, this.viewModel.entities.ToList()));

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

                if (!File.Exists($"{selectedFolderPath}\\Controllers\\DefaultControllers.cs"))
                {
                    File.Create($"{selectedFolderPath}\\Controllers\\DefaultControllers.cs").Close();
                }
                File.WriteAllText($"{selectedFolderPath}\\Controllers\\DefaultControllers.cs", creater.CreateDefaultController(option, true));

                if (!File.Exists($"{selectedFolderPath}\\Controllers\\{option.MethodName}ProcController.cs"))
                {
                    File.Create($"{selectedFolderPath}\\Controllers\\{option.MethodName}ProcController.cs").Close();
                }
                File.WriteAllText($"{selectedFolderPath}\\Controllers\\{option.MethodName}ProcController.cs", creater.CreateProcController(option, this.viewModel.sps.ToList(), true));

                if (!File.Exists($"{selectedFolderPath}\\Abstracts\\I{option.MethodName}Repository.cs"))
                {
                    File.Create($"{selectedFolderPath}\\Abstracts\\I{option.MethodName}Repository.cs").Close();
                }
                File.WriteAllText($"{selectedFolderPath}\\Abstracts\\I{option.MethodName}Repository.cs", creater.CreateAbstract(option, this.viewModel.sps.ToList(), true));

                if (!File.Exists($"{selectedFolderPath}\\Repositories\\{option.MethodName}Repository.cs"))
                {
                    File.Create($"{selectedFolderPath}\\Repositories\\{option.MethodName}Repository.cs").Close();
                }
                File.WriteAllText($"{selectedFolderPath}\\Repositories\\{option.MethodName}Repository.cs", creater.CreateDefaultRepository(option, this.viewModel.sps.ToList(), true));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void Btn_AlterProject_Click2(object paramData)
        {
            try
            {
                CSharpCreater creater = new CSharpCreater();
                string selectedFolderPath = Convert.ToString(paramData);
                foreach (var entity in this.viewModel.entities)
                {
                    var list = option.GetTableProperties(entity.name);


                    if (!File.Exists($"{selectedFolderPath}\\Entities\\{entity.name}.cs"))
                    {
                        File.Create($"{selectedFolderPath}\\Entities\\{entity.name}.cs").Close();
                    }
                    File.WriteAllText($"{selectedFolderPath}\\Entities\\{entity.name}.cs", creater.CreateEntity(option, entity, list, true));

                    if (entity.ObjectType.Equals("TABLE", StringComparison.OrdinalIgnoreCase))
                    {
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void Btn_AlterProject_Click3(object paramData)
        {
            try
            {
                CSharpCreater creater = new CSharpCreater();
                string selectedFolderPath = Convert.ToString(paramData);
                foreach (var sp in this.viewModel.sps)
                {
                    var inputs = option.GetSpProperties(sp.name);
                    var outputs = option.GetSpOutputs(sp.name);

                    if (!File.Exists($"{selectedFolderPath}\\Models\\Parameters\\Input{GetNameFromSP(sp.name)}Parameter.cs"))
                    {
                        File.Create($"{selectedFolderPath}\\Models\\Parameters\\Input{GetNameFromSP(sp.name)}Parameter.cs").Close();
                    }
                    File.WriteAllText($"{selectedFolderPath}\\Models\\Parameters\\Input{GetNameFromSP(sp.name)}Parameter.cs", creater.CreateParameter(option, sp, inputs, true));

                    if (outputs != null && !(outputs.Where(x => x.name.Equals("IsError", StringComparison.OrdinalIgnoreCase)).Count() > 0))
                    {
                        var objTarget = option.Find(viewModel.entities.ToList(), outputs);
                        if (objTarget == null)
                        {
                            if (!File.Exists($"{selectedFolderPath}\\Models\\Parameters\\Output{GetNameFromSP(sp.name)}Parameter.cs"))
                            {
                                File.Create($"{selectedFolderPath}\\Models\\Parameters\\Output{GetNameFromSP(sp.name)}Parameter.cs").Close();
                            }
                            File.WriteAllText($"{selectedFolderPath}\\Models\\Parameters\\Output{GetNameFromSP(sp.name)}Parameter.cs", creater.CreateParameter(option, sp, outputs, true));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
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

        public void AllEnables(bool enable, DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is FrameworkElement frameworkElement && frameworkElement.Name != "BottomBar")
                {
                    frameworkElement.IsEnabled = enable;
                }

                AllEnables(enable, child); // 재귀 호출
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
                    if (string.IsNullOrWhiteSpace(result) && arr.Length > 1)
                    {
                        result = arr[arr.Length - 2].Trim();
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                result = option.Database.GetAppName();
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

        private void CheckUsei18n_Checked(object sender, RoutedEventArgs e)
        {
            this.option.Usei18n = true;
        }

        private void CheckUsei18n_Unchecked(object sender, RoutedEventArgs e)
        {
            this.option.Usei18n = false;
        }

        private void CheckUseMultiApi_Checked(object sender, RoutedEventArgs e)
        {
            this.option.UseMultiApi = true;
        }

        private void CheckUseMultiApi_Unchecked(object sender, RoutedEventArgs e)
        {
            this.option.UseMultiApi = false;
        }

        private RichTextBox GetCurrentBox()
        {
            RichTextBox? result = null;
            TabItem? selectedTab = null;

            TabItem languageTab = Languages.SelectedItem as TabItem;
            if (languageTab != null)
            {
                option.Language = languageTab.Header.ToString();
            }

            switch ((this.option?.Language ?? "").Trim().ToUpper())
            {
                case "ASP.NET":
                    selectedTab = AspNetOptions.SelectedItem as TabItem;
                    if (selectedTab != null)
                    {
                        option.Category = selectedTab.Header.ToString();
                    }

                    switch ((this.option?.Category ?? "").Trim().ToUpper())
                    {
                        case "ENTITY":
                            result = AspNetEntity;
                            break;
                        case "CONTROLLER":
                            result = AspNetController;
                            break;
                        case "ABSTRACT":
                            result = AspNetAbstract;
                            break;
                        case "REPOSITORY":
                            result = AspNetRepository;
                            break;
                    }
                    break;
                case "JAVA":
                    selectedTab = JavaOptions.SelectedItem as TabItem;
                    if (selectedTab != null)
                    {
                        option.Category = selectedTab.Header.ToString();
                    }

                    switch ((this.option?.Category ?? "").Trim().ToUpper())
                    {
                        case "ENTITY":
                            result = JavaEntity;
                            break;
                        case "CONTROLLER":
                            result = JavaController;
                            break;
                        case "ABSTRACT":
                            result = JavaAbstract;
                            break;
                        case "REPOSITORY":
                            result = JavaRepository;
                            break;
                    }
                    break;
                case "DATABASE":
                    selectedTab = SQL.SelectedItem as TabItem;
                    if (selectedTab != null)
                    {
                        option.Category = selectedTab.Header.ToString();
                    }

                    switch ((this.option?.Category ?? "").Trim().ToUpper())
                    {
                        case "MSSQL":
                            result = MsSqlQuery;
                            break;
                        case "MYSQL":
                            result = MySqlQuery;
                            break;
                        case "MONGODB":
                            result = MongoDbQuery;
                            break;
                    }
                    break;
                case "TYPESCRIPT":
                    selectedTab = TypeOptions.SelectedItem as TabItem;
                    if (selectedTab != null)
                    {
                        option.Category = selectedTab.Header.ToString();
                    }

                    switch ((this.option?.Category ?? "").Trim().ToUpper())
                    {
                        case "ENTITY":
                            result = TsEntity;
                            break;
                        case "CONTROLLER":
                            result = TsController;
                            break;
                        case "ABSTRACT":
                            result = TsController;
                            break;
                        case "REPOSITORY":
                            result = TsRepository;
                            break;
                    }
                    break;
                case "NODE.JS":
                    selectedTab = NodeOptions.SelectedItem as TabItem;
                    if (selectedTab != null)
                    {
                        option.Category = selectedTab.Header.ToString();
                    }

                    switch ((this.option?.Category ?? "").Trim().ToUpper())
                    {
                        case "ENTITY":
                            result = JsEntity;
                            break;
                        case "CONTROLLER":
                            result = JsController;
                            break;
                        case "ABSTRACT":
                            result = JsAbstract;
                            break;
                        case "REPOSITORY":
                            result = JsRepository;
                            break;
                        case "YAML":
                            result = YAMLBox;
                            break;
                    }
                    break;
                case "VUE.JS":
                    selectedTab = VueOptions.SelectedItem as TabItem;
                    if (selectedTab != null)
                    {
                        option.Category = selectedTab.Header.ToString();
                    }

                    switch ((this.option?.Category ?? "").Trim().ToUpper())
                    {
                        case "FORM":
                            result = VueForm;
                            break;
                        case "COMPONENT":
                            result = VueComponent;
                            break;
                    }
                    break;
                case "REACT.JS":
                    selectedTab = ReactOptions.SelectedItem as TabItem;
                    if (selectedTab != null)
                    {
                        option.Category = selectedTab.Header.ToString();
                    }

                    switch ((this.option?.Category ?? "").Trim().ToUpper())
                    {
                        case "FORM":
                            result = ReactForm;
                            break;
                        case "COMPONENT":
                            result = ReactComponent;
                            break;
                    }
                    break;
                case "HTML":
                    selectedTab = HtmlOptions.SelectedItem as TabItem;
                    if (selectedTab != null)
                    {
                        option.Category = selectedTab.Header.ToString();
                    }

                    switch ((this.option?.Category ?? "").Trim().ToUpper())
                    {
                        case "TAILWINDCSSFORM":
                            result = TailwindCssFormBox;
                            break;
                    }
                    break;
            }

            return result;
        }

        private void Btn_DbClear_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("대상 데이터베이스내에 생성된 모든 테이블과 SP, 뷰, 함수가 삭제됩니다.  진행하시겠습니까?", "Database Clear!!", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                using (var rep = new SqlServerRepository(this.context))
                {
                    rep.ClearDatabase();
                    MessageBox.Show("삭제되었습니다.  하지만, FK 순서에 따라 일부가 삭제되지 않았을 수 있으니, DB Clear 버튼을 연속해서 눌러주세요.");
                }
            }
        }

        private void Btn_FileSave_Click(object sender, RoutedEventArgs e)
        {
            if (option.target != null && !string.IsNullOrWhiteSpace(option.target.name))
            {
                string selectedFolderPath = ShowFolderDialog();
                string filename = option.target.name;

                if (!string.IsNullOrEmpty(selectedFolderPath))
                {
                    if (Directory.Exists(selectedFolderPath))
                    {
                        Directory.CreateDirectory($"{selectedFolderPath}");
                    }

                    TabItem languageTab = Languages.SelectedItem as TabItem;
                    if (languageTab != null)
                    {
                        option.Language = languageTab.Header.ToString();
                    }

                    string body = string.Empty;
                    string extend = "txt";

                    RichTextBox targetBox = GetCurrentBox();
                    if (targetBox != null)
                    {
                        body = new TextRange(targetBox.Document.ContentStart, targetBox.Document.ContentEnd).Text;
                    }

                    switch ((this.option?.Language ?? "").Trim().ToUpper())
                    {
                        case "ASP.NET":
                            extend = "cs";
                            break;
                        case "JAVA":
                            extend = "java";
                            break;
                        case "DATABASE":
                            extend = "sql";
                            break;
                        case "TYPESCRIPT":
                            extend = "ts";
                            break;
                        case "NODE.JS":
                            extend = "js";
                            break;
                        case "VUE.JS":
                            extend = "vue";
                            break;
                        case "REACT.JS":
                            extend = "tsx";
                            break;
                        case "HTML":
                            extend = "html";
                            break;
                    }

                    try
                    {
                        if (!File.Exists($"{selectedFolderPath}\\{filename}.{extend}"))
                        {
                            File.Create($"{selectedFolderPath}").Close();
                        }
                        File.WriteAllText($"{selectedFolderPath}\\{filename}.{extend}", body);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{ex.Message}.  {selectedFolderPath}");
                    }
                }
                else
                {
                    MessageBox.Show($"올바른 위치를 지정해 주세요[1].");
                }
            }
            else
            {
                MessageBox.Show($"대상을 지정해 주세요.");
            }
        }

        private void Btn_CommonFile_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsConnection)
            {
                string selectedFolderPath = ShowFolderDialog();

                if (!string.IsNullOrEmpty(selectedFolderPath))
                {
                    if (!Directory.Exists(selectedFolderPath))
                    {
                        MessageBox.Show($"대상 위치를 찾을 수 없습니다.");
                    }
                    else
                    {
                        Loading(true);

                        worker = new Thread(new ParameterizedThreadStart((path) => {
                            Btn_ComminFile_Proc(path);

                            Dispatcher.Invoke(() =>
                            {
                                worker = null;
                                Loading(false);
                            });
                        }));

                        worker.Start(selectedFolderPath);
                    }
                }
            }
            else
            {
                MessageBox.Show($"Database 연결을 먼저 진행해 주세요.");
            }
        }

        private void Btn_ComminFile_Proc(object paramData)
        {
            try
            {
                string selectedFolderPath = Convert.ToString(paramData);
                MsSqlCreater creater = new MsSqlCreater();

                if (!Directory.Exists($"{selectedFolderPath}\\Functions"))
                {
                    Directory.CreateDirectory($"{selectedFolderPath}\\Functions");
                }

                if (!Directory.Exists($"{selectedFolderPath}\\Views"))
                {
                    Directory.CreateDirectory($"{selectedFolderPath}\\Views");
                }

                foreach (string common in creater.Functions)
                {
                    if (!File.Exists($"{selectedFolderPath}\\Functions\\{common}.sql"))
                    {
                        File.Create($"{selectedFolderPath}\\Functions\\{common}.sql").Close();
                    }
                    File.WriteAllText($"{selectedFolderPath}\\Functions\\{common}.sql", creater.CreateCommon(common));
                }

                foreach (string common in creater.Views)
                {
                    if (!File.Exists($"{selectedFolderPath}\\Views\\{common}.sql"))
                    {
                        File.Create($"{selectedFolderPath}\\Views\\{common}.sql").Close();
                    }
                    File.WriteAllText($"{selectedFolderPath}\\Views\\{common}.sql", creater.CreateCommon(common));
                }


                MessageBox.Show($"공통요소가 작성되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void Btn_NodeCreate_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsConnection)
            {
                string selectedFolderPath = ShowFolderDialog();
                option.ProjectName = GetProjectName(selectedFolderPath);
                option.MethodName = GetMethodName(selectedFolderPath);

                if (!string.IsNullOrEmpty(selectedFolderPath))
                {
                    if (!Directory.Exists(selectedFolderPath))
                    {
                        MessageBox.Show($"대상 위치를 찾을 수 없습니다.");
                    }
                    else
                    {
                        Loading(true);

                        worker = new Thread(new ParameterizedThreadStart((path) => {
                            Btn_NodeCreate_Proc(path);

                            Dispatcher.Invoke(() =>
                            {
                                worker = null;
                                Loading(false);
                            });
                        }));

                        worker.Start(selectedFolderPath);
                    }
                }
            }
            else
            {
                MessageBox.Show($"Database 연결을 먼저 진행해 주세요.");
            }
        }

        private void Btn_NodeCreate_Proc(object paramData)
        {
            try
            {
                string selectedFolderPath = Convert.ToString(paramData);
                JavaScriptCreater creater = new JavaScriptCreater();

                if (!Directory.Exists($"{selectedFolderPath}\\models"))
                {
                    Directory.CreateDirectory($"{selectedFolderPath}\\models");
                }

                if (!Directory.Exists($"{selectedFolderPath}\\routes"))
                {
                    Directory.CreateDirectory($"{selectedFolderPath}\\routes");
                }

                if (!Directory.Exists($"{selectedFolderPath}\\swagger"))
                {
                    Directory.CreateDirectory($"{selectedFolderPath}\\swagger");
                }

                if (!File.Exists($"{selectedFolderPath}\\package.json"))
                {
                    File.Create($"{selectedFolderPath}\\package.json").Close();
                }
                File.WriteAllText($"{selectedFolderPath}\\package.json", creater.NodePackgeJsonCreate(this.option));

                if (!File.Exists($"{selectedFolderPath}\\index.js"))
                {
                    File.Create($"{selectedFolderPath}\\index.js").Close();
                }
                File.WriteAllText($"{selectedFolderPath}\\index.js", creater.NodeIndexJsCreate(this.option));

                if (!File.Exists($"{selectedFolderPath}\\config.js"))
                {
                    File.Create($"{selectedFolderPath}\\config.js").Close();
                }
                File.WriteAllText($"{selectedFolderPath}\\config.js", creater.NodeConfigCreate(this.option));


                MessageBox.Show($"모든 파일이 생성되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}