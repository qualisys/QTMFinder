using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using QTMRealTimeSDK;


namespace QTMFinder
{
    public partial class MainWindow : Window
    {
        RTProtocol rtProtocol = new RTProtocol();
        ObservableCollection<ObservableDiscoveryResponse> observableDiscoveryResponses;
        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;

        public MainWindow()
        {
            InitializeComponent();
            RefreshDiscoveryResponses();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RefreshDiscoveryResponses();
        }

        private void IpAddress_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBlock tb = (TextBlock)sender;
            Clipboard.SetText(tb.Text);
        }

        private void RefreshDiscoveryResponses()
        {
            observableDiscoveryResponses = new ObservableCollection<ObservableDiscoveryResponse>();

            if (rtProtocol.DiscoverRTServers(4547))
            {
                HashSet<DiscoveryResponse> discoveryResponses = rtProtocol.DiscoveryResponses;
                foreach (DiscoveryResponse dr in discoveryResponses)
                {
                    ObservableDiscoveryResponse odr = new ObservableDiscoveryResponse
                    {
                        HostName = dr.HostName,
                        IpAddress = dr.IpAddress,
                        Port = dr.Port,
                        InfoText = dr.InfoText,
                        CameraCount = dr.CameraCount
                    };
                    observableDiscoveryResponses.Add(odr);
                }
            }

            ResultsListView.ItemsSource = observableDiscoveryResponses;
        }

        void GridViewColumnHeader_Clicked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("CLICK");
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    string header = headerClicked.Column.Header as string;
                    
                    // Convert header to property name
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                    string titleCaseHeader = textInfo.ToTitleCase(header.ToLower());
                    string[] splitTitleCaseHeader = titleCaseHeader.Split(' ');
                    string propName;
                    try
                    {
                        propName = splitTitleCaseHeader[0] + splitTitleCaseHeader[1];
                    }
                    catch (Exception)
                    {
                        propName = splitTitleCaseHeader[0];
                    }
                    Sort(propName, direction);

                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header  
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    {
                        _lastHeaderClicked.Column.HeaderTemplate = null;
                    }

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(ResultsListView.Items);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }


        }

    public class ObservableDiscoveryResponse : INotifyPropertyChanged
    {
        public string HostName { get; set; }
        public string IpAddress { get; set; }
        public short Port { get; set; }
        public string InfoText { get; set; }
        public int CameraCount { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
