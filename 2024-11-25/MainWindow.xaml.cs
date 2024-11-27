using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using Newtonsoft.Json;

namespace _2024_11_25
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly HttpClient httpClient = new HttpClient();

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
                List<Cat> cats = JsonConvert.DeserializeObject<List<Cat>>(jsonData);
                table.DataContext = cats;
                numOfCatsText.DataContext = cats.Count;
                IEnumerable<Cat> orderedCats = cats.OrderBy(x => x.Age);
                oldestCatText.DataContext = orderedCats.Last().Name;
                youngestCatText.DataContext = orderedCats.First().Name;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

    struct Cat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    struct ApiErrorMsg
    {
        public string Msg { get; set; }
    }
}
