using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
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
    /// Interaction logic for TaskWindow.xaml
    /// </summary>
    public partial class TaskWindow : Window
    {
        private WallEntities wallEntities = new WallEntities();
        public TaskWindow()
        {
            InitializeComponent();
            cbPos.ItemsSource = (wallEntities.Positions.Where(p=>p.Id != 1)).ToList().Select(p=>p.Title);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var position = wallEntities.Positions.FirstOrDefault(p => p.Title == cbPos.Text);

                Tasks task = new Tasks
                {
                    Header = tbHeader.Text,
                    Info = tbDescr.Text,
                    Position = position.Id,
                    DeadLine = DateTime.Parse(dpDead.Text)
                };

                if(!wallEntities.Tasks.Any(t=>t.Header == task.Header && t.Info == task.Info))
                {
                    wallEntities.Tasks.Add(task);
                    wallEntities.SaveChanges();

                    MessageBox.Show($"Successfully added new Task: {(wallEntities.Tasks.First(t=>t.Header == tbHeader.Text)).Header}","New Task");
                }
                else
                {
                    MessageBox.Show("This task already been added!","New Task error");
                }

            }
            catch (DbEntityValidationException ex)
            {
                foreach (var entityValidationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                    {
                        // Log or handle the validation error, for example:
                        MessageBox.Show("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,ex.Source);
            }
            finally
            {
                this.Close();
            }
        }
    }
}
