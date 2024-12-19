using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace TaskManagement.Helpers
{
    public static class PasswordHelper
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password", typeof(SecureString), typeof(PasswordHelper), new PropertyMetadata(default(SecureString)));

        public static SecureString GetPassword(DependencyObject obj)
        {
            return (SecureString)obj.GetValue(PasswordProperty);
        }

        public static void SetPassword(DependencyObject obj, SecureString value)
        {
            obj.SetValue(PasswordProperty, value);
        }

        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(PasswordHelper), new PropertyMetadata(false, OnAttachChanged));

        public static bool GetAttach(DependencyObject obj)
        {
            return (bool)obj.GetValue(AttachProperty);
        }

        public static void SetAttach(DependencyObject obj, bool value)
        {
            obj.SetValue(AttachProperty, value);
        }

        private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                if ((bool)e.NewValue)
                {
                    passwordBox.PasswordChanged += OnPasswordChanged;
                }
                else
                {
                    passwordBox.PasswordChanged -= OnPasswordChanged;
                }
            }
        }

        private static void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                SetPassword(passwordBox, passwordBox.SecurePassword);
            }
        }
    }
}
