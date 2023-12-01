using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Woose.Core;

namespace Woose.Builder.Popup
{
    /// <summary>
    /// ConnectionForm.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ConnectionForm : Window
    {
        private Database database;

        private MainWindow parent;

        public Database Context
        {
            get
            {
                return this.database;
            }
            set
            {
                this.database = value;
            }
        }

        public ConnectionForm(MainWindow parent)
        {
            InitializeComponent();
            this.parent = parent;
            this.database = new Database();
        }

        public ConnectionForm(MainWindow parent, Database target)
        {
            InitializeComponent();
            this.parent = parent;
            this.database = target;
            ConnectionTitle.Text = this.database.DatabaseName;
            ComboBoxItem selectedItem = DatabaseType.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Content?.ToString() == this.database.DatabaseType);

            if (selectedItem != null)
            {
                DatabaseType.SelectedItem = selectedItem;
            }
            ConnectionString.Text = this.database.ConnectionString;
        }

        private void Btn_Conn_Save_Click(object sender, RoutedEventArgs e)
        {
            this.database.DatabaseName = ConnectionTitle.Text;
            if (string.IsNullOrWhiteSpace(this.database.DatabaseName))
            {
                MessageBox.Show("명칭을 입력하세요.", "Alert", MessageBoxButton.OK);
                return;
            }
            if (DatabaseType.SelectedItem != null)
            {
                this.database.DatabaseType = ((ComboBoxItem)DatabaseType.SelectedItem).Content.ToString();
            }
            else
            {
                MessageBox.Show("유형을 선택해 주세요.", "Alert", MessageBoxButton.OK);
                return;
            }

            this.database.ConnectionString = ConnectionString.Text;
            if (string.IsNullOrWhiteSpace(this.database.ConnectionString))
            {
                MessageBox.Show("연결문자열을 입력하세요.", "Alert", MessageBoxButton.OK);
                return;
            }

            ReturnValue rst = new ReturnValue();

            using(var rep = new SqliteRepository())
            {
                if (this.database.Id > 0)
                {
                    rst = rep.UpdateDatabase(this.database);
                }
                else
                {
                    rst = rep.InsertDatabase(this.database);
                }
                
            }

            if (rst.Check)
            {
                if (rst.Value.Equals("Update", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("수정되었습니다.", "Success", MessageBoxButton.OK);
                }
                else
                {
                    MessageBox.Show("추가되었습니다.", "Success", MessageBoxButton.OK);
                }

                this.parent.viewModel.RefreshDatabases();
                this.Close();
            }
            else
            {
                MessageBox.Show(rst.Message, "Alert", MessageBoxButton.OK);
            }
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
