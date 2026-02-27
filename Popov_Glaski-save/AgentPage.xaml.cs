using Popov_Glaski_save.database;
using Popov_Glaski_save.services;

using System;
using System.Collections.Generic;
using System.IO;
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

namespace Popov_Glaski_save
{
    /// <summary>
    /// Логика взаимодействия для AgentPage.xaml
    /// </summary>
    public partial class AgentPage : Page
    {
        private Popov_GlaskiSaveEntities _context;
        private MessageService _messageService = new MessageService();

        private List<Agent> _filteredAgents;
        private int pageSize = 10;
        private int currentPage = 1;

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

            int totalNumAgents = currentAgents.Count;

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

            int currentNumAgents = currentAgents.Count;

            TBlockNumRecords.Text = $"{currentNumAgents} из {totalNumAgents}";

            _filteredAgents = currentAgents;
            currentPage = 1;
            ChangePage();
        }

        private void ChangePage()
        {
            LBoxPages.Items.Clear();

            int totalPages = (_filteredAgents.Count + pageSize - 1) / pageSize;

            for (int i = 1; i <= totalPages; i++)
            {
                LBoxPages.Items.Add(i);
            }

            LBoxPages.SelectedItem = currentPage;

            var agentsPage = _filteredAgents
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize).ToList();

            ListViewAgents.ItemsSource = agentsPage;
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

        private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (_filteredAgents.Count + pageSize - 1) / pageSize;
            if (currentPage > 1)
            {
                currentPage--;
                ChangePage();
            }
        }

        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (_filteredAgents.Count + pageSize - 1) / pageSize;
            if (currentPage < totalPages)
            {
                currentPage++;
                ChangePage();
            }
        }

        private void LBoxPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LBoxPages.SelectedItem is int page && page != currentPage)
            {
                currentPage = page;
                ChangePage();
            }
        }

        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is Agent agent)
            {
                var addEditAgentWindow = new AddEditAgentWindow(agent);

                string oldShortRelativeLogoPath = agent.Logo;

                try
                {
                    if (addEditAgentWindow.ShowDialog() == true)
                    {
                        _context.SaveChanges();

                        UpdateAgents();

                        await Task.Delay(500);
                        GC.Collect();
                        await Task.Delay(200);

                        string absoluteLogoPath = "";
                        if (!string.IsNullOrEmpty(oldShortRelativeLogoPath))
                        {
                            absoluteLogoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", "images", oldShortRelativeLogoPath.TrimStart('/'));
                            DeletePrevLogo(absoluteLogoPath);
                        }
                    }
                    else
                    {
                        _context.Entry(agent).Reload();
                        UpdateAgents();
                    }
                }
                catch (Exception ex)
                {
                    _messageService.ShowError($"Ошибка сохранения информации.\n\n{ex.Message}");
                }
            }
        }

        private void DeletePrevLogo(string absolutePath)
        {
            string shortRelativePath = $"/agents/{Path.GetFileName(absolutePath)}";

            try
            {
                if (!_context.Agent.Any(a => a.Logo == shortRelativePath))
                {
                    File.Delete(absolutePath);
                    _messageService.ShowInfo($"Предыдущий логотип \"{Path.GetFileName(absolutePath)}\" успешно удалён.");
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Ошибка удаления предыдущего логотипа.\n\n{ex.Message}");
            }
        }

        private void BtnAddAgent_Click(object sender, RoutedEventArgs e)
        {
            var AddEditAgentWindow = new AddEditAgentWindow(null);

            try
            {
                if (AddEditAgentWindow.ShowDialog() == true)
                {
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Ошибка добавления нового агента.\n\n{ex.Message}");
            }

            UpdateAgents();
        }
    }
}
