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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Schema;
using VeverkeGenealogyTools.Types;
using VeverkeGenealogyTools.Types.GEDCOM;
using VeverkeGenealogyTools.Types.GEDCOM.Entities;
using VeverkeGenealogyTools.Types.SQLiteDAL;

namespace VeverkeGenealogyTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GEDCOMParser _gedcomParser;
        private string _gedcomFilePath;
        private string _yizkorBookDbFilePath;
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

            var ancestorsMarriage = _selectedTreeItemRelevantMarriages.FirstOrDefault(m => m.Value.Children.Contains(individualId));

            if (ancestorsMarriage.Key == 0)
            {
                //oldest ancestor for current family
                var familyName = _selectedTreeItemRelevantIndividuals[individualId].Surnames.FirstOrDefault();
                if (!string.IsNullOrEmpty(familyName))
                {
                    ancestorsMarriages.Add(familyName, _selectedTreeItemRelevantIndividuals[individualId].MarriageIDs.FirstOrDefault());
                }
                return;
            }
            var key = _selectedTreeItemRelevantIndividuals[ancestorsMarriage.Value.Husband].Surnames.FirstOrDefault();

            collectAncestorsMarriages(ancestorsMarriage.Value.Husband, ancestorsMarriages);
            collectAncestorsMarriages(ancestorsMarriage.Value.Wife, ancestorsMarriages);

        }

        private void LoadTree()
        {
            treeView.Items.Clear();
            TreeViewItem rootTreeViewItem = new TreeViewItem { Header = "Family Tree" };
            _selectedTreeItemRelevantIndividuals = _gedcomParser.Individuals;
            _selectedTreeItemRelevantMarriages = _gedcomParser.Marriages;
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

            var distinctCountries = _selectedTreeItemRelevantIndividuals.Select(i => i.Value.Birth.Place.Country).Distinct().OrderBy(c => c);
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
            var surnameGroups = _selectedTreeItemRelevantIndividuals.GroupBy(i => string.Join(",", i.Value.Surnames)).OrderByDescending(g => g.Count());
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
            var causesOfDeathGroups = _selectedTreeItemRelevantIndividuals.GroupBy(i => i.Value.Death.CauseofDeath).OrderByDescending(g => g.Count());
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
            var ancestorMarriage = _selectedTreeItemRelevantMarriages.FirstOrDefault(m => m.Value.Children.Contains(indivualId));
            if (ancestorMarriage.Key == 0)
            {
                return _selectedTreeItemRelevantIndividuals[indivualId];
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
                        Individuals = _selectedTreeItemRelevantIndividuals.Values,
                        Marriages = _selectedTreeItemRelevantMarriages.Values
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
            var distinctSurnames = _selectedTreeItemRelevantIndividuals.SelectMany(i => i.Value.Surnames).Distinct().OrderBy(s => s);
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

        private void mni_Features_GetLeafs(object sender, RoutedEventArgs e)
        {
            string _getSurname(Individual individual)
            {
                var individualSurname = individual.GetSurnames();
                if (!string.IsNullOrEmpty(individualSurname))
                {
                    return individualSurname;
                }

                var fatherSurname = _gedcomParser.GetParents(individual.ID).father?.GetSurnames();
                if (!string.IsNullOrEmpty(fatherSurname))
                {
                    return fatherSurname;
                }

                return string.Empty;
            }

            listView.Items.Clear();
            var leafs = _gedcomParser.GetLeafs().Where(l => !l.LikelyNeverMarried()).OrderBy(leaf => _getSurname(leaf));
            for(var i = 0; i < leafs.Count(); i++)
            {
                var leaf = leafs.ElementAt(i);
                var parents = _gedcomParser.GetParents(leaf.ID);
                var birth = leaf.Birth.Date.Dates?.Count > 0 ? leaf.Birth.Date.Dates.FirstOrDefault().ToString("yyyy-MM-dd") : string.Empty;
                var death = leaf.Death.Date.Dates?.Count > 0 ? leaf.Death.Date.Dates.FirstOrDefault().ToString("yyyy-MM-dd") : string.Empty;
                listView.Items.Add(GetListViewItem(i, $"Family: [{_getSurname(leaf)}]   Name: [{leaf.GetNames()}]   Father: [{parents.father?.GetNames() ?? string.Empty}]   Mother: [{parents.mother?.GetNames() ?? string.Empty}]   Birth: [{birth}]   Death: [{death}]"));
            }
            tabControl.SelectedIndex = 1;
        }

        private void ListViewScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - (e.Delta /10));
            e.Handled = true;
        }

        private void mni_File_ImportYbDb(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "SQLite db files (*.db)|*.db";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }
            _yizkorBookDbFilePath = openFileDialog.FileName;
            DAL dAL = new DAL(_yizkorBookDbFilePath);
            var pages = dAL.ExecuteScalar<int>($"SELECT COUNT(1) FROM Page");
            lblYbName.Content = $"Yizkor book db loaded: [{Path.GetFileName(_yizkorBookDbFilePath)}] Pages: [{pages}]";

            listViewYizkorBook.Items.Clear();
            tabControl.SelectedIndex = 1;
        }

        private void btnSearchYB_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtYbSearch.Text))
            {
                return;
            }
            listViewYizkorBook.Items.Clear();
            DAL dAL = new DAL(_yizkorBookDbFilePath);
            DataTable dataTable = dAL.Query($"\r\n                SELECT\r\n                    p.number as Page,\r\n                    l.number as Line,\r\n                    w.number as Word\r\n                    FROM\r\n                    Word w \r\n                    JOIN Line l on l.id = w.LineId\r\n                    JOIN Page p ON p.id = l.PageId\r\n                    WHERE\r\n                    w.text LIKE '%{txtYbSearch.Text}%'\r\n                    ORDER BY\r\n                    page,\r\n                    line,\r\n                    word\r\n            ");
            
            lblYbSearchResults.Content = dataTable.Rows.Count == 0 ? $"No entries found for [{txtYbSearch.Text}]. Try another term." : $"{dataTable.Rows.Count} entries found for [{txtYbSearch.Text}]. Clicking a given page column in the result grid will download and open that book page.";
            
            foreach (DataRow row in dataTable.Rows)
            {
                int.TryParse(row["page"]?.ToString(), out var page);
                int.TryParse(row["line"]?.ToString(), out var line);
                int.TryParse(row["word"]?.ToString(), out var word);

                listViewYizkorBook.Items.Add(new YBSearchResult(page, line, word));
            }
        }

        private void listViewYizkorBook_SelectionChanged(object sender, RoutedEventArgs e)
        {
            using WebClient webClient = new WebClient();
            string text = string.Empty;
            string destinationFileName = string.Empty;
            foreach (object selectedItem in listViewYizkorBook.SelectedItems)
            {
                var selectedYBSearchResult = (YBSearchResult)selectedItem;
                int num2 = 56632812 + selectedYBSearchResult.Page;
                destinationFileName = $"Page {selectedYBSearchResult.Page}.jpg";
                if (!File.Exists(destinationFileName))
                {
                    webClient.DownloadFile($"https://images.nypl.org/index.php?id={num2}&t=w", destinationFileName);
                }
            }
            if (File.Exists(destinationFileName))
            {
                Process.Start("explorer.exe", destinationFileName);
            }
        }
    }
}
