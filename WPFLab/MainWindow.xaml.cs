using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Schema;
using WPFGedcomParser.Types;

namespace WPFGedcomParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GEDCOMParser _gedcomParser;
        private string _gedcomFilePath;
        private string _lastMenuAction;
        private Dictionary<int, Marriage> _selectedTreeItemRelevantMarriages;
        private Dictionary<int, Individual> _selectedTreeItemRelevantIndividuals;
        public MainWindow()
        {
            InitializeComponent();
            _gedcomParser = new GEDCOMParser();
            //this.DataContext = _dataContext;
        }

        private void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            //progressBar.Value = e.ProgressPercentage;
        }

        private async void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            //for (var i = 0; i < 10; i++)
            //{
            //    //await Task.Delay(TimeSpan.FromSeconds(0.01));
            //    (sender as BackgroundWorker).ReportProgress(i);
            //}
        }

        private void collectAncestorsMarriages(int individualId, Dictionary<string, int> ancestorsMarriages)
        {
            if (individualId == 0)
            {
                return;
            }

            if (ancestorsMarriages == null)
            {
                ancestorsMarriages = new Dictionary<string, int>();
            }

            var ancestorsMarriage = _gedcomParser.Marriages.FirstOrDefault(m => m.Value.Children.Contains(individualId));

            if (ancestorsMarriage.Key == 0)
            {
                //oldest ancestor for current family
                var familyName = _gedcomParser.Individuals[individualId].Surnames.FirstOrDefault();
                if (!string.IsNullOrEmpty(familyName))
                {
                    ancestorsMarriages.Add(familyName, _gedcomParser.Individuals[individualId].MarriageIDs.FirstOrDefault());
                }
                return;
            }
            var key = _gedcomParser.Individuals[ancestorsMarriage.Value.Husband].Surnames.FirstOrDefault();

            collectAncestorsMarriages(ancestorsMarriage.Value.Husband, ancestorsMarriages);
            collectAncestorsMarriages(ancestorsMarriage.Value.Wife, ancestorsMarriages);

        }

        private void LoadTree()
        {
            treeView.Items.Clear();
            TreeViewItem rootTreeViewItem = new TreeViewItem { Header = "Family Tree" };
            //List<int> rootMarriages = new List<int>(new int[] { 8, 135, 144 });
            var dicMarriages = new Dictionary<string, int>();
            collectAncestorsMarriages(1, dicMarriages);

            var rootMarriages = dicMarriages.Select(m => m.Value);
            foreach (int rootMarriage in rootMarriages)
                if (_gedcomParser.Marriages.TryGetValue(rootMarriage, out Marriage rootMarriageMatch))
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
            _gedcomFilePath = openFileDialog.FileName;

            Title = System.IO.Path.GetFileName(_gedcomFilePath);
            DataTable fileData = _gedcomParser.ReadIntoDataTable(_gedcomFilePath);
            _gedcomParser.ParseData(fileData);

            LoadTree();
            lblIndividuals.Content = $"Individuals: [{_gedcomParser.Individuals.Count}]";
        }


        private void btnShowGeographicDistribution_Click(object sender, RoutedEventArgs e)
        {
            listView.Items.Clear();
            //var countriesGroups = _gedcomParser.Individuals.GroupBy(i => i.Value.Birth.Place.County).Where(c => !string.IsNullOrEmpty(c.Key));
            //countriesGroups.GroupJoin(_gedcomParser.Individuals.GroupBy(i => i.Value.Birth.Place.County).Where(c => !string.IsNullOrEmpty(c.Key)));
            //for (var i = 0; i < countriesGroups.Count(); i++)
            //{
            //    var countryGroup = countriesGroups.ElementAt(i);
            //    var citiesGroups = countryGroup.GroupBy(c => c.Value.Birth.Place.City);
            //    for(var j = 0; j < citiesGroups.Count(); j++)
            //    {
            //        var cityGroup = citiesGroups.ElementAt(j);
            //        listView.Items.Add(GetListViewItem(i + j, $"{countryGroup.Key}, {cityGroup.Key}"));
            //    }    
            //}

            var distinctCountries = _gedcomParser.Individuals.Select(i => i.Value.Birth.Place.Country).Distinct().OrderBy(c => c);
            for (var i = 0; i < distinctCountries.Count(); i++)
            {
                var country = distinctCountries.ElementAt(i);
                listView.Items.Add(GetListViewItem(i, $"({i + 1}) {country}"));
            }

            tabControl.SelectedIndex = 1;
            _lastMenuAction = nameof(btnShowGeographicDistribution_Click);
        }

        private void btn_ShowIndividualsPerSurname_Click(object sender, RoutedEventArgs e)
        {
            listView.Items.Clear();
            var surnameGroups = _gedcomParser.Individuals.GroupBy(i => string.Join(",", i.Value.Surnames)).OrderByDescending(g => g.Count());
            for (var i = 0; i < surnameGroups.Count(); i++)
            {
                var surnameGroup = surnameGroups.ElementAt(i);
                listView.Items.Add(GetListViewItem(i, $"{surnameGroup.Key} [{surnameGroup.Count()}]"));
            }

            tabControl.SelectedIndex = 1;
            _lastMenuAction = nameof(btn_ShowIndividualsPerSurname_Click);
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

            tabControl.SelectedIndex = 1;
            _lastMenuAction = nameof(btnShowCausesOfDeathDistribution_Click);
        }

        private Brush GetListItemBackgroundColor(int itemIndex)
        {
            return itemIndex % 2 == 0 ? Brushes.AliceBlue : Brushes.SkyBlue;
        }

        private ListViewItem GetListViewItem(int itemIndex, string content)
        {
            return new ListViewItem { Background = GetListItemBackgroundColor(itemIndex), BorderBrush = Brushes.Black, Content = content };
        }

        private Individual getOldestAncestor(int indivualId)
        {
            var ancestorMarriage = _gedcomParser.Marriages.FirstOrDefault(m => m.Value.Children.Contains(indivualId));
            if (ancestorMarriage.Key == 0)
            {
                return _gedcomParser.Individuals[indivualId];
            }

            if (ancestorMarriage.Value.Husband != 0)
            {
                return getOldestAncestor(ancestorMarriage.Value.Husband);
            }
            else if(ancestorMarriage.Value.Wife != 0)
            {
                return getOldestAncestor(ancestorMarriage.Value.Wife);
            }

            return null;
        }

        private void btnExportToJson_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Visibility = Visibility.Visible;

            //var folderBrowserDialog = new FolderBrowserDialog();
            //openFileDialog.Multiselect = false;
            //openFileDialog.Filter = "GEDCOM files (*.ged)|*.ged";
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //if (openFileDialog.ShowDialog() != true)
            //{
            //    return;
            //}


            /*
                string newText = "abc";
                form.Label.Invoke((MethodInvoker)delegate {
                    // Running on the UI thread
                    form.Label.Text = newText;
                });
            */
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged; ;

            backgroundWorker.RunWorkerAsync();

            var folderPath = Path.GetDirectoryName(_gedcomFilePath);
            var fileName = $"gedcom-{Path.GetFileName(_gedcomFilePath)}-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}";
            var filePath = $"{Path.Combine(folderPath, fileName)}.json";

            var exportTask = Task.Run(async () =>
            {
                var json =
                    JsonConvert.SerializeObject(
                    new
                    {
                        Individuals = _gedcomParser.Individuals.Values,
                        Marriages = _gedcomParser.Marriages.Values
                    }, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                
                File.WriteAllText(filePath, json);

                //await Task.Delay(5000);
            });

            //exportTask.Wait();

            //Process.Start("explorer.exe", $"/e, /select, \"{filePath}\"");
            Process.Start("explorer.exe", folderPath);
            progressBar.Visibility = Visibility.Collapsed;
        }

        private void btnShowSurnamesList(object sender, RoutedEventArgs e)
        {
            listView.Items.Clear();
            var distinctSurnames = _gedcomParser.Individuals.SelectMany(i => i.Value.Surnames).Distinct().OrderBy(s => s);
            for (var i = 0; i < distinctSurnames.Count(); i++)
            {
                var surname = distinctSurnames.ElementAt(i);
                listView.Items.Add(GetListViewItem(i, $"({i + 1}) {surname}"));
            }
            tabControl.SelectedIndex = 1;
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (listView.Items.Count == 0)
            {
                return;
            }

            listView.Items.Clear();
            this.GetType().GetMethod(_lastMenuAction).Invoke(null, new object[] { this, RoutedEventArgs.Empty });

        }
    }
}
