using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.System;
using Windows.UI.Core;
using BrainShare.Common;
// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace BrainShare.Views
{
    public sealed partial class Settings : SettingsFlyout
    {
        public Settings()
        {
            InitializeComponent();


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
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    Constants.NotesModule = true;
                }
                else
                {
                    Constants.NotesModule = false;
                }
            }
        }
        private void Library_Toggle(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    Constants.LibraryModule = true;
                }
                else
                {
                    Constants.LibraryModule = false;
                }
            }
        }
        private void Videos_Toggle(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    Constants.VideosModule = true;
                }
                else
                {
                    Constants.VideosModule = false;
                }
            }
        }
    }
}
