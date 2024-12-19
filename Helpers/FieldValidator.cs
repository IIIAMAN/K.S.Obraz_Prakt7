using System;
using System.Text.RegularExpressions;

namespace TaskManagement.Helpers
{
    public static class FieldValidator
    {
        // Метод для проверки, что строка не является null или пустой
        public static bool IsNotNullOrEmpty(string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        // Метод для проверки, что строка состоит только из букв
        public static bool IsAlphabetic(string value)
        {
            return IsNotNullOrEmpty(value) && Regex.IsMatch(value, @"^[A-Za-zА-Яа-я]+$");
        }

        // Метод для проверки имени и фамилии
        public static bool IsValidName(string name)
        {
            if (!IsNotNullOrEmpty(name))
                return false;

            // Удаляем пробелы и проверяем, что оставшиеся символы - это только буквы
            string trimmedName = name.Replace(" ", "");
            return IsAlphabetic(trimmedName) && trimmedName.Length >= 2;
        }

        // Метод для проверки логина
        public static bool IsValidLogin(string login, Func<string, bool> isLoginUnique)
        {
            if (!IsNotNullOrEmpty(login))
                return false;

            // Удаляем пробелы и проверяем, что оставшиеся символы - это только буквы и цифры
            string trimmedLogin = login.Replace(" ", "");
            return Regex.IsMatch(trimmedLogin, @"^[A-Za-zА-Яа-я0-9]+$") && trimmedLogin.Length >= 3 && isLoginUnique(trimmedLogin);
        }

        // Метод для проверки пароля
        public static bool IsValidPassword(string password)
        {
            if (!IsNotNullOrEmpty(password))
                return false;

            string trimmedPassword = password.Replace(" ", "");
            return trimmedPassword.Length >= 5;
        }

        // Метод для проверки даты
        public static bool IsValidDate(DateTime? date)
        {
            if (date == null)
                return false;

            return date.Value >= DateTime.Now && date.Value.Year <= 2030;
        }

        // Метод для проверки номера телефона (русский)
        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (!IsNotNullOrEmpty(phoneNumber))
                return false;

            string phonePattern = @"^\+7\d{10}$";
            return Regex.IsMatch(phoneNumber, phonePattern);
        }

        // Метод для проверки описания
        public static bool IsValidDescription(string description)
        {
            if (!IsNotNullOrEmpty(description))
                return false;

            string trimmedDescription = description.Replace(" ", "");
            return trimmedDescription.Length >= 1;
        }

        // Метод для проверки наименования
        public static bool IsValidNameField(string name)
        {
            if (!IsNotNullOrEmpty(name))
                return false;

            string trimmedName = name.Replace(" ", "");
            return IsAlphabetic(trimmedName) && trimmedName.Length >= 3;
        }
    }
}
