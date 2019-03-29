using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Iron_Max
{
    sealed partial class App : Application
    {

        Frame frameWindow;
        GamePage GameWindowsLaunched;
       
        public App()
        {
            
            InitializeComponent();
            Suspending += OnSuspending;
        }
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            frameWindow = Window.Current.Content as Frame;
            if(frameWindow == null)
            {
                frameWindow = new Frame();
                if(args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                { }
                frameWindow.Navigate(typeof(Menu));

                Window.Current.Content = frameWindow;
            }
            if(GameWindowsLaunched == null)
            {
                this.GameWindowsLaunched = new GamePage("");
            }
            
            Window.Current.Activate();
        }
        
        public void WindowGameLaunched()
        {
            Window.Current.Content = GameWindowsLaunched;
        }
        public void WindowMenuLaunched()
        {
            Window.Current.Content = frameWindow;
        }

       

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Save application state and stop any background activity

            deferral.Complete();
        }
    }
}
