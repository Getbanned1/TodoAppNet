using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System;
using System.Windows.Controls;  

namespace TodoAppNet
{
    public partial class MainWindow : Window
    {
        private Point _dragStartPoint;
        private ListViewItem _draggedItem;

        public MainWindow(User currentUser)
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(currentUser);
            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // Очистка ресурсов при закрытии окна
            if (DataContext is MainWindowViewModel viewModel)
            {
                // Дополнительная очистка при необходимости
            }
        }

        private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            _draggedItem = FindAncestor<ListViewItem>(e.OriginalSource as DependencyObject);
        }

        private void ListView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _draggedItem == null)
                return;

            Point currentPosition = e.GetPosition(null);
            Vector diff = _dragStartPoint - currentPosition;

            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                DragDropEffects dragEffect = DragDropEffects.Move;
                DataObject dragData = new DataObject(typeof(TodoItem), _draggedItem.DataContext);
                DragDrop.DoDragDrop(_draggedItem, dragData, dragEffect);
            }
        }

        private async void ListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TodoItem)))
            {
                TodoItem droppedItem = e.Data.GetData(typeof(TodoItem)) as TodoItem;
                ListViewItem targetItem = FindAncestor<ListViewItem>(e.OriginalSource as DependencyObject);

                if (droppedItem != null && targetItem != null && _draggedItem != null)
                {
                    TodoItem targetData = targetItem.DataContext as TodoItem;
                    var viewModel = DataContext as MainWindowViewModel;

                    if (viewModel != null && droppedItem != targetData)
                    {
                        int oldIndex = viewModel.TodoItems.IndexOf(droppedItem);
                        int newIndex = viewModel.TodoItems.IndexOf(targetData);

                        if (oldIndex != -1 && newIndex != -1)
                        {
                            viewModel.TodoItems.Move(oldIndex, newIndex);
                            await viewModel.UpdateTaskOrderAsync();
                        }
                    }
                }
            }
            ResetDragState();
        }

        private void ListView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TodoItem)))
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;

                var targetItem = FindAncestor<ListViewItem>(e.OriginalSource as DependencyObject);
                if (targetItem != null)
                {
                    targetItem.Background = Brushes.LightBlue;
                }
            }
        }

        private void ListView_DragEnter(object sender, DragEventArgs e)
        {
            var targetItem = FindAncestor<ListViewItem>(e.OriginalSource as DependencyObject);
            if (targetItem != null)
            {
                targetItem.Background = Brushes.LightBlue;
            }
        }

    
        private void ListView_DragLeave(object sender, DragEventArgs e)
        {
            var targetItem = FindAncestor<ListViewItem>(e.OriginalSource as DependencyObject);
            if (targetItem != null)
            {
                targetItem.ClearValue(Control.BackgroundProperty);
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private void ResetDragState()
        {
            _draggedItem = null;
            _dragStartPoint = default(Point);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.Logout();
            }
        }
    }
}