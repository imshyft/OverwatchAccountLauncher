﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using Studio.Contracts.Views;
using Studio.Core.Contracts.Services;
using Studio.Core.Models;
using Wpf.Ui.Controls;
using Button = System.Windows.Controls.Button;

namespace Studio.Views
{
    /// <summary>
    /// Interaction logic for AccountListPage.xaml
    /// </summary>
    public partial class AccountListPage : Page
    {
        public ObservableCollection<ProfileData> ProfileData { get; set; } = new ObservableCollection<ProfileData>();
        private bool _mouseOverButton = false;
        public AccountListPage(ISampleDataService sampleDataService)
        {
            InitializeComponent();
            
            DataContext = this;


            foreach (var profile in sampleDataService.GetFavouriteProfiles())
            {
                ProfileData.Add(profile);
            }

            AccountDataGrid.SelectedItem = null;
        }


        private void OnProfileListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && !_mouseOverButton)
            {
                NavigationService?.Navigate(new AccountDetailsPage((e.AddedItems[0] as ProfileData)));
            }

            AccountDataGrid.SelectedItem = null;
        }

        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject source)
            {
                var row = VisualTreeHelper.GetParent(source) as DataGridRow;
                if (row != null)
                {
                    NavigationService?.Navigate(new AccountDetailsPage((row.DataContext as ProfileData)));
                }
            }
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ProfileData profile = ((FrameworkElement)sender).DataContext as ProfileData;
            Debug.WriteLine($"Launching Profile {profile.Account.Name}");
            e.Handled = true;
        }


        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e) => _mouseOverButton = true;
        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e) => _mouseOverButton = false;

    }
}
