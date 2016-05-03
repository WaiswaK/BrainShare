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
using System.Net.Http;
using System.Runtime.Serialization;
using Windows.UI.ApplicationSettings;
using Windows.System;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace BrainShare
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        UserDetail Stats = new UserDetail();
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
            Uri uri = new Uri("http://learn.brainshare.ug/privacy_policy");
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
            try
            {
                navigationHelper.OnNavigatedFrom(e);
            }
            catch
            {

            }
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
                // MessageDialog msgDialog = new MessageDialog("Access Denied. Cant write to file.", "R/W Error!");
                // await msgDialog.ShowAsync();
                //surpressed Access Denied Mthd - jams writing to file at times

                String error = ex.ToString();
                string header = "Save"; //for test purposes
                var message = new MessageDialog(error, header).ShowAsync(); // for testing purposes


            }
            //Login starts here
            Login();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                await RestoreAsync();
            }
            catch (Exception exc) { String word = exc.ToString(); }

            base.OnNavigatedTo(e); //important to cover this error http://stackoverflow.com/questions/13790344/argumentnullexception-on-changing-frame
            navigationHelper.OnNavigatedTo(e);

            //SettingsPane.GetForCurrentView().CommandsRequested += onCommandsRequested; // Added here

        }
        private async Task RestoreAsync()
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("UserDetails");
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
            StorageFile userdetailsfile = await ApplicationData.Current.LocalFolder.CreateFileAsync("UserDetails", CreationCollisionOption.ReplaceExisting);
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
            catch
            {

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
            List<User> users = CommonTask.SelectAllUsers();
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
                        loggedIn.School = CommonTask.GetSchool(user.School_id);
                        loggedIn.full_names = user.profileName;
                        loggedIn.Library = CommonTask.GetLibrary(loggedIn.School.SchoolId);
                        string[] SplitSubjectId = user.subjects.Split(delimiter);
                        List<string> SubjectIdList = SplitSubjectId.ToList();
                        List<int> subjectids = CommonTask.SubjectIdsConvert(SubjectIdList);
                        foreach (var id in subjectids)
                        {
                            SubjectObservable subject = CommonTask.GetSubject(id);
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
            List<User> users = CommonTask.SelectAllUsers();
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
                        loggedIn.School = CommonTask.GetSchool(user.School_id);
                        loggedIn.full_names = user.profileName;
                        loggedIn.Library = CommonTask.GetLibrary(loggedIn.School.SchoolId);
                        string[] SplitSubjectId = user.subjects.Split(delimiter);
                        List<string> SubjectIdList = SplitSubjectId.ToList();
                        List<int> subjectids = CommonTask.SubjectIdsConvert(SubjectIdList);
                        foreach (var id in subjectids)
                        {
                            SubjectObservable subject = CommonTask.GetSubject(id);
                            UserSubjects.Add(subject);
                        }
                        loggedIn.subjects = UserSubjects;
                        found = true;
                        loggedIn.update_status = Constants.finished_update;
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
                var client = new HttpClient();
                var postData = new List<KeyValuePair<string, string>>();
                postData.Add(new KeyValuePair<string, string>("email", email_tb.Text));
                postData.Add(new KeyValuePair<string, string>("password", password_tb.Password));
                var formContent = new FormUrlEncodedContent(postData);
                var authresponse = await client.PostAsync("http://brainshare.ug/liveapis/authenticate.json", formContent);
                var authresult = await authresponse.Content.ReadAsStreamAsync();
                var authstreamReader = new System.IO.StreamReader(authresult);
                var authresponseContent = authstreamReader.ReadToEnd().Trim().ToString();
                var user = JsonObject.Parse(authresponseContent);

                CreateUser(user, email_tb.Text, password_tb.Password);

            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                string header = "Authentication"; //For testing purposes
                var message = new MessageDialog(error, header).ShowAsync(); //For testing purposes
                //var message = new MessageDialog(Message.Connection_Error, Message.Connection_Error_Header).ShowAsync();
                loadingRing.IsActive = false;
                LoadingMsg.Visibility = Visibility.Collapsed;
            }
        }
        private async void CreateUser(JsonObject loginObject, string username, string password)
        {
            LoginStatus user = CommonTask.Notification(loginObject);
            UserObservable userdetails = new UserObservable();
            SubjectObservable subject = new SubjectObservable();
            LibraryObservable Library = new LibraryObservable();
            if (user.statusCode.Equals("200") && user.statusDescription.Equals("Authentication was successful"))
            {
                userdetails.email = username;
                userdetails.password = password;
                userdetails.School = CommonTask.GetSchool(loginObject);
                userdetails.full_names = CommonTask.GetUsername(loginObject);

                try
                {
                    var library_httpclient = new HttpClient();
                    var library_postData = new List<KeyValuePair<string, string>>();
                    library_postData.Add(new KeyValuePair<string, string>("email", username));
                    library_postData.Add(new KeyValuePair<string, string>("password", password));
                    library_postData.Add(new KeyValuePair<string, string>("id", userdetails.School.SchoolId.ToString()));
                    var library_formContent = new FormUrlEncodedContent(library_postData);
                    var library_response = await library_httpclient.PostAsync("http://brainshare.ug/liveapis/books.json", library_formContent);
                    var library_result = await library_response.Content.ReadAsStreamAsync();
                    var library_streamReader = new StreamReader(library_result);
                    var library_responseContent = library_streamReader.ReadToEnd().Trim().ToString();
                    var library = JsonArray.Parse(library_responseContent);
                    Library = CommonTask.GetLibrary(library, userdetails.School.SchoolId);
                }
                catch
                {

                }
                userdetails.Library = Library;
                LoadingMsg.Text = Message.Syncronization;
                LoadingMsg.Visibility = Visibility.Visible;
                try
                {
                    var client = new HttpClient();
                    var postData = new List<KeyValuePair<string, string>>();
                    postData.Add(new KeyValuePair<string, string>("email", email_tb.Text));
                    postData.Add(new KeyValuePair<string, string>("password", password_tb.Password));
                    var formContent = new FormUrlEncodedContent(postData);
                    var courseresponse = await client.PostAsync("http://brainshare.ug/liveapis/course_units.json", formContent);
                    var coursesresult = await courseresponse.Content.ReadAsStreamAsync();
                    var coursestreamReader = new System.IO.StreamReader(coursesresult);
                    var courseresponseContent = coursestreamReader.ReadToEnd().Trim().ToString();
                    var subjects = JsonArray.Parse(courseresponseContent);

                    List<SubjectObservable> courses = new List<SubjectObservable>();
                    List<int> IDs = CommonTask.SubjectIds(subjects);
                    foreach (var id in IDs)
                    {
                        var notes_httpclient = new HttpClient();
                        var notes_postData = new List<KeyValuePair<string, string>>();
                        notes_postData.Add(new KeyValuePair<string, string>("email", username));
                        notes_postData.Add(new KeyValuePair<string, string>("password", password));
                        notes_postData.Add(new KeyValuePair<string, string>("id", id.ToString()));
                        var notes_formContent = new FormUrlEncodedContent(notes_postData);
                        var notes_response = await notes_httpclient.PostAsync("http://brainshare.ug/liveapis/uni_notes.json", notes_formContent);
                        var notes_result = await notes_response.Content.ReadAsStreamAsync();
                        var notes_streamReader = new System.IO.StreamReader(notes_result);
                        var notes_responseContent = notes_streamReader.ReadToEnd().Trim().ToString();
                        var notes = JsonArray.Parse(notes_responseContent);

                        var videos_httpclient = new HttpClient();
                        var videospostData = new List<KeyValuePair<string, string>>();
                        videospostData.Add(new KeyValuePair<string, string>("email", username));
                        videospostData.Add(new KeyValuePair<string, string>("password", password));
                        videospostData.Add(new KeyValuePair<string, string>("id", id.ToString()));
                        var videosformContent = new FormUrlEncodedContent(videospostData);
                        var videosresponse = await videos_httpclient.PostAsync("http://brainshare.ug/liveapis/uni_videos.json", videosformContent);
                        var videosresult = await videosresponse.Content.ReadAsStreamAsync();
                        var videosstreamReader = new System.IO.StreamReader(videosresult);
                        var videosresponseContent = videosstreamReader.ReadToEnd().Trim().ToString();
                        var videos = JsonArray.Parse(videosresponseContent);

                        var assgnmt_httpclient = new HttpClient();
                        var assgnmt_postData = new List<KeyValuePair<string, string>>();
                        assgnmt_postData.Add(new KeyValuePair<string, string>("email", username));
                        assgnmt_postData.Add(new KeyValuePair<string, string>("password", password));
                        assgnmt_postData.Add(new KeyValuePair<string, string>("id", id.ToString()));
                        var assgnmt_formContent = new FormUrlEncodedContent(assgnmt_postData);
                        var assgnmt_response = await assgnmt_httpclient.PostAsync("http://brainshare.ug/liveapis/assignments.json", assgnmt_formContent);
                        var assgnmt_result = await assgnmt_response.Content.ReadAsStreamAsync();
                        var assgnmt_streamReader = new System.IO.StreamReader(assgnmt_result);
                        var assgnmt_responseContent = assgnmt_streamReader.ReadToEnd().Trim().ToString();
                        var assignments = JsonArray.Parse(assgnmt_responseContent);

                        var file_httpclient = new HttpClient();
                        var file_postData = new List<KeyValuePair<string, string>>();
                        file_postData.Add(new KeyValuePair<string, string>("email", username));
                        file_postData.Add(new KeyValuePair<string, string>("password", password));
                        file_postData.Add(new KeyValuePair<string, string>("id", id.ToString()));
                        var file_formContent = new FormUrlEncodedContent(file_postData);
                        var file_response = await file_httpclient.PostAsync("http://brainshare.ug/liveapis/uni_files.json", file_formContent);
                        var file_result = await file_response.Content.ReadAsStreamAsync();
                        var file_streamReader = new System.IO.StreamReader(file_result);
                        var file_responseContent = file_streamReader.ReadToEnd().Trim().ToString();
                        var files = JsonArray.Parse(file_responseContent); 
                       
                        subject = CommonTask.GetSubject(subjects, id, notes, videos, assignments, files);
                        courses.Add(subject);
                    }
                    userdetails.subjects = courses;
                    CommonTask.InsertLibAsync(userdetails.Library); //Library add here
                    AuthenticateUser(userdetails);
                }
                catch (Exception ex)
                {
                    string error = ex.ToString();
                    string header = "Subjects error"; //For testing purposes
                    //var message = new MessageDialog(Message.Connection_Error, Message.Connection_Error_Header).ShowAsync();   //syncronising error msg here
                    var message = new MessageDialog(error, header).ShowAsync(); // for testing purposes
                    loadingRing.IsActive = false;
                    LoadingMsg.Visibility = Visibility.Collapsed;
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
            List<User> users = CommonTask.SelectAllUsers();
            bool found = false;
            lib = user.Library;
            subs = user.subjects;
            if (subs.Count > 0)
            {
                if (CommonTask.IsInternetConnectionAvailable())
                {
                    if (users == null)
                    {
                        await CommonTask.InsertUserAsync(user);
                        CommonTask.InsertSubjectsAsync(user.subjects);
                        user.update_status = Constants.finished_update;
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
                        if (CommonTask.oldSubjects() != null)
                        {
                            subs = CommonTask.new_subjects(user.subjects);
                            if (subs == null) { }
                            else
                            {
                                CommonTask.InsertSubjectsAsync(user.subjects);
                                if (found == false)
                                {
                                    try
                                    {
                                        await CommonTask.InsertUserAsync(user);
                                    }
                                    catch(Exception ex)
                                    {
                                        string word = ex.ToString();
                                    }
                                }
                            }
                        }
                        else
                        {
                            await CommonTask.InsertUserAsync(user);
                            CommonTask.InsertSubjectsAsync(user.subjects);
                        }
                        user.update_status = Constants.finished_update;
                        Frame.Navigate(typeof(StudentPage), user);
                    }
                }
                else
                {
                    if (CommonTask.oldSubjects() == null)
                    {
                        var message = new MessageDialog(Message.Offline_Message, Message.Content_Header).ShowAsync();
                        loadingRing.IsActive = false;
                        LoadingMsg.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        user.update_status = Constants.finished_update;
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

