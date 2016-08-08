using BrainShare.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Streams;
using BrainShare.Models;
using BrainShare.Views;
using Windows.UI.Popups;
using BrainShare.Database;
using System.Runtime.Serialization;
using Windows.UI.ApplicationSettings;
using Windows.System;
using BrainShare.Core;
using BrainShare.ViewModels;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace BrainShare
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class LoginView : Page
    {
        ErrorLogTask Logfile = new ErrorLogTask();
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
        public LoginView()
        {
            InitializeComponent();
            SettingsPane.GetForCurrentView().CommandsRequested += onCommandsRequested;
            navigationHelper = new NavigationHelper(this);
            navigationHelper.LoadState += navigationHelper_LoadState;
            navigationHelper.SaveState += navigationHelper_SaveState;    
        }

        //New method for Settings
        private void onCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs e)
        {
            e.Request.ApplicationCommands.Add(new SettingsCommand("privacypolicy", "Privacy policy", OpenPrivacyPolicy));
            e.Request.ApplicationCommands.Add(new SettingsCommand("defaults", "Modules",
                (handler) =>
                {
                    Settings sf = new Settings();
                    sf.Show();
                }));
        }
        
                
    private async void OpenPrivacyPolicy(IUICommand command)
        {
            Uri uri = new Uri(Constant.PrivacyPolicyUri);
            await Launcher.LaunchUriAsync(uri);
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
            LoginViewModel vm = new LoginViewModel();
            DataContext = vm;
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
        #region NavigationHelper registration
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.   
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
                navigationHelper.OnNavigatedFrom(e);
            //SettingsPane.GetForCurrentView().CommandsRequested -= onCommandsRequested; // Added here
        }
        #endregion
        public void Button_Click(object sender, RoutedEventArgs e)
        {
            loadingRing.IsActive = true;
            LoadingMsg.Text = Message.User_Validation;
            LoadingMsg.Visibility = Visibility.Visible;
            Login();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e); 
            navigationHelper.OnNavigatedTo(e);
        }
        public async void Login()
        {
             try
             {
                 await CommonTask.InitializeDatabase();
             }
             catch(Exception ex)
             {
                Logfile.Error_details = ex.ToString();
                Logfile.Error_title = "Login Method";
                Logfile.Location = "LoginView";
                ErrorLogTask.LogFileSaveAsync(Logfile);
            }
             if (CommonTask.IsInternetConnectionAvailable())
             {
                 OnlineExperience();
             }
             else
             {
                 OfflineExperience();
             }            
        }
        private void OfflineExperience()
        {
            List<User> users = DBRetrievalTask.SelectAllUsers();
            if (users == null)
            {
                var message = new MessageDialog(Message.Login_Message_Fail, Message.Login_Header).ShowAsync();
                loadingRing.IsActive = false;
                LoadingMsg.Visibility = Visibility.Collapsed;
            }
            else
            {
                bool found = false;
                bool success = false;
                List<SubjectModel> UserSubjects = new List<SubjectModel>();
                UserModel loggedIn = new UserModel();
                char[] delimiter = { '.' };

                foreach (var user in users)
                {
                    if (user.e_mail.Equals(email_tb.Text) && user.password.Equals(password_tb.Password))
                    {
                        loggedIn.email = user.e_mail;
                        loggedIn.password = user.password;
                        loggedIn.School = DBRetrievalTask.GetSchool(user.School_id);
                        loggedIn.full_names = user.profileName;
                        loggedIn.Library = DBRetrievalTask.GetLibrary(loggedIn.School.SchoolId);
                        string[] SplitSubjectId = user.subjects.Split(delimiter);
                        List<string> SubjectIdList = SplitSubjectId.ToList();
                        List<int> subjectids = ModelTask.SubjectIdsConvert(SubjectIdList);
                        foreach (var id in subjectids)
                        {
                            SubjectModel subject = DBRetrievalTask.GetSubject(id);
                            UserSubjects.Add(subject);
                        }
                        loggedIn.subjects = UserSubjects;
                        success = true;
                        found = true;
                        AuthenticateUser(loggedIn);
                    }
                    else if (user.e_mail.Equals(email_tb.Text) && !user.password.Equals(password_tb.Password))
                    {
                        found = true;
                    }
                }

                if (found == true && success == false)
                {
                    var message = new MessageDialog("Wrong Login Information", "Login Fail").ShowAsync();
                    loadingRing.IsActive = false;
                    LoadingMsg.Visibility = Visibility.Collapsed;
                }
                if (found == false)
                {
                    var message = new MessageDialog("You need to be online to login", "Login Fail").ShowAsync();
                    loadingRing.IsActive = false;
                    LoadingMsg.Visibility = Visibility.Collapsed;
                }
            }
        }
        private void OnlineExperience()
        {
            List<User> users = new List<User>();
            try
            {
                users = DBRetrievalTask.SelectAllUsers();
            }
            catch
            {
                users = null;
            }
            bool found = false;
            List<SubjectModel> UserSubjects = new List<SubjectModel>();
            UserModel loggedIn = new UserModel();
            char[] delimiter = { '.' };

            if (users == null)
            {
                OnlineLogin();
            }
            else
            {
                foreach (var user in users)
                {
                    if (user.e_mail.Equals(email_tb.Text) && user.password.Equals(password_tb.Password))
                    {
                        loggedIn.email = user.e_mail;
                        loggedIn.password = user.password;
                        loggedIn.School = DBRetrievalTask.GetSchool(user.School_id);
                        loggedIn.full_names = user.profileName;
                        loggedIn.Library = DBRetrievalTask.GetLibrary(loggedIn.School.SchoolId);
                        string[] SplitSubjectId = user.subjects.Split(delimiter);
                        List<string> SubjectIdList = SplitSubjectId.ToList();
                        List<int> subjectids = ModelTask.SubjectIdsConvert(SubjectIdList);
                        foreach (var id in subjectids)
                        {
                            SubjectModel subject = DBRetrievalTask.GetSubject(id);
                            UserSubjects.Add(subject);
                        }
                        loggedIn.subjects = UserSubjects;
                        found = true;
                        loggedIn.update_status = Constant.finished_update;
                        loggedIn.NotesImagesDownloading = false;
                        Frame.Navigate(typeof(StudentView), loggedIn);
                    }
                }
                if (found == false)
                {
                    OnlineLogin();
                }
            }
        }
        private async void OnlineLogin()
        {
            try
            {
                CreateUser(await JSONTask.LoginJsonObject(email_tb.Text, password_tb.Password), email_tb.Text, password_tb.Password);
            }
            catch (Exception ex)
            {
                loadingRing.IsActive = false;
                LoadingMsg.Visibility = Visibility.Collapsed;
                Logfile.Error_details = ex.ToString();
                Logfile.Error_title = "Online Login Method";
                Logfile.Location = "LoginView";
                ErrorLogTask.LogFileSaveAsync(Logfile);
            }
        }
        private async void CreateUser(JsonObject loginObject, string username, string password)
        {
            LoginStatus user = JSONTask.Notification(loginObject);
            UserModel userdetails = new UserModel();
            LibraryModel Library = new LibraryModel();
            if (user.statusCode.Equals("200") && user.statusDescription.Equals("Authentication was successful"))
            {
                userdetails.email = username;
                userdetails.password = password;
                userdetails.School = JSONTask.GetSchool(loginObject);
                userdetails.full_names = JSONTask.GetUsername(loginObject);

                try
                {
                    Library = await JSONTask.Current_Library(username, password, userdetails.School.SchoolId);
                }
                catch(Exception ex)
                {
                    Logfile.Error_details = ex.ToString();
                    Logfile.Error_title = "CreateUser Method";
                    Logfile.Location = "LoginView";
                    ErrorLogTask.LogFileSaveAsync(Logfile);
                }
                userdetails.Library = Library;
                LoadingMsg.Text = Message.Syncronization;
                LoadingMsg.Visibility = Visibility.Visible;
                try
                {
                    JsonArray subjects = await JSONTask.SubjectsJsonArray(username, password);
                    List<int> IDs = JSONTask.SubjectIds(subjects);
                    userdetails.subjects = await JSONTask.Get_New_Subjects(username, password, IDs, subjects);
                    DBInsertionTask.InsertLibAsync(userdetails.Library); //Library add here
                    AuthenticateUser(userdetails);
                }
                catch (Exception ex)
                {
                    loadingRing.IsActive = false;
                    LoadingMsg.Visibility = Visibility.Collapsed;
                    Logfile.Error_details = ex.ToString();
                    Logfile.Error_title = "Create Method";
                    Logfile.Location = "LoginView";
                    ErrorLogTask.LogFileSaveAsync(Logfile);
                }
            }
            else
            {
                var message = new MessageDialog(Message.Wrong_User_details, Message.Login_Header).ShowAsync();
                loadingRing.IsActive = false;
                LoadingMsg.Visibility = Visibility.Collapsed;
            }
        }
        private async void AuthenticateUser(UserModel user)
        {
            List<SubjectModel> subs = new List<SubjectModel>();
            List<User> users = new List<User>();
            try
            {
                users = DBRetrievalTask.SelectAllUsers();
            }
            catch
            {
                users = null;
            }
            bool found = false;
            LibraryModel lib = user.Library;
            subs = user.subjects;
            if (subs.Count > 0)
            {
                if (CommonTask.IsInternetConnectionAvailable())
                {
                    if (users == null)
                    {
                        await DBInsertionTask.InsertUserAsync(user);
                        DBInsertionTask.InsertSubjectsAsync(user.subjects);
                        user.update_status = Constant.finished_update;
                        user.NotesImagesDownloading = false;
                        Frame.Navigate(typeof(StudentView), user);
                    }
                    else
                    {
                        foreach (var profile in users)
                        {
                            if (profile.e_mail.Equals(user.email))
                            {
                                found = true;
                            }
                        }
                        if (ModelTask.oldSubjects() != null)
                        {
                            subs = ModelTask.new_subjects(user.subjects);
                            if (subs == null) { }
                            else
                            {
                                DBInsertionTask.InsertSubjectsAsync(user.subjects);
                                if (found == false)
                                {
                                    try
                                    {
                                        await DBInsertionTask.InsertUserAsync(user);
                                    }
                                    catch(Exception ex)
                                    {
                                        Logfile.Error_details = ex.ToString();
                                        Logfile.Error_title = "AuthenticateUser Method";
                                        Logfile.Location = "LoginView";
                                        ErrorLogTask.LogFileSaveAsync(Logfile);
                                    }
                                }
                            }
                        }
                        else
                        {
                            await DBInsertionTask.InsertUserAsync(user);
                            DBInsertionTask.InsertSubjectsAsync(user.subjects);
                        }
                        user.update_status = Constant.finished_update;
                        user.NotesImagesDownloading = false;
                        Frame.Navigate(typeof(StudentView), user);
                    }
                }
                else
                {
                    if (ModelTask.oldSubjects() == null)
                    {
                        var message = new MessageDialog(Message.Offline_Message, Message.Content_Header).ShowAsync();
                        loadingRing.IsActive = false;
                        LoadingMsg.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        user.update_status = Constant.finished_update;
                        user.NotesImagesDownloading = false;
                        Frame.Navigate(typeof(StudentView), user);
                    }
                }
            }
            else
            {
                var message = new MessageDialog(Message.No_Subject, Message.No_Subject_Header).ShowAsync();
                loadingRing.IsActive = false;
                LoadingMsg.Visibility = Visibility.Collapsed;
            }
        }
    }
}

