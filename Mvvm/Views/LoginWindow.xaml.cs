using System;
using System.Windows;
using System.Windows.Controls;
using TaskManagement.ViewModels;

namespace TaskManagement.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.LoginViewModel(); // Здесь можно использовать DI вместо создания нового экземпляра
            Console.WriteLine("LoginWindow initialized and DataContext set");
        }

        /// <summary>
        /// Обработчик для изменения пароля в PasswordBox.
        /// Связывает текст PasswordBox с ViewModel через привязку.
        /// </summary>
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.UserPassword = ((PasswordBox)sender).Password;
            }
        }

        /// <summary>
        /// Закрывает окно при нажатии на кнопку закрытия.
        /// </summary>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Разворачивает или восстанавливает окно при нажатии на кнопку разворачивания.
        /// </summary>
        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        /// <summary>
        /// Сворачивает окно при нажатии на кнопку сворачивания.
        /// </summary>
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Позволяет перемещать окно при зажатии мыши на панели управления.
        /// </summary>
        private void plnControlBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// Изменяет стиль элементов управления панели управления при наведении.
        /// </summary>
        private void pnlControlBar_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Здесь можно добавить логику для изменения стиля панели при наведении мыши.
        }
    }
}
