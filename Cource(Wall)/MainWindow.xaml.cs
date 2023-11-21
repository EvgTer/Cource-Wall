using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Cource_Wall_
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WallEntities wallEntities = new WallEntities();
        private Employees currentEmployee;
        private ObservableCollection<TaskData> taskList;

        public MainWindow(Employees employee)
        {
            InitializeComponent();
            this.currentEmployee = employee;
            InitializeData();

        }

        private void InitializeData()
        {
            try
            {
                taskList = new ObservableCollection<TaskData>( wallEntities.Tasks
                    .Where(s=>s.Position == currentEmployee.Position)
                    .Select(task => new TaskData
                    {
                        Header = task.Header,
                        Deadline = task.DeadLine,
                    })
                    );
                
                taskDataGrid.ItemsSource = taskList;
                prTasks.Maximum = taskList.Count;
                

                foreach (var task in taskList)
                {
                    if (task.Deadline < DateTime.Now)
                    {
                        taskDataGrid.ItemContainerGenerator.StatusChanged += (s, e) =>
                        {
                            if (taskDataGrid.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                            {
                                DataGridRow row = taskDataGrid.ItemContainerGenerator.ContainerFromItem(task) as DataGridRow;
                                if (row != null)
                                {
                                    row.Style = FindResource("RedRowStyle") as Style;
                                }
                            }
                        };
                    }
                    if (wallEntities.Tasks.FirstOrDefault(t=>t.Header == task.Header && t.DeadLine == task.Deadline).Comment!= null || wallEntities.Tasks.FirstOrDefault(t=>t.Header == task.Header && t.DeadLine == task.Deadline).FileName!= null ||wallEntities.Tasks.FirstOrDefault(t=>t.Header == task.Header && t.DeadLine == task.Deadline).File!= null)
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
                        prTasks.Value += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Task table error");
            }
        }
        private List<ConfettiPiece> confettiPieces = new List<ConfettiPiece>();
        private void GenerateConfetti(int count)
        {
            Random random = new Random();

            for (int i = 0; i < count; i++)
            {
                double x = random.Next((int)confettiCanvas.ActualWidth);
                double y = -20;
                double speed = random.Next(5, 10);
                double angle = random.Next(360);
                Brush fill = new SolidColorBrush(Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)));

                Ellipse confettiPiece = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = fill
                };

                Canvas.SetLeft(confettiPiece, x);
                Canvas.SetTop(confettiPiece, y);

                confettiCanvas.Children.Add(confettiPiece);

                DoubleAnimation animation = new DoubleAnimation
                {
                    To = this.ActualHeight,
                    Duration = TimeSpan.FromSeconds(speed)
                };

                Storyboard.SetTarget(animation, confettiPiece);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(Canvas.Top)"));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                storyboard.Completed += (s, e) =>
                {
                    confettiCanvas.Children.Remove(confettiPiece);
                };

                storyboard.Begin();
            }
        }


        private void UpdateConfetti()
        {
            for (int i = confettiPieces.Count - 1; i >= 0; i--)
            {
                ConfettiPiece piece = confettiPieces[i];
                piece.X += piece.Speed * Math.Cos(piece.Angle * Math.PI / 180);
                piece.Y += piece.Speed * Math.Sin(piece.Angle * Math.PI / 180);

                if (piece.Y > ActualHeight)
                {
                    confettiPieces.RemoveAt(i);
                }
            }
        }
        private DispatcherTimer confettiTimer;

        private void StartConfettiAnimation()
        {
            confettiTimer = new DispatcherTimer();
            confettiTimer.Interval = TimeSpan.FromMilliseconds(16);
            confettiTimer.Tick += (sender, e) =>
            {
                UpdateConfetti();
            };
            confettiTimer.Start();
        }

        private async void prTasks_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if(prTasks.Value == prTasks.Maximum)
            {
                await Task.Delay(1000);

                GenerateConfetti(100);
                StartConfettiAnimation();

                await Task.Delay(2000);

                MessageBox.Show("You have finished all the tasks!", "Congratulations",MessageBoxButton.OK,MessageBoxImage.Exclamation);
            }
            
        }

        private void taskDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (taskDataGrid.SelectedItem != null)
                {
                    
                    Type itemType = taskDataGrid.SelectedItem.GetType();
                    var headerProperty = itemType.GetProperty("Header");
                    var deadlineProperty = itemType.GetProperty("Deadline");

                    if (headerProperty != null && deadlineProperty != null)
                    {
                        string header = headerProperty.GetValue(taskDataGrid.SelectedItem) as string;
                        DateTime deadline = (DateTime)deadlineProperty.GetValue(taskDataGrid.SelectedItem);

                        Tasks selectedTask = wallEntities.Tasks.FirstOrDefault(t => t.Header == header && t.DeadLine == deadline);

                        DataGridRow row = taskDataGrid.ItemContainerGenerator.ContainerFromItem(taskDataGrid.SelectedItem) as DataGridRow;
                        bool isGreenRow = false;
                        if (row != null)
                        {
                            Style rowStyle = row.Style;
                            isGreenRow = (rowStyle == FindResource("GreenRowStyle") as Style);

                        
                            CompleatingWindow completingWindow = null;
                            if (!isGreenRow)
                            {
                                completingWindow = new CompleatingWindow(header);
                                bool? success = completingWindow.ShowDialog();

                                if (completingWindow.DialogResult.HasValue)
                                {
                                   row.Style = FindResource("GreenRowStyle") as Style;
                                   prTasks.Value += 1;
                                }
                            }
                            else
                            {
                                MessageBox.Show("This task has already been completed!");
                            }

                            wallEntities.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw ex;
            }
            finally
            {
                taskDataGrid.SelectedItem = null;

                taskList.Clear();
                InitializeData();
            }
        }
        public class ConfettiPiece
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Speed { get; set; }
            public double Angle { get; set; }
            public Brush Fill { get; set; }
        }
        public class TaskData
        {
            public string Header { get; set; }
            public DateTime? Deadline { get; set; }
        }
    }
}
