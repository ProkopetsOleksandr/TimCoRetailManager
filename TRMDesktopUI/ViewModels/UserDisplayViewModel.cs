using Caliburn.Micro;
using System;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
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

        private UserModel _selectedUser;

        public UserModel SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value;
                SelectedUserName = value.Email;
                UserRole = new BindingList<string>(value.Roles.Values.ToList());
                LoadRoles();
                NotifyOfPropertyChange(() => SelectedUser);
            }
        }

        private string _selectedUserName;

        public string SelectedUserName
        {
            get { return _selectedUserName; }
            set
            {
                _selectedUserName = value;
                NotifyOfPropertyChange(() => SelectedUserName);
            }
        }


        private BindingList<string> _availableRoles = new BindingList<string>();

        public BindingList<string> AvailableRole
        {
            get { return _availableRoles; }
            set
            {
                _availableRoles = value;
                NotifyOfPropertyChange(() => AvailableRole);

            }
        }

        private BindingList<string> _userRole = new BindingList<string>();

        public BindingList<string> UserRole
        {
            get { return _userRole; }
            set 
            {
                _userRole = value;
                NotifyOfPropertyChange(() => UserRole);
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
            catch (Exception)
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

        private async Task LoadRoles()
        {
            var roles = await _userEndPoint.GetAllRoles();

            foreach (var role in roles)
            {
                if (!UserRole.Contains(role.Value))
                {
                    AvailableRole.Add(role.Value);
                }
            }
        }

        
        private string _selectedAvailableRole;
        private string _selectedUserRole;

        public string SelectedAvailableRole
        {
            get { return _selectedAvailableRole; }
            set 
            { 
                _selectedAvailableRole = value;
                NotifyOfPropertyChange(() => SelectedAvailableRole);
            }
        }

        public string SelectedUserRole
        {
            get { return _selectedUserRole; }
            set
            {
                _selectedUserRole = value;
                NotifyOfPropertyChange(() => SelectedUserRole);
            }
        }

        public async void AddSelectedRole()
        {
            await _userEndPoint.AddUserToRole(SelectedUser.Id, SelectedAvailableRole);

            UserRole.Add(SelectedAvailableRole);
            AvailableRole.Remove(SelectedAvailableRole);
        }

        public async void RemoveSelectedRole()
        {
            await _userEndPoint.RemoveUserToRole(SelectedUser.Id, SelectedUserRole);

            AvailableRole.Add(SelectedUserRole);
            UserRole.Remove(SelectedUserRole);
        }
    }
}
