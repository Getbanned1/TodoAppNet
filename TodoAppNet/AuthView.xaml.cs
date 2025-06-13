using System.Windows;
using TodoAppNet;
using System;
using System.Windows.Controls;
namespace TodoAppNet
{
    public partial class AuthView : Window
    {
        public AuthView()
        {
            InitializeComponent();
            DataContext = new AuthViewModel(this);
        }
    }
}