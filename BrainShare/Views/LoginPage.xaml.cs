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
    public sealed partial class LoginPage : Page
    {
        UserDetail Stats = new UserDetail();
        ErrorLogTask Logfile = new ErrorLogTask();

    public class UserDetail
        {
            public string email { get; set; }
            public string password { get; set; }
        }
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
        public LoginPage()
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
            Uri uri = new Uri(Constants.PrivacyPolicyUri);
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
            LoginPageViewModel vm = new LoginPageViewModel();
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
        public async void Button_Click(object sender, RoutedEventArgs e)
        {
            loadingRing.IsActive = true;
            LoadingMsg.Text = Message.User_Validation;
            LoadingMsg.Visibility = Visibility.Visible;

            //Write remember data to file
            //capture and store user data
            Stats.email = email_tb.Text;

            Stats.password = password_tb.Password;
            try
            {
                await SaveAsync();
            }
            catch (Exception ex)
            {
                Logfile.Error_details = ex.ToString();
                Logfile.Error_title = "Button_Click Method";
                Logfile.Location = "LoginPage";
                ErrorLogTask.LogFileSaveAsync(Logfile);
            }
            Login();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                await RestoreAsync();
            }
            catch (Exception exc)
            {
                Logfile.Error_details = exc.ToString();
                Logfile.Error_title = "OnNavigatedTo";
                Logfile.Location = "LoginPage";
                ErrorLogTask.LogFileSaveAsync(Logfile);
            }

            base.OnNavigatedTo(e); //important to cover this error http://stackoverflow.com/questions/13790344/argumentnullexception-on-changing-frame
            navigationHelper.OnNavigatedTo(e);

            //SettingsPane.GetForCurrentView().CommandsRequested += onCommandsRequested; // Added here

        }
        private async Task RestoreAsync()
        {
            StorageFile file = await Constants.appFolder.GetFileAsync("UserDetails");
            if (file == null) return;
            IRandomAccessStream inStream = await file.OpenReadAsync();
            // Deserialize the Session State.
            DataContractSerializer serializer = new DataContractSerializer(typeof(UserDetail));
            var StatsDetails = (UserDetail)serializer.ReadObject(inStream.AsStreamForRead());
            inStream.Dispose();
            email_tb.Text = StatsDetails.email;
            password_tb.Password = StatsDetails.password;
        }
        private async Task SaveAsync()
        {
            StorageFile userdetailsfile = await Constants.appFolder.CreateFileAsync("UserDetails", CreationCollisionOption.ReplaceExisting);
            IRandomAccessStream raStream = await userdetailsfile.OpenAsync(FileAccessMode.ReadWrite);
            using (IOutputStream outStream = raStream.GetOutputStreamAt(0))
            {
                // Serialize the Session State.
                DataContractSerializer serializer = new DataContractSerializer(typeof(UserDetail));
                serializer.WriteObject(outStream.AsStreamForWrite(), Stats);
                await outStream.FlushAsync();
            }
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
                Logfile.Location = "LoginPage";
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
            List<User> users = DatabaseOutputTask.SelectAllUsers();
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
                List<SubjectObservable> UserSubjects = new List<SubjectObservable>();
                UserObservable loggedIn = new UserObservable();
                char[] delimiter = { '.' };

                foreach (var user in users)
                {
                    if (user.e_mail.Equals(email_tb.Text) && user.password.Equals(password_tb.Password))
                    {
                        loggedIn.email = user.e_mail;
                        loggedIn.password = user.password;
                        loggedIn.School = DatabaseOutputTask.GetSchool(user.School_id);
                        loggedIn.full_names = user.profileName;
                        loggedIn.Library = DatabaseOutputTask.GetLibrary(loggedIn.School.SchoolId);
                        string[] SplitSubjectId = user.subjects.Split(delimiter);
                        List<string> SubjectIdList = SplitSubjectId.ToList();
                        List<int> subjectids = ModelTask.SubjectIdsConvert(SubjectIdList);
                        foreach (var id in subjectids)
                        {
                            SubjectObservable subject = DatabaseOutputTask.GetSubject(id);
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
                users = DatabaseOutputTask.SelectAllUsers();
            }
            catch
            {
                users = null;
            }
            bool found = false;
            List<SubjectObservable> UserSubjects = new List<SubjectObservable>();
            UserObservable loggedIn = new UserObservable();
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
                        loggedIn.School = DatabaseOutputTask.GetSchool(user.School_id);
                        loggedIn.full_names = user.profileName;
                        loggedIn.Library = DatabaseOutputTask.GetLibrary(loggedIn.School.SchoolId);
                        string[] SplitSubjectId = user.subjects.Split(delimiter);
                        List<string> SubjectIdList = SplitSubjectId.ToList();
                        List<int> subjectids = ModelTask.SubjectIdsConvert(SubjectIdList);
                        foreach (var id in subjectids)
                        {
                            SubjectObservable subject = DatabaseOutputTask.GetSubject(id);
                            UserSubjects.Add(subject);
                        }
                        loggedIn.subjects = UserSubjects;
                        found = true;
                        loggedIn.update_status = Constants.finished_update;
                        loggedIn.NotesImagesDownloading = false;
                        Frame.Navigate(typeof(StudentPage), loggedIn);
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
                Logfile.Location = "LoginPage";
                ErrorLogTask.LogFileSaveAsync(Logfile);
            }
        }
        private async void CreateUser(JsonObject loginObject, string username, string password)
        {
            LoginStatus user = JSONTask.Notification(loginObject);
            UserObservable userdetails = new UserObservable();
            LibraryObservable Library = new LibraryObservable();
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
                    Logfile.Location = "LoginPage";
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
                    DatabaseInputTask.InsertLibAsync(userdetails.Library); //Library add here
                    AuthenticateUser(userdetails);
                }
                catch (Exception ex)
                {
                    loadingRing.IsActive = false;
                    LoadingMsg.Visibility = Visibility.Collapsed;
                    Logfile.Error_details = ex.ToString();
                    Logfile.Error_title = "Create Method";
                    Logfile.Location = "LoginPage";
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
        private async void AuthenticateUser(UserObservable user)
        {
            List<SubjectObservable> subs = new List<SubjectObservable>();
            LibraryObservable lib = new LibraryObservable();
            List<User> users = DatabaseOutputTask.SelectAllUsers();
            bool found = false;
            lib = user.Library;
            subs = user.subjects;
            if (subs.Count > 0)
            {
                if (CommonTask.IsInternetConnectionAvailable())
                {
                    if (users == null)
                    {
                        await DatabaseInputTask.InsertUserAsync(user);
                        DatabaseInputTask.InsertSubjectsAsync(user.subjects);
                        user.update_status = Constants.finished_update;
                        user.NotesImagesDownloading = false;
                        Frame.Navigate(typeof(StudentPage), user);
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
                                DatabaseInputTask.InsertSubjectsAsync(user.subjects);
                                if (found == false)
                                {
                                    try
                                    {
                                        await DatabaseInputTask.InsertUserAsync(user);
                                    }
                                    catch(Exception ex)
                                    {
                                        Logfile.Error_details = ex.ToString();
                                        Logfile.Error_title = "AuthenticateUser Method";
                                        Logfile.Location = "LoginPage";
                                        ErrorLogTask.LogFileSaveAsync(Logfile);
                                    }
                                }
                            }
                        }
                        else
                        {
                            await DatabaseInputTask.InsertUserAsync(user);
                            DatabaseInputTask.InsertSubjectsAsync(user.subjects);
                        }
                        user.update_status = Constants.finished_update;
                        user.NotesImagesDownloading = false;
                        Frame.Navigate(typeof(StudentPage), user);
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
                        user.update_status = Constants.finished_update;
                        user.NotesImagesDownloading = false;
                        Frame.Navigate(typeof(StudentPage), user);
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

