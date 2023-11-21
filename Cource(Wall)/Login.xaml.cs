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

namespace Cource_Wall_
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tbLogin.Text.ToString() == string.Empty || tbPassword.Password.ToString() == string.Empty)
                {
                    MessageBox.Show("Please enter login and/or password", "Inncorrect input");
                }

                WallEntities wallEntities = new WallEntities();
                var employee = wallEntities.Employees.FirstOrDefault(empl => empl.Login.ToString() == tbLogin.Text.ToString() && empl.Password.ToString() == tbPassword.Password.ToString());

                if (employee != null)
                {
                    switch (employee.Position)
                    {
                        case 1:
                            this.Visibility = Visibility.Hidden;
                            new ManagerWindow(employee).ShowDialog();
                            tbLogin.Clear();
                            tbPassword.Clear();
                            this.Visibility = Visibility.Visible;
                            break;
                        case 2:
                        case 3:
                            this.Visibility = Visibility.Hidden;
                            new MainWindow(employee).ShowDialog();
                            tbLogin.Clear();
                            tbPassword.Clear();
                            this.Visibility = Visibility.Visible;
                            break;
                    }
                }
                else if (wallEntities.Employees.Any(empl => empl.Login.ToString() == tbLogin.Text.ToString() && empl.Password.ToString() != tbPassword.Password.ToString()))
                {
                    MessageBox.Show("Password is wrong!", "Incorrect input", MessageBoxButton.OK, MessageBoxImage.Error);
                    tbPassword.Clear();
                }
                else if (!wallEntities.Employees.Any(empl => empl.Login.ToString() == tbLogin.Text.ToString() && empl.Password.ToString() == tbPassword.Password.ToString()))
                {
                    MessageBox.Show("Wrong data,\nInput another login and password", "Incorrect input", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    tbLogin.Clear();
                    tbPassword.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }
    }
}
