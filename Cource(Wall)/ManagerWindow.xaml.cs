using System;
using System.Collections.ObjectModel;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Cource_Wall_
{
    /// <summary>
    /// Interaction logic for ManagerWindow.xaml
    /// </summary>
    public partial class ManagerWindow : Window
    {
        private WallEntities wallEntities;
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
                wallEntities = new WallEntities();
                taskList = new ObservableCollection<TaskData>(wallEntities.Tasks
                    .Select(task => new TaskData
                    {
                        Header = task.Header,
                        Position = wallEntities.Positions.FirstOrDefault(p => p.Id == task.Position).Title
                    }));
                taskDataGrid.ItemsSource = taskList;

                foreach (TaskData taskData in taskDataGrid.Items)
                {
                    Type itemType = taskData.GetType();
                    var positionProperty = itemType.GetProperty("Position");
                    var headerProperty = itemType.GetProperty("Header");

                    if (headerProperty != null && positionProperty != null)
                    {
                        string header = headerProperty.GetValue(taskData) as string;

                        Tasks task = wallEntities.Tasks.FirstOrDefault(p => p.Header == header);

                        taskDataGrid.ItemContainerGenerator.StatusChanged += (s, e) =>
                        {
                            if (taskDataGrid.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                            {
                                DataGridRow row = taskDataGrid.ItemContainerGenerator.ContainerFromItem(taskData) as DataGridRow;

                                if (row != null)
                                {
                                    if (wallEntities.Tasks.Any(t => t.Header == task.Header && (t.Comment != null || t.FileName != "Completing" && t.FileName != null || t.File != null)))
                                    {
                                        row.Style = FindResource("GreenRowStyle") as Style;
                                    }
                                    else if (wallEntities.Tasks.Any(t => t.Header == task.Header && t.FileName == "Completing"))
                                    {
                                        row.Style = FindResource("OrangeRowStyle") as Style;
                                    }
                                    else
                                    {
                                        row.Style = FindResource("BlackRowStyle") as Style;
                                    }
                                }
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Task table error");
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

        private void taskDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (taskDataGrid.SelectedItem != null && btnDelete.Visibility == Visibility.Hidden)
            {
                btnDelete.Visibility = Visibility.Visible;
                return;
            }
            else if (btnDelete.Visibility == Visibility.Visible)
            {
                btnDelete.Visibility = Visibility.Hidden;
            }
        }

        private void taskDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TaskData data = taskDataGrid.SelectedItem as TaskData;
                Tasks task = wallEntities.Tasks.FirstOrDefault(t => t.Header == data.Header);

                if (task != null)
                {
                    if(task.File != null && task.FileName!= null)
                    {
                        int lenght = task.FileName.ToString().IndexOf('^');
                        string login = string.Empty;
                        if(lenght > 0)
                        {
                            login = task.FileName.Substring(0,lenght);
                        }

                        if (login != null && login != string.Empty)
                        {
                            MessageBox.Show($"{login} did this task!\n" +
                                $"Comment: {task.Comment}");
                        }
                        else if (login == string.Empty)
                        {
                            MessageBox.Show("Login wasn't found!\n" +
                                $"Comment: {task.Comment}");
                        }
                        else
                        {
                            MessageBox.Show("User wasn't found");
                        }
                    }
                
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TaskData data = taskDataGrid.SelectedItem as TaskData;
                Tasks task = wallEntities.Tasks.FirstOrDefault(t => t.Header == data.Header);

                if (task != null)
                {
                    wallEntities.Tasks.Remove(task);

                    wallEntities.SaveChanges();

                    MessageBox.Show($"Deleted task: {task.Header}", "Deleting");

                    taskDataGrid.SelectedItem = null;

                    taskList.Clear();
                    InitializeData();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
