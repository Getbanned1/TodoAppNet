using Firebase.Auth;
using Firebase.Auth.Providers;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Google.Apis.Auth.OAuth2;
using Grpc.Auth;
using Grpc.Core;
using System.IO;

namespace TodoAppNet
{
    public class AuthViewModel : BaseViewModel
    {
        static FirebaseAuthConfig config = new FirebaseAuthConfig
        {
            ApiKey = "AIzaSyB2J9pTBJboZ04FaS5q_UKFxYNuyFC_dJI",
            AuthDomain = "todoapp-bb489.firebaseapp.com",
            Providers = new FirebaseAuthProvider[] { new EmailProvider()}
        };
        static FirebaseAuthClient client = new FirebaseAuthClient(config: config);
        FirestoreDb _firestore  { get; set; }
        FirebaseAuthService authService = new FirebaseAuthService(client);
        private Window _currentWindow;

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        private string _errorMessage;
        private string _regErrorMessage;

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        public string RegErrorMessage
        {
            get => _regErrorMessage;
            set { _regErrorMessage = value; OnPropertyChanged(nameof(RegErrorMessage)); }
        }

        public AuthViewModel(Window window)
        {
            _currentWindow = window;

            // Явное создание Firestore с учетными данными из JSON файла
            var credential = GoogleCredential.FromFile("todoapp-bb489-4c8135bf3abb.json");
            var builder = new FirestoreClientBuilder { Credential = credential };
            _firestore = FirestoreDb.Create("todoapp-bb489", builder.Build());

            LoginCommand = new RelayCommand(async (param) => await Login(param));
            RegisterCommand = new RelayCommand(async (param) => await Register(param));
        }


        private async Task Login(object parameter)
        {
            var window = _currentWindow as Window;
            var loginTextBox = window.FindName("LoginTextBox") as TextBox;
            var passwordBox = window.FindName("PasswordBox") as PasswordBox;

            string email = loginTextBox.Text;
            string password = passwordBox.Password;

            try
            {
                var authResult = await authService.LoginAsync(email, password);

                var currentUser = new User
                {
                    Id = authResult.User?.Uid,
                    Email = email,
                    DisplayName = "", // Можно получить из authResult, если есть
                    LastSyncTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                var mainWindow = new MainWindow(currentUser);
                mainWindow.Show();
                _currentWindow.Close();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }


        private async Task Register(object parameter)
        {
            var window = _currentWindow as Window;
            var regLoginTextBox = window.FindName("RegLoginTextBox") as TextBox;
            var regPasswordBox = window.FindName("RegPasswordBox") as PasswordBox;
            var confirmPasswordBox = window.FindName("ConfirmPasswordBox") as PasswordBox;

            string email = regLoginTextBox.Text;
            string password = regPasswordBox.Password;
            string confirmPassword = confirmPasswordBox.Password;

            if (password != confirmPassword)
            {
                RegErrorMessage = "Пароли не совпадают";
                return;
            }

            try
            {
                var authresult = await authService.SignUpAsync(email, password);
                var currentUser = new User
                {
                    Id = authresult.User?.Uid,
                    Email = email,
                    DisplayName = "",
                    LastSyncTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                RegErrorMessage = "Регистрация прошла успешно!";
                await SaveUserAsync(_firestore, currentUser);

                var mainWindow = new MainWindow(currentUser);
                mainWindow.Show();
                _currentWindow.Close();
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                RegErrorMessage = ex.Message;
            }
        }

        public async Task SaveUserAsync(FirestoreDb firestore, User currentUser)
        {
            var userDoc = firestore.Collection("users").Document(currentUser.Id);

            Timestamp lastSyncTimestamp;

            if (currentUser.LastSyncTimestamp.HasValue)
            {
                lastSyncTimestamp = Timestamp.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(currentUser.LastSyncTimestamp.Value).UtcDateTime);
            }
            else
            {
                lastSyncTimestamp = Timestamp.FromDateTime(DateTime.UtcNow);
            }

            var userData = new Dictionary<string, object>
            {
                { "email", currentUser.Email },
                { "displayName", currentUser.DisplayName },
                { "photoUrl", currentUser.PhotoUrl },
                { "lastSyncTimestamp", lastSyncTimestamp }
            };

            await userDoc.SetAsync(userData, SetOptions.MergeAll);
        }

    }
}
