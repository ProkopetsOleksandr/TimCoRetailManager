using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Threading.Tasks;
using System.Windows;
using TRMDesktopUI.Library.Api;
using TRMDesktopUI.Library.Models;

namespace TRMDesktopUI.ViewModels
{
    public class UserDisplayViewModel : Screen
        {
        private readonly IWindowManager _windowManager;
        private readonly IUserEndpoint _userEndPoint;

        private BindingList<UserModel> _users;
        public BindingList<UserModel> Users
        {
            get 
            { 
                return _users;
            }
            set
            {
                _users = value;
                NotifyOfPropertyChange(() => Users);
            }
        }

        public UserDisplayViewModel(IWindowManager windowManager, IUserEndpoint userEndPoint)
        {
            _windowManager = windowManager;
            _userEndPoint = userEndPoint;
        }

        protected override async void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            try
            {
                await LoadUsers();
            }
            catch (Exception ex)
            {
                var status = IoC.Get<StatusInfoViewModel>();
                status.Update("Unauthorize access", "Sorry, you don't have permission to see this page");

                dynamic settings = new ExpandoObject();
                settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                settings.ResizeMode = ResizeMode.NoResize;
                settings.Title = "System Erorr";

                await _windowManager.ShowDialogAsync(status, null, settings);
                await TryCloseAsync();
            }
        }

        private async Task LoadUsers()
        {
            var users = await _userEndPoint.GetAll();
            Users = new BindingList<UserModel>(users);
        }
    }
}
