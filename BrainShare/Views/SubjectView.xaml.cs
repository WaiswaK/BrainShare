using BrainShare.Common;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BrainShare.Models;
using BrainShare.ViewModels;
using Windows.UI.Xaml;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace BrainShare.Views
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class SubjectView: Page
    {
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
        public SubjectView()
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
            var subject = e.NavigationParameter as SubjectModel;
            if (subject.videos.Count == 0)
                Videos.Visibility = Visibility.Collapsed;
            if (subject.topics.Count == 0)
                Folders.Visibility = Visibility.Collapsed;
            if (subject.assignments.Count == 0)
                Assignments.Visibility = Visibility.Collapsed;
            if (subject.files.Count == 0)
                Files.Visibility = Visibility.Collapsed;
            SubjectViewModel vm = new SubjectViewModel(subject);
            DataContext = vm;  
        }        
        private void Topic_click(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem;
            FolderModel _folder = ((FolderModel)item);
            Frame.Navigate(typeof(TopicsView), _folder); 
        }
        private void Book_click(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem;
            AttachmentModel _file = ((AttachmentModel)item);
            Frame.Navigate(typeof(PDFReader), _file);
        }
        private void Video_click(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem;
            VideoModel _file = ((VideoModel)item);
            Frame.Navigate(typeof(PlayView), _file);
        }
        private void Assignment_click(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem;
            AssignmentModel _assignment = ((AssignmentModel)item);
            Frame.Navigate(typeof(AssignmentView), _assignment);
        }
        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }       
        private void itemGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
