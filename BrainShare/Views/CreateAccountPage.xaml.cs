using BrainShare.Common;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace BrainShare.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateAccountPage : Page
    {
        public CreateAccountPage()
        {
            InitializeComponent();
            
        }
        private void WebView2_Loaded(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri(Constants.FullBaseUri);
            WebView2.Navigate(uri);
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Uri uri = new Uri(Constants.FullBaseUri);
            WebView2.Navigate(uri);
        }
    }   
}
