using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.System;
using Windows.UI.Core;
using Windows.Storage;
// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace BrainShare.Views
{
    public sealed partial class Settings : SettingsFlyout
    {
        private const string _noteskey = "Notes";
        private const string _libkey = "Library";
        private const string _videoskey = "Videos";
        public Settings()
        {
            InitializeComponent();

            // Initialize the ToggleSwitch controls
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(_noteskey))
                Notes_Module.IsOn = !(bool)ApplicationData.Current.LocalSettings.Values[_noteskey];
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(_noteskey))
                Videos_Module.IsOn = !(bool)ApplicationData.Current.LocalSettings.Values[_videoskey];
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(_noteskey))
                Library_Module.IsOn = !(bool)ApplicationData.Current.LocalSettings.Values[_libkey];

            // Handle all key events when loaded into visual tree
            Loaded += (sender, e) =>
            {
                Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += SettingsFlyout1_AcceleratorKeyActivated;
            };
            Unloaded += (sender, e) =>
            {
                Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -= SettingsFlyout1_AcceleratorKeyActivated;
            };
        }
        void SettingsFlyout1_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            // Only investigate further when Left is pressed
            if (args.EventType == CoreAcceleratorKeyEventType.SystemKeyDown &&
                args.VirtualKey == VirtualKey.Left)
            {
                var coreWindow = Window.Current.CoreWindow;
                var downState = CoreVirtualKeyStates.Down;

                // Check for modifier keys
                // The Menu VirtualKey signifies Alt
                bool menuKey = (coreWindow.GetKeyState(VirtualKey.Menu) & downState) == downState;
                bool controlKey = (coreWindow.GetKeyState(VirtualKey.Control) & downState) == downState;
                bool shiftKey = (coreWindow.GetKeyState(VirtualKey.Shift) & downState) == downState;

                if (menuKey && !controlKey && !shiftKey)
                {
                    args.Handled = true;
                    Hide();
                }
            }
        }
        private void Notes_Toggle(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values[_noteskey] = !(bool)Notes_Module.IsOn;
        }
        private void Library_Toggle(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values[_libkey] = !(bool)Library_Module.IsOn;
        }
        private void Videos_Toggle(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values[_videoskey] = !(bool)Videos_Module.IsOn;
        }
    }
}
