using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace _2024_11_25
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly HttpClient httpClient = new HttpClient();
        List<Cat> cats = new List<Cat>();
        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private async void addButton_Click(object sender, RoutedEventArgs e)
        {
            HttpContent body = new StringContent(JsonConvert.SerializeObject(new
            {
                name = nameField.Text,
                age = ageField.Text
            }), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync("http://localhost:4444/api/cats", body);
            if (response.IsSuccessStatusCode)
            {
                nameField.Text = "";
                ageField.Text = "";
                LoadData();
            }
            else if ((int)response.StatusCode == 400)
            {
                MessageBox.Show(JsonConvert.DeserializeObject<ApiErrorMsg>(await response.Content.ReadAsStringAsync()).Msg);
            }
            else MessageBox.Show(response.StatusCode.ToString());

        }

        async void LoadData()
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync("http://localhost:4444/api/cats");
                response.EnsureSuccessStatusCode();
                string jsonData = await response.Content.ReadAsStringAsync();
                cats = JsonConvert.DeserializeObject<List<Cat>>(jsonData);
                UpdateTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void UpdateTable()
        {
            if (cats.Count > 0)
            {
                List<Cat> visibleCats;
                if (searchBox.Text == "") visibleCats = cats;
                else
                {
                    visibleCats = new List<Cat>();
                    foreach (Cat cat in cats)
                    {
                        if (cat.Name.Contains(searchBox.Text)) visibleCats.Add(cat);
                    }
                }
                table.DataContext = visibleCats;
                if (cats.Count == visibleCats.Count) numOfCatsText.DataContext = cats.Count;
                else numOfCatsText.DataContext = $"{cats.Count} ({visibleCats.Count} visible)";
                IOrderedEnumerable<Cat> orderedCats = cats.OrderBy(x => x.Age);
                oldestCatText.DataContext = orderedCats.Last().Name;
                youngestCatText.DataContext = orderedCats.First().Name;
            }
            else
            {
                numOfCatsText.DataContext = "";
                oldestCatText.DataContext = "";
                youngestCatText.DataContext = "";
            }
        }

        private async void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Cat cat = button.DataContext as Cat;
            try
            {
                HttpResponseMessage response = await httpClient.DeleteAsync($"http://localhost:4444/api/cats?id={cat.Id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            LoadData();
        }

        private void searchBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) => UpdateTable();

        private void table_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName.Contains("At")) e.Column.Header = e.PropertyName.Replace("A", " a");
        }
    }

    class Cat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime createdAt { private get; set; }
        public DateTime updatedAt { private get; set; }
        public string CreatedAt { get => createdAt.ToString("yyyy-MM-dd HH:mm"); }
        public string UpdatedAt { get => updatedAt.ToString("yyyy-MM-dd HH:mm"); }
    }

    class ApiErrorMsg
    {
        public string Msg { get; set; }
    }
}
