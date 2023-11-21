using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Cource_Wall_
{
    /// <summary>
    /// Interaction logic for ManagerWindow.xaml
    /// </summary>
    public partial class ManagerWindow : Window
    {
        private WallEntities wallEntities = new WallEntities();
        private ObservableCollection<TaskData> taskList;
        public ManagerWindow(Employees employee)
        {
            InitializeComponent();
            InitializeData();
        }
        private void InitializeData()
        {
            try
            {
                taskList = new ObservableCollection<TaskData>(wallEntities.Tasks
            .Select(task => new TaskData
            {
                Header = task.Header,
                Position = (wallEntities.Positions.FirstOrDefault(p => p.Id == task.Position) != null) ? wallEntities.Positions.FirstOrDefault(p => p.Id == task.Position).Title : null
            }));
                taskDataGrid.ItemsSource = taskList;
                foreach (var task in taskDataGrid.Items) 
                {
                    Type itemType = task.GetType();
                    var positionProperty = itemType.GetProperty("Position");
                    var headerProperty = itemType.GetProperty("Header");

                    if (headerProperty != null && positionProperty != null)
                    {
                        string header = headerProperty.GetValue(task) as string;
                        string pos = positionProperty.GetValue(task) as string;

                        if (wallEntities.Tasks.FirstOrDefault(t => t.Header == header).Comment != null || wallEntities.Tasks.FirstOrDefault(t => t.Header == header).FileName != null || wallEntities.Tasks.FirstOrDefault(t => t.Header == header).File != null)
                        {
                            taskDataGrid.ItemContainerGenerator.StatusChanged += (s, e) =>
                            {
                                if (taskDataGrid.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                                {
                                    DataGridRow row = taskDataGrid.ItemContainerGenerator.ContainerFromItem(task) as DataGridRow;
                                    if (row != null)
                                    {
                                        row.Style = FindResource("GreenRowStyle") as Style;
                                    }
                                }
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Task table error");
            }
        }
        private void btnEmployee_Click(object sender, RoutedEventArgs e)
        {
            new EmployeeWindow().ShowDialog();
        }

        private void btnTask_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new TaskWindow().ShowDialog();
                taskList.Clear();
                InitializeData();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var entityValidationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                    {
                        Console.WriteLine("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
        }
        public class TaskData
        {
            public string Header { get; set; }
            public string Position { get; set; }
        }

        private void btbDelete_Click(object sender, RoutedEventArgs e)
        {
            if(taskDataGrid.SelectedItem!= null)
            {
                TaskData data = taskDataGrid.SelectedItem as TaskData;
                Tasks task = wallEntities.Tasks.FirstOrDefault(t=>t.Header == data.Header);

                if(task != null)
                {
                    wallEntities.Tasks.Remove(task);
                }
                wallEntities.SaveChanges();

                btbDelete.Visibility = Visibility.Hidden;

                MessageBox.Show($"Deleted task: {task.Header}","Deleting");

                taskList.Clear();
                InitializeData();
            }
            else
            {
                MessageBox.Show("Please, choose the task");
            }
        }

        private void taskDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(taskDataGrid.SelectedItem!= null && btbDelete.Visibility == Visibility.Hidden)
            {
                btbDelete.Visibility = Visibility.Visible;
            }
            else if(btbDelete.Visibility == Visibility.Visible)
            {
                btbDelete.Visibility = Visibility.Hidden;
            }
        }

        private void taskDataGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            if(taskDataGrid.SelectedItem== null && btbDelete.Visibility == Visibility.Visible)
            {
                btbDelete.Visibility = Visibility.Hidden;
            }
            else if(btbDelete.Visibility == Visibility.Visible)
            {
                btbDelete.Visibility = Visibility.Hidden;
            }
        }
    }
}
