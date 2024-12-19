using System;
using System.Linq;
using System.Collections.ObjectModel;
using LiveCharts;
using LiveCharts.Wpf;
using TaskManagement.TaskManagementDataSetTableAdapters;
using TaskManagement.ViewModels;

namespace TaskManagement.MVVM.ViewModels.AdministratorPageViewModel
{
    internal class MainPageViewModel : ViewModelBase
    {
        private readonly string _currentUserLogin;

        public SeriesCollection UserRolesSeriesCollection { get; set; }
        public SeriesCollection UserActivitySeriesCollection { get; set; }

        public ObservableCollection<string> UserRolesLabels { get; set; }
        public ObservableCollection<string> UserActivityLabels { get; set; }

        public MainPageViewModel(string currentUserLogin)
        {
            _currentUserLogin = currentUserLogin;
            LoadUserRolesStatistics();
            LoadUserActivityStatistics();
        }

        private void LoadUserRolesStatistics()
        {
            var usersAdapter = new UsersTableAdapter();
            var rolesAdapter = new RolesTableAdapter();
            var dataSet = new TaskManagementDataSet();

            usersAdapter.Fill(dataSet.Users);
            rolesAdapter.Fill(dataSet.Roles);

            var roleCounts = from user in dataSet.Users
                             join role in dataSet.Roles on user.RoleID equals role.ID
                             group user by role.Name into roleGroup
                             select new { RoleName = roleGroup.Key, Count = roleGroup.Count() };

            UserRolesSeriesCollection = new SeriesCollection();
            UserRolesLabels = new ObservableCollection<string>();

            foreach (var roleStat in roleCounts)
            {
                UserRolesSeriesCollection.Add(new PieSeries
                {
                    Title = roleStat.RoleName,
                    Values = new ChartValues<int> { roleStat.Count }
                });
                UserRolesLabels.Add(roleStat.RoleName);
            }

            OnPropertyChanged(nameof(UserRolesSeriesCollection));
            OnPropertyChanged(nameof(UserRolesLabels));
        }

        private void LoadUserActivityStatistics()
        {
            var activityAdapter = new UserActivityLogTableAdapter();
            var dataSet = new TaskManagementDataSet();

            activityAdapter.Fill(dataSet.UserActivityLog);

            var activityCounts = dataSet.UserActivityLog
                .AsEnumerable()
                .GroupBy(row => ((DateTime)row["Timestamp"]).ToString("yyyy-MM-dd")) // Исправляем имя поля на "Timestamp"
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToList();

            UserActivitySeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Активность пользователей",
                    Values = new ChartValues<int>(activityCounts.Select(x => x.Count))
                }
            };

            UserActivityLabels = new ObservableCollection<string>(activityCounts.Select(x => x.Date));

            OnPropertyChanged(nameof(UserActivitySeriesCollection));
            OnPropertyChanged(nameof(UserActivityLabels));
        }
    }
}
