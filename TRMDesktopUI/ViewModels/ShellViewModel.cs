using Caliburn.Micro;
using System.Threading;
using System.Threading.Tasks;
using TRMDesktopUI.EventModels;
using TRMDesktopUI.Library.Models;

namespace TRMDesktopUI.ViewModels
{
    public class ShellViewModel : Conductor<object>, IHandle<LogOnEvent>
    {
        private SalesViewModel _salesVM;
        private IEventAggregator _eventAggregator;
        private ILoggedInUserModel _user;

        public ShellViewModel(IEventAggregator eventAggregator, SalesViewModel salesVM, ILoggedInUserModel user)
        {
            _salesVM = salesVM;
            _user = user;

            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);

            ActivateItemAsync(IoC.Get<LoginViewModel>());
        }

        public void Exit()
        {
            TryCloseAsync();
        }

        public void LogOut()
        {
            _user.ResetUserModel();
            ActivateItemAsync(IoC.Get<LoginViewModel>());
            NotifyOfPropertyChange(() => IsLoggedIn);
        }

        public async Task HandleAsync(LogOnEvent message, CancellationToken cancellationToken)
        {
            await ActivateItemAsync(_salesVM);
            NotifyOfPropertyChange(() => IsLoggedIn);
        }

        public bool IsLoggedIn => !string.IsNullOrWhiteSpace(_user.Token);
    }
}
