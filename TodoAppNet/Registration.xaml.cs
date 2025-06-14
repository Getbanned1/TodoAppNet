using System.Windows;
using System.Windows.Controls;

namespace YourNamespace
{
    public partial class Registration : Window
    {
        public Registration()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // Простая валидация
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                StatusTextBlock.Text = "Пожалуйста, заполните все поля.";
                return;
            }

            if (password != confirmPassword)
            {
                StatusTextBlock.Text = "Пароли не совпадают.";
                return;
            }

            // Здесь можно добавить логику регистрации (отправка на сервер и т.д.)
            // Для примера просто показываем сообщение
            StatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
            StatusTextBlock.Text = "Регистрация прошла успешно!";
        }
    }
}