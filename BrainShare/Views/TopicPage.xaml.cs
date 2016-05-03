using BrainShare.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using BrainShare.Models;
using BrainShare.ViewModels;
using Windows.Storage;
using Windows.Storage.Streams;



// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace BrainShare.Views
{
    
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class TopicPage : Page
    {
        string all_notes = null;
           
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return navigationHelper; }
        }
          

        public TopicPage()
        {

            InitializeComponent();
            navigationHelper = new NavigationHelper(this);
            navigationHelper.LoadState += navigationHelper_LoadState;
            navigationHelper.SaveState += navigationHelper_SaveState;
            
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
             var topic = e.NavigationParameter as TopicObservable;
             TopicPageViewModel vm = new TopicPageViewModel(topic);
             DataContext = vm;
             all_notes = topic.body;
            
        }

        private async void WebView2_Loaded(object sender, RoutedEventArgs e)
        {

            //To be removed after tests

            //string imagePAth = @"C:\Users\Kenneth\AppData\Local\Packages\CodeVisionLtd.BrainShare_fd97ja0290hr0\LocalState\Biology_ECOLOGY_notes_image1.png";
            //string imagePAth = @"D:\UnSorted Content\Junk\Jh\0db0e0c312ee26ad7e303804a5449f91.jpg";
            //string notesfromJson = "<br><div><br></div><div><img src=";
            //string notesfromJson = "<img src=";
            //string quote = "\"";

            //string x = " width=" + quote + "470" + "><br></div><div>";

            //string header = @"file:///";

            //string pth2 = "file:///D:/UnSorted%20Content/Junk/Jh/02.jpg";

            //string x = " width=" + quote + "900" + "><br>";

            StorageFolder appFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await appFolder.GetFileAsync("00b8170e-318b-4f31-8f2b-fca44ec3099b.jpg");
            string base64 = "";
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                var reader = new DataReader(stream.GetInputStreamAt(0));
                var bytes = new byte[stream.Size];
                await reader.LoadAsync((uint)stream.Size);
                reader.ReadBytes(bytes);
                base64 = Convert.ToBase64String(bytes);
            }

            string tester = "<html><head><title>Image test</title></head><body><p>This is a test app!</p><img src=\"data:image/png;base64" + base64 + "\" /></body></html>";
            
            //string test = notesfromJson + quote + pth2 + quote + x;

            //< img src = "http://i.imgur.com/dApHyaa.gif" width = "600" >< br >

            //D: \UnSorted Content\Junk\Jh\0db0e0c312ee26ad7e303804a5449f91.jpg

                var WebView = (WebView)sender;
            //string content = WebViewContentHelper.WrapHtml(all_notes, WebView.ActualWidth, WebView.ActualHeight);
            string content = WebViewContentHelper.WrapHtml(tester, WebView.ActualWidth, WebView.ActualHeight);
            WebView.NavigateToString(content);
        }

        private void File_click(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem;
            AttachmentObservable _file = ((AttachmentObservable)item);
            Frame.Navigate(typeof(PDFViewPage), _file);
        }

        private void itemGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        /// 
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e); 
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

       
    }
}
