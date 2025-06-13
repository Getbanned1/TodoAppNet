using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using TodoAppNet;

namespace TodoAppNet
{
    public class MainWindowViewModel : BaseViewModel
    {
        private FirestoreDb _firestore;

        public ObservableCollection<TodoItem> TodoItems { get; } = new ObservableCollection<TodoItem>();

        private TodoItem _selectedTodo;
        public TodoItem SelectedTodo
        {
            get => _selectedTodo;
            set
            {
                _selectedTodo = value;
                OnPropertyChanged(nameof(SelectedTodo));
                //DeleteTodoCommand.RaiseCanExecuteChanged();
                //SaveTodoCommand.RaiseCanExecuteChanged();
            }
        }

        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
            }
        }

        public RelayCommand AddTodoCommand { get; }
        public RelayCommand DeleteTodoCommand { get; }
        public RelayCommand SaveTodoCommand { get; }
        public RelayCommand LogoutCommand { get; }

        public MainWindowViewModel(User currentUser)
        {
            CurrentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

            // Инициализация Firestore с явным указанием пути к JSON ключу сервисного аккаунта
            var credential = GoogleCredential.FromFile("todoapp-bb489-4c8135bf3abb.json");
            var builder = new FirestoreClientBuilder { Credential = credential };
            _firestore = FirestoreDb.Create("todoapp-bb489", builder.Build());

            AddTodoCommand = new RelayCommand(AddTodo);
            DeleteTodoCommand = new RelayCommand(DeleteTodo, _ => SelectedTodo != null);
            SaveTodoCommand = new RelayCommand(async () => await SaveTodoAsync(), _ => SelectedTodo != null);
            LogoutCommand = new RelayCommand(Logout);

            // Загрузка задач пользователя при инициализации
            _ = LoadTodosAsync();
        }

        private void AddTodo()
        {
            var newTodo = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Новая задача",
                CreatedAt = DateTime.UtcNow,
                IsCompleted = false
            };
            TodoItems.Add(newTodo);
            SelectedTodo = newTodo;
        }

        private void DeleteTodo()
        {
            if (SelectedTodo == null) return;

            var todoToDelete = SelectedTodo;
            TodoItems.Remove(todoToDelete);
            _ = DeleteTodoAsync(todoToDelete.Id);
            SelectedTodo = null;
        }

        private async Task SaveTodoAsync()
        {
            if (SelectedTodo == null) return;

            await AddOrUpdateTodoAsync(SelectedTodo);
        }

        private async Task LoadTodosAsync()
        {
            try
            {
                TodoItems.Clear();

                var userDoc = _firestore.Collection("users").Document(CurrentUser.Id);
                var tasksCollection = userDoc.Collection("tasks");
                var snapshot = await tasksCollection.GetSnapshotAsync();

                foreach (var doc in snapshot.Documents)
                {
                    var todo = doc.ConvertTo<TodoItem>();
                    todo.Id = doc.Id; // Убедитесь, что Id совпадает с Firestore документом
                    TodoItems.Add(todo);
                }
            }
            catch (Exception ex)
            {
                // Логирование или обработка ошибок загрузки
            }
        }

        private async Task AddOrUpdateTodoAsync(TodoItem todo)
        {
            try
            {
                var userDoc = _firestore.Collection("users").Document(CurrentUser.Id);
                var tasksCollection = userDoc.Collection("tasks");

                var todoData = new Dictionary<string, object>
                {
                    { "title", todo.Title },
                    { "description", todo.Description },
                    { "dueDate", todo.DueDate ?? Timestamp.FromDateTime(DateTime.UtcNow) },
                    { "isCompleted", todo.IsCompleted },
                    { "createdAt", todo.CreatedAt }
                };

                if (string.IsNullOrEmpty(todo.Id))
                {
                    var docRef = await tasksCollection.AddAsync(todoData);
                    todo.Id = docRef.Id;
                }
                else
                {
                    var docRef = tasksCollection.Document(todo.Id);
                    await docRef.SetAsync(todoData, SetOptions.Overwrite);
                }
            }
            catch (Exception ex)
            {
                // Логирование или обработка ошибок сохранения
            }
        }

        private async Task DeleteTodoAsync(string todoId)
        {
            try
            {
                var userDoc = _firestore.Collection("users").Document(CurrentUser.Id);
                var tasksCollection = userDoc.Collection("tasks");
                var docRef = tasksCollection.Document(todoId);
                await docRef.DeleteAsync();
            }
            catch (Exception ex)
            {
                // Логирование или обработка ошибок удаления
            }
        }

        private void Logout()
        {
            // Реализуйте логику выхода из системы по вашему сценарию
        }
    }

    [FirestoreData]
    public class TodoItem
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty("title")]
        public string Title { get; set; }

        [FirestoreProperty("description")]
        public string Description { get; set; }

        [FirestoreProperty("dueDate")]
        public Timestamp? DueDate { get; set; }

        [FirestoreProperty("isCompleted")]
        public bool IsCompleted { get; set; }

        [FirestoreProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
