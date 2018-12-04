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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageSynchronizer.Processor;

namespace ImageSynchronizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void HandleProcessCommand(object sender, RoutedEventArgs e)
        {
            var dialog= MessageBox.Show("Did you check metaDataPath, backUpMetaDataPath, input-output dir and" +
                            "shouldprocess, updateMetaOnly and operation type?", "Confirm", MessageBoxButton.YesNo);
            if (dialog == MessageBoxResult.Yes)
            {
                ProcessorManager manager = new ProcessorManager();
                manager.ProcessResources();
                //manager.RenameToUniqueNames();
                MessageBox.Show("Process Complete", "Success", MessageBoxButton.OK);
            }
        }
    }
}
