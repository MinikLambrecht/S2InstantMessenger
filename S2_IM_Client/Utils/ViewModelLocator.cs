using Unity;
using S2_IM_Client.Services;
using S2_IM_Client.ViewModels;

namespace S2_IM_Client.Utils
{
    public class ViewModelLocator
    {
        private readonly UnityContainer _container;

        public ViewModelLocator()
        {
            _container = new UnityContainer();
            _container.RegisterType<IChatService, ChatService>();
            _container.RegisterType<IDialogService, DialogService>();
        }

        public MainWindowViewModel MainVm
        {
            get { return _container.Resolve<MainWindowViewModel>(); }
        }
    }
}
