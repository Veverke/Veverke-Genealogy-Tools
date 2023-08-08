using GenealogySoftwareV3.Types;
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

namespace WPFLab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GEDCOMDefails _dataContext;
        public MainWindow()
        {
            InitializeComponent();
            _dataContext = new GEDCOMDefails();
            this.DataContext = _dataContext;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem rootTreeViewItem = new TreeViewItem { Header = "Family Tree" };
            List<int> rootMarriages = new List<int>(new int[] { 8, 135, 144 });
            Marriage rootMarriageMatch;

            foreach (int rootMarriage in rootMarriages)
                if (ofdGEDCOM.fileInterpreter.Marriages.TryGetValue(rootMarriage, out rootMarriageMatch))
                    ofdGEDCOM.fileInterpreter.GenerateTree(rootTreeViewItem, rootMarriageMatch);

            treeView.Items.Add(rootTreeViewItem);

            //ofdGEDCOM.fileInterpreter.GetFamiliesRoots();
        }

        private void btnShowGeographicDistribution_Click(object sender, RoutedEventArgs e)
        {
            listView.Items.Clear();
            var distinctCountries = ofdGEDCOM.fileInterpreter.Individuals.Select(i => i.Value.Birth.Place.Country).Distinct().OrderBy(c => c);
            for (var i = 0; i < distinctCountries.Count(); i++)
            {
                var country = distinctCountries.ElementAt(i);
                listView.Items.Add(new ListViewItem { Content = $"({i + 1}) {country}" });
            }
        }

        private void btnShowSurmames_Click(object sender, RoutedEventArgs e)
        {
            listView.Items.Clear();
            var distinctSurnames = ofdGEDCOM.fileInterpreter.Individuals.SelectMany(i => i.Value.Surnames).Distinct().OrderBy(s => s);
            for(var i = 0; i < distinctSurnames.Count(); i++)
            {
                var surname = distinctSurnames.ElementAt(i);
                listView.Items.Add(new ListViewItem { Content = $"({i + 1}) {surname}" });
            }

            listView.Items.Add(new ListViewItem { Content = "----------------------- Total individuals per surname ----------------------------------" });
            var surnameGroups = ofdGEDCOM.fileInterpreter.Individuals.GroupBy(i => string.Join(",", i.Value.Surnames)).OrderByDescending(g => g.Count());
            foreach(var surnameGroup in surnameGroups)
            {
                listView.Items.Add(new ListViewItem { Content = $"{surnameGroup.Key} [{surnameGroup.Count()}]" });
            }
        }

        private void btnShowCausesOfDeathDistribution_Click(object sender, RoutedEventArgs e)
        {
            listView.Items.Clear();
            var causesOfDeathGroups = ofdGEDCOM.fileInterpreter.Individuals.GroupBy(i => i.Value.Death.CauseofDeath).OrderByDescending(g => g.Count());
            foreach(var causeOfDeathGroup in causesOfDeathGroups)
            {
                listView.Items.Add(new ListViewItem { Content = $"{causeOfDeathGroup.Key} [{causeOfDeathGroup.Count()}]" });
            }
        }
    }
}
