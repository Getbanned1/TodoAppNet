using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Google.Apis.Auth.OAuth2;

namespace TodoAppNet
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly FirestoreDb _firestore;
        private TodoItem _selectedTodo;
        private User _currentUser;

        public ObservableCollection<TodoItem> TodoItems { get; } = new ObservableCollection<TodoItem>();
        public ObservableCollection<Tag> AvailableTags { get; } = new ObservableCollection<Tag>();

        public TodoItem SelectedTodo
        {
            get => _selectedTodo;
            set
            {
                _selectedTodo = value;
                OnPropertyChanged(nameof(SelectedTodo));
                DeleteTodoCommand.RaiseCanExecuteChanged();
                SaveTodoCommand.RaiseCanExecuteChanged();
                if (_selectedTodo != null)
                {
                    _ = LoadTagsForTaskAsync(_selectedTodo);
                }
            }
        }

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
        public RelayCommand AddTagCommand { get; }
        public RelayCommand RemoveTagCommand { get; }
        public RelayCommand OpenAddTagWindowCommand { get; }

        public MainWindowViewModel(User currentUser)
        {
            CurrentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

            // Инициализация Firestore
            var credential = GoogleCredential.FromFile("todoapp-bb489-4c8135bf3abb.json");
            var builder = new FirestoreClientBuilder { Credential = credential };
            _firestore = FirestoreDb.Create("todoapp-bb489", builder.Build());

            // Инициализация команд
            AddTodoCommand = new RelayCommand(AddTodo);
            DeleteTodoCommand = new RelayCommand(DeleteTodo, _ => SelectedTodo != null);
            SaveTodoCommand = new RelayCommand(async () => await SaveTodoAsync(), _ => SelectedTodo != null);
            LogoutCommand = new RelayCommand(Logout);
            AddTagCommand = new RelayCommand(AddTagToSelectedTodo);
            RemoveTagCommand = new RelayCommand(RemoveTagFromSelectedTodo);
            OpenAddTagWindowCommand = new RelayCommand(OpenAddTagWindow);

            // Загрузка данных
            _ = LoadInitialDataAsync();
        }

        private void OpenAddTagWindow()
        {
            var addTagWindow = new AddTagWindow();
            if (addTagWindow.ShowDialog() == true)
            {
                var newTag = new Tag
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = addTagWindow.TagName,
                    Color = addTagWindow.TagColor
                };

                _ = AddTagToFirestoreAsync(newTag);
            }
        }

        private async Task AddTagToFirestoreAsync(Tag tag)
        {
            try
            {
                var tagRef = _firestore.Collection("tags").Document(tag.Id);
                await tagRef.SetAsync(new
                {
                    name = tag.Name,
                    color = tag.Color
                }, SetOptions.Overwrite);

                AvailableTags.Add(tag);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления тега: {ex.Message}");
            }
        }

        
        private async Task LoadInitialDataAsync()
        {
            try
            {
                await Task.WhenAll(
                    LoadTodosAsync(),
                    LoadAvailableTagsAsync()
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        public async Task LoadTagsForTaskAsync(TodoItem task)
        {
            try
            {
                if (task == null) return;

                var userDoc = _firestore.Collection("users").Document(CurrentUser.Id);
                var taskDoc = userDoc.Collection("tasks").Document(task.Id);
                var tagsSnapshot = await taskDoc.Collection("tags").GetSnapshotAsync();

                task.Tags.Clear();
                foreach (var doc in tagsSnapshot.Documents)
                {
                    var tag = doc.ConvertTo<Tag>();
                    tag.Id = doc.Id;
                    task.Tags.Add(tag);
                }
                OnPropertyChanged(nameof(SelectedTodo));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки тегов: {ex.Message}");
            }
        }

        private async Task LoadAvailableTagsAsync()
        {
            try
            {
                AvailableTags.Clear();
                var snapshot = await _firestore.Collection("tags").GetSnapshotAsync();

                foreach (var doc in snapshot.Documents)
                {
                    var tag = doc.ConvertTo<Tag>();
                    tag.Id = doc.Id;
                    AvailableTags.Add(tag);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки доступных тегов: {ex.Message}");
            }
        }

        private async Task LoadTodosAsync()
        {
            try
            {
                TodoItems.Clear();
                var userDoc = _firestore.Collection("users").Document(CurrentUser.Id);
                var query = userDoc.Collection("tasks").OrderBy("sortOrder");
                var snapshot = await query.GetSnapshotAsync();

                foreach (var doc in snapshot.Documents)
                {
                    var todo = doc.ConvertTo<TodoItem>();
                    todo.Id = doc.Id;
                    await LoadTagsForTaskAsync(todo);
                    TodoItems.Add(todo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки задач: {ex.Message}");
            }
        }

        private void AddTodo()
        {
            try
            {
                var newTodo = new TodoItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Новая задача",
                    CreatedAt = DateTime.UtcNow,
                    IsCompleted = false,
                    SortOrder = TodoItems.Count,
                    Tags = new List<Tag>()
                };
                TodoItems.Add(newTodo);
                SelectedTodo = newTodo;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания задачи: {ex.Message}");
            }
        }

        private async void DeleteTodo()
        {
            if (SelectedTodo == null) return;

            try
            {
                var result = MessageBox.Show("Удалить эту задачу?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await DeleteTodoAsync(SelectedTodo.Id);
                    TodoItems.Remove(SelectedTodo);
                    SelectedTodo = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }


            private async Task SaveTodoAsync()
        {
            if (SelectedTodo == null) return;

            try
            {
                await AddOrUpdateTodoAsync(SelectedTodo);
                MessageBox.Show("Задача сохранена!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
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
                    { "description", todo.Description ?? string.Empty },
                    { "dueDate", todo.DueDate ?? Timestamp.FromDateTime(DateTime.UtcNow) },
                    { "isCompleted", todo.IsCompleted },
                    { "createdAt", todo.CreatedAt },
                    { "sortOrder", todo.SortOrder }
                };

                if (string.IsNullOrEmpty(todo.Id))
                {
                    var docRef = await tasksCollection.AddAsync(todoData);
                    todo.Id = docRef.Id;
                }
                else
                {
                    await tasksCollection.Document(todo.Id).SetAsync(todoData, SetOptions.Overwrite);
                }

                await SyncTagsForTaskAsync(todo);
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка сохранения задачи", ex);
            }
        }

        private async Task SyncTagsForTaskAsync(TodoItem task)
        {
            try
            {
                var userDoc = _firestore.Collection("users").Document(CurrentUser.Id);
                var taskDoc = userDoc.Collection("tasks").Document(task.Id);
                var tagsCollection = taskDoc.Collection("tags");

                // Удаляем отсутствующие теги
                var existingTags = await tagsCollection.GetSnapshotAsync();
                foreach (var doc in existingTags.Documents)
                {
                    if (!task.Tags.Any(t => t.Id == doc.Id))
                    {
                        await doc.Reference.DeleteAsync();
                    }
                }

                // Добавляем/обновляем теги
                foreach (var tag in task.Tags)
                {
                    var tagData = new Dictionary<string, object>
                    {
                        { "name", tag.Name },
                        { "color", tag.Color ?? "#FF808080" }
                    };
                    await tagsCollection.Document(tag.Id).SetAsync(tagData, SetOptions.MergeAll);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка синхронизации тегов", ex);
            }
        }

        private async Task DeleteTodoAsync(string todoId)
        {
            try
            {
                var userDoc = _firestore.Collection("users").Document(CurrentUser.Id);
                await userDoc.Collection("tasks").Document(todoId).DeleteAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка удаления задачи", ex);
            }
        }

        private async void AddTagToSelectedTodo(object parameter)
        {
            if (SelectedTodo == null || !(parameter is Tag tag)) return;

            try
            {
                if (!SelectedTodo.Tags.Any(t => t.Id == tag.Id))
                {
                    SelectedTodo.Tags.Add(tag);

                    
                    var userDoc = _firestore.Collection("users").Document(CurrentUser.Id);
                    var taskDoc = userDoc.Collection("tasks").Document(SelectedTodo.Id);
                    var tagDoc = taskDoc.Collection("tags").Document(tag.Id);

                    await tagDoc.SetAsync(new
                    {
                        name = tag.Name,
                        color = tag.Color ?? "#FF808080"
                    }, SetOptions.MergeAll);

                    OnPropertyChanged(nameof(SelectedTodo));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления тега: {ex.Message}");
            }
        }

        private async void RemoveTagFromSelectedTodo(object parameter)
        {
            if (SelectedTodo == null || !(parameter is Tag tag)) return;

            try
            {
                SelectedTodo.Tags.RemoveAll(t => t.Id == tag.Id);

                var userDoc = _firestore.Collection("users").Document(CurrentUser.Id);
                var taskDoc = userDoc.Collection("tasks").Document(SelectedTodo.Id);
                var tagDoc = taskDoc.Collection("tags").Document(tag.Id);

                await tagDoc.DeleteAsync();

                OnPropertyChanged(nameof(SelectedTodo));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления тега: {ex.Message}");
            }
        }

        public async Task UpdateTaskOrderAsync()
        {
            try
            {
                var userDoc = _firestore.Collection("users").Document(CurrentUser.Id);
                var batch = _firestore.StartBatch();

                for (int i = 0; i < TodoItems.Count; i++)
                {
                    var taskDoc = userDoc.Collection("tasks").Document(TodoItems[i].Id);
                    batch.Update(taskDoc, "sortOrder", i);
                    TodoItems[i].SortOrder = i;
                }

                await batch.CommitAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления порядка задач: {ex.Message}");
            }
        }

        public void Logout()
        {
            try
            {
                var authWindow = new AuthView();
                authWindow.Show();

                foreach (Window window in Application.Current.Windows)
                {
                    if (window is MainWindow)
                    {
                        window.Close();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выходе: {ex.Message}");
            }
        }
    }
}