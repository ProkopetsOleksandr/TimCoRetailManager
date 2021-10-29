using Caliburn.Micro;
using System.Threading;
using System.Threading.Tasks;
using TRMDesktopUI.EventModels;

namespace TRMDesktopUI.ViewModels
{
    public class ShellViewModel : Conductor<object>, IHandle<LogOnEvent>
    {
        private SalesViewModel _salesVM;
        private IEventAggregator _eventAggregator;

        public ShellViewModel(IEventAggregator eventAggregator, SalesViewModel salesVM)
        {
            _salesVM = salesVM;

            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);

            ActivateItemAsync(IoC.Get<LoginViewModel>());
        }

        public async Task HandleAsync(LogOnEvent message, CancellationToken cancellationToken)
        {
            await ActivateItemAsync(_salesVM);
        }
    }
}
