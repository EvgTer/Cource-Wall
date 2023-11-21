using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
    /// Interaction logic for EmployeeWindow.xaml
    /// </summary>
    public partial class EmployeeWindow : Window
    {
        private WallEntities wallEntities = new WallEntities();
        public EmployeeWindow()
        {
            InitializeComponent();
            cbPos.ItemsSource = (wallEntities.Positions.Where(p => p.Id != 1)).ToList().Select(p => p.Title);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var position = wallEntities.Positions.FirstOrDefault(p => p.Title == cbPos.Text);

                Employees employee = new Employees
                {
                    Name = tbName.Text,
                    Login = tbLogin.Text,
                    Password = tbPass.Text,
                    Position = position.Id,
                    Salary = Decimal.Parse(tbSalary.Text)
                };

                if (!wallEntities.Employees.Any(t => t.Name == employee.Name && t.Login == employee.Login && t.Password == employee.Password))
                {
                    wallEntities.Employees.Add(employee);
                    wallEntities.SaveChanges();

                    MessageBox.Show($"Successfully added new Employee: {(wallEntities.Employees.First(t => t.Name == tbName.Text)).Name}", "New Employee");
                }
                else
                {
                    MessageBox.Show("This employee already been added!", "New Employee error");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
            finally
            {
                this.Close();
            }
        }
    }
}
