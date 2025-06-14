using System.Windows;
using System.Windows.Controls;

namespace TodoAppNet
{
    public partial class AddTagWindow : Window
    {
        public string TagName { get; private set; }
        public string TagColor { get; private set; }

        public AddTagWindow()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TagNameTextBox.Text))
            {
                MessageBox.Show("Введите название тега");
                return;
            }

            TagName = TagNameTextBox.Text;
            TagColor = GetColorFromCombo();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private string GetColorFromCombo()
        {
            var selectedItem = ColorComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                string content = selectedItem.Content.ToString();
                return content.Substring(0, 7); // Извлекаем HEX-код цвета
            }
            return "#FF808080"; // Серый по умолчанию
        }
    }
}