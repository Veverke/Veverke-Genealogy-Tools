using GenealogySoftwareV3.Types;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using System.Xml.Schema;

namespace WPFGedcomParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly GEDCOMParser _gedcomParser;
        public MainWindow()
        {
            InitializeComponent();
            _gedcomParser = new GEDCOMParser();
            //this.DataContext = _dataContext;
        }

        private void LoadTree()
        {
            treeView.Items.Clear();
            TreeViewItem rootTreeViewItem = new TreeViewItem { Header = "Family Tree" };
            List<int> rootMarriages = new List<int>(new int[] { 8, 135, 144 });
            Marriage rootMarriageMatch;

            foreach (int rootMarriage in rootMarriages)
                if (_gedcomParser.Marriages.TryGetValue(rootMarriage, out rootMarriageMatch))
                    _gedcomParser.GenerateTree(rootTreeViewItem, rootMarriageMatch);

            treeView.Items.Add(rootTreeViewItem);
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
            
            Title = System.IO.Path.GetFileName(openFileDialog.FileName);

            DataTable fileData = _gedcomParser.ReadIntoDataTable(openFileDialog.FileName);
            _gedcomParser.ParseData(fileData);

            LoadTree();
        }


        private void btnShowGeographicDistribution_Click(object sender, RoutedEventArgs e)
        {
            listView.Items.Clear();
            var distinctCountries = _gedcomParser.Individuals.Select(i => i.Value.Birth.Place.Country).Distinct().OrderBy(c => c);
            for (var i = 0; i < distinctCountries.Count(); i++)
            {
                var country = distinctCountries.ElementAt(i);
                listView.Items.Add(GetListViewItem(i, $"({i + 1}) {country}"));
            }
        }

        private void btnShowSurmames_Click(object sender, RoutedEventArgs e)
        {
            listView.Items.Clear();
            var distinctSurnames = _gedcomParser.Individuals.SelectMany(i => i.Value.Surnames).Distinct().OrderBy(s => s);
            for(var i = 0; i < distinctSurnames.Count(); i++)
            {
                var surname = distinctSurnames.ElementAt(i);
                listView.Items.Add(GetListViewItem(i, $"({i + 1}) {surname}"));
            }

            listView.Items.Add(new ListViewItem { Content = "----------------------- Total individuals per surname ----------------------------------" });
            var surnameGroups = _gedcomParser.Individuals.GroupBy(i => string.Join(",", i.Value.Surnames)).OrderByDescending(g => g.Count());
            for(var i = 0; i < surnameGroups.Count(); i++)
            {
                var surnameGroup = surnameGroups.ElementAt(i);
                listView.Items.Add(GetListViewItem(i, $"{surnameGroup.Key} [{surnameGroup.Count()}]"));
            }
        }

        private void btnShowCausesOfDeathDistribution_Click(object sender, RoutedEventArgs e)
        {
            listView.Items.Clear();
            var causesOfDeathGroups = _gedcomParser.Individuals.GroupBy(i => i.Value.Death.CauseofDeath).OrderByDescending(g => g.Count());
            for (var i = 0; i < causesOfDeathGroups.Count(); i++)
            {
                var causeOfDeathGroup = causesOfDeathGroups.ElementAt(i);
                listView.Items.Add(GetListViewItem(i, $"{causeOfDeathGroup.Key} [{causeOfDeathGroup.Count()}]"));
            }
        }

        private Brush GetListItemBackgroundColor(int itemIndex)
        {
            return itemIndex % 2 == 0 ? Brushes.White : Brushes.LightGray;
        }

        private ListViewItem GetListViewItem(int itemIndex, string content)
        {
            return new ListViewItem { Background = GetListItemBackgroundColor(itemIndex), BorderBrush = Brushes.Black, Content = content};
        }

        private void btnExportToJson_Click(object sender, RoutedEventArgs e)
        {
            var json =
                JsonConvert.SerializeObject(
                new
                {
                    Individuals = _gedcomParser.Individuals.Values,
                    Marriages = _gedcomParser.Marriages.Values
                }, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            File.WriteAllText($"gedcom-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.json", json);
        }
    }
}
