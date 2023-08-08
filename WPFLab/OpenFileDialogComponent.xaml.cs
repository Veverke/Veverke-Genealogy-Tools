using GenealogySoftwareV3.Types;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace WPFLab
{
    /// <summary>
    /// Interaction logic for OpenFileDialogComponent.xaml
    /// </summary>
    public partial class OpenFileDialogComponent : UserControl
    {
        public readonly FileReader fileInterpreter;
        public OpenFileDialogComponent()
        {
            InitializeComponent();
            fileInterpreter = new FileReader();
        }

        private void btnOpenFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "GEDCOM files (*.ged)|*.ged";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            foreach (string filename in openFileDialog.FileNames)
                lboxFiles.Items.Add(Path.GetFileName(filename));

            DataTable fileData = fileInterpreter.LoadDataIntoDataTable(openFileDialog.FileName);
            fileInterpreter.InterpretFileContents(fileData);

            /*
             * TreeViewItem rootTreeViewItem = new TreeViewItem { Header = "Family Tree" };
            List<int> rootMarriages = new List<int>(new int[] { 8, 135, 144 });
            Marriage rootMarriageMatch;
            foreach (int rootMarriage in rootMarriages)
                if (fileInterpreter.Marriages.TryGetValue(rootMarriage, out rootMarriageMatch))
                    fileInterpreter.GenerateTree(rootNode, rootMarriageMatch);

            treeView.Nodes.Add(rootNode);

            fileInterpreter.GetFamiliesRoots();
            */
            
            DataContext = fileInterpreter;
        }
    }
}
