using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
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
        }
        private void Notes_Toggle(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values[_noteskey] = !Notes_Module.IsOn;
        }
        private void Library_Toggle(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values[_libkey] = !Library_Module.IsOn;
        }
        private void Videos_Toggle(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values[_videoskey] = !Videos_Module.IsOn;
        }
    }
}
