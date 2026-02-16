using Popov_Glaski_save.database;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Popov_Glaski_save
{
    /// <summary>
    /// Логика взаимодействия для AgentPage.xaml
    /// </summary>
    public partial class AgentPage : Page
    {
        private Popov_GlaskiSaveEntities _context;
        public AgentPage()
        {
            InitializeComponent();

            _context = Popov_GlaskiSaveEntities.GetContext();

            CBoxFilter.ItemsSource = _context.AgentType.Select(at => new {
                Id = at.ID,
                Name = at.Title
            }).Prepend(new {Id = -1, Name = "Все типы"}).ToList();
            CBoxFilter.SelectedValuePath = "Id";
            CBoxFilter.DisplayMemberPath = "Name";

            CBoxFilter.SelectedIndex = 0;

            UpdateAgents();
        }

        private void UpdateAgents()
        {
            var currentAgents = _context.Agent.ToList();

            string searchText = TBoxSearch.Text.ToLower();
            string clearSearchText = new string(searchText.Where(char.IsDigit).ToArray());

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                currentAgents = currentAgents.Where(a =>
                    a.Title.ToLower().Contains(searchText) ||
                    a.Email.ToLower().Contains(searchText) ||
                    (!string.IsNullOrEmpty(clearSearchText) && a.ClearPhone.Contains(clearSearchText))
                ).ToList();
            }

            if (CBoxFilter.SelectedValue is int selectedId && selectedId != -1)
            {
                currentAgents = currentAgents.Where(a => a.AgentTypeID == selectedId).ToList();
            }

            if (CBoxSort.SelectedItem != null)
            {
                switch (CBoxSort.SelectedIndex)
                {
                    case 0:
                        currentAgents = currentAgents.OrderBy(a => a.Title).ToList();
                        break;
                    case 1:
                        currentAgents = currentAgents.OrderByDescending(a => a.Title).ToList();
                        break;
                    case 3:
                        currentAgents = currentAgents.OrderBy(a => a.Discount).ToList();
                        break;
                    case 4:
                        currentAgents = currentAgents.OrderByDescending(a => a.Discount).ToList();
                        break;
                    case 6:
                        currentAgents = currentAgents.OrderBy(a => a.Priority).ToList();
                        break;
                    case 7:
                        currentAgents = currentAgents.OrderByDescending(a => a.Priority).ToList();
                        break;
                }
            }

            ListViewAgents.ItemsSource = currentAgents;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditAgentPage());
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateAgents();
        }

        private void CBoxSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAgents();
        }

        private void CBoxFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAgents();
        }
    }
}
