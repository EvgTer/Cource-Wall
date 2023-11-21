using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for CompleatingWindow.xaml
    /// </summary>
    public partial class CompleatingWindow : Window
    {
        private WallEntities wallEntities = new WallEntities();
        private Tasks currentTask;
        private string fileHeader;
        private byte[] file;
        public CompleatingWindow(string header)
        {
            InitializeComponent();
            currentTask = wallEntities.Tasks.FirstOrDefault(t=>t.Header == header);
            prUpload.Visibility = Visibility.Hidden;
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var taskToUpdate = wallEntities.Tasks.FirstOrDefault(t => t.Id == currentTask.Id);

                if (taskToUpdate != null)
                {
                    taskToUpdate.Comment = tbComm.Text;
                    taskToUpdate.File = file;
                    taskToUpdate.FileName = fileHeader;


                    wallEntities.SaveChanges();
                }
                else
                {
                    MessageBox.Show("Task not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                bool? success = true;
                DialogResult = success;
                Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult= null;
            Close();
        }

        private async void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                prUpload.Visibility = Visibility.Visible;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = txtFile.Text = fileHeader = openFileDialog.FileName;

                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        var cancellationTokenSource = new CancellationTokenSource();

                        try
                        {
                            file = new byte[fileStream.Length];

                            long fileSize = fileStream.Length;

                            byte[] buffer = new byte[8156];
                            int bytesRead;
                            long totalBytesRead = 0;

                            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationTokenSource.Token)) > 0)
                            {
                                if (cancellationTokenSource.Token.IsCancellationRequested)
                                {
                                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                                }

                                Buffer.BlockCopy(buffer, 0, file, (int)totalBytesRead, bytesRead);

                                totalBytesRead += bytesRead;
                                int percentComplete = (int)((double)totalBytesRead / fileSize * 100);

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    prUpload.Value = percentComplete;
                                });
                                await Task.Delay(100);
                            }
                            MessageBox.Show("Successfully loaded!");
                        }
                        catch (OperationCanceledException)
                        {
                            MessageBox.Show("File upload was canceled.");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"File upload failed: {ex.Message}");
                        }
                        finally
                        {
                            prUpload.Value = 0;
                            prUpload.Visibility = Visibility.Hidden;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}
