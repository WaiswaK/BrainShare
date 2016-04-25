using BrainShare.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BrainShare.ViewModels;
using BrainShare.Models;
using BrainShare.Database;
using System.Threading.Tasks;
using System.Net.Http;
using Windows.UI.Popups;
using Windows.Data.Json;



// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace BrainShare.Views
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class StudentPage : Page
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


        public StudentPage()
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
            var user = e.NavigationParameter as UserObservable;
            UserObservable initial = user;
            char[] delimiter = { '.' };
            List<SubjectObservable> subjectsNew = new List<SubjectObservable>();
            var db = new SQLite.SQLiteConnection(Constants.dbPath);
            var query = (db.Table<User>().Where(c => c.e_mail == user.email)).Single();

            string[] SplitSubjectId = query.subjects.Split(delimiter);
            List<string> SubjectIdList = SplitSubjectId.ToList();
            List<int> subjectids = CommonTask.SubjectIdsConvert(SubjectIdList);
            foreach (var id in subjectids)
            {
                SubjectObservable subject = CommonTask.GetSubject(id);
                subjectsNew.Add(subject);
            }

            user.subjects = subjectsNew;

            //Library Update methods
            LibraryObservable lib = CommonTask.GetLibrary(user.School.SchoolId);
            user.Library = lib;

            StudentPageViewModel vm = new StudentPageViewModel(user);
            DataContext = vm;
            if (user.update_status == Constants.finished_update)
            {
                if (CommonTask.IsInternetConnectionAvailable())
                {
                    UpdateUser(initial.email, initial.password, CommonTask.SubjectIdsForUser(initial.email), user.subjects, user);
                   // CommonTask.GetNotesImagesSubjectsAsync(user.subjects);//Update Notes //Needs to be awaited
                }
            }
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
        private async void UpdateUser(string username, string password, List<int> oldIDs, List<SubjectObservable> InstalledSubjects, UserObservable currentUser)
        {
            UserObservable userdetails = new UserObservable();
            SubjectObservable subject = new SubjectObservable();
            List<SubjectObservable> final = new List<SubjectObservable>();
            LibraryObservable Current_Library = new LibraryObservable();
            LibraryObservable Old_Library = CommonTask.GetLibrary(currentUser.School.SchoolId);
            currentUser.update_status = Constants.updating;

            pgBar.Visibility = Visibility.Visible;
            // LoadingMsg.Text = "Checking for updates...";
            // LoadingMsg.Visibility = Visibility.Visible;

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
                var library_streamReader = new System.IO.StreamReader(library_result);
                var library_responseContent = library_streamReader.ReadToEnd().Trim().ToString();
                var library = JsonArray.Parse(library_responseContent);
                Current_Library = CommonTask.GetLibrary(library, userdetails.School.SchoolId);
            }
            catch
            {

            }
            try
            {
                var client = new HttpClient();
                var postData = new List<KeyValuePair<string, string>>();
                postData.Add(new KeyValuePair<string, string>("email", username));
                postData.Add(new KeyValuePair<string, string>("password", password));
                var formContent = new FormUrlEncodedContent(postData);
                var authresponse = await client.PostAsync("http://brainshare.ug/liveapis/authenticate.json", formContent);
                var authresult = await authresponse.Content.ReadAsStreamAsync();
                var authstreamReader = new System.IO.StreamReader(authresult);
                var authresponseContent = authstreamReader.ReadToEnd().Trim().ToString();
                var loginObject = JsonObject.Parse(authresponseContent);

                LoginStatus user = CommonTask.Notification(loginObject);
                if (user.statusCode.Equals("200") && user.statusDescription.Equals("Authentication was successful"))
                {
                    userdetails.email = username;
                    userdetails.password = password;
                    userdetails.School = CommonTask.GetSchool(loginObject);
                    userdetails.full_names = CommonTask.GetUsername(loginObject);

                    try
                    {
                        var units_http_client = new HttpClient();
                        var units_postData = new List<KeyValuePair<string, string>>();
                        units_postData.Add(new KeyValuePair<string, string>("email", username));
                        units_postData.Add(new KeyValuePair<string, string>("password", password));
                        var units_formContent = new FormUrlEncodedContent(units_postData);
                        var courseresponse = await units_http_client.PostAsync("http://brainshare.ug/liveapis/course_units.json", units_formContent);
                        var coursesresult = await courseresponse.Content.ReadAsStreamAsync();
                        var coursestreamReader = new System.IO.StreamReader(coursesresult);
                        var courseresponseContent = coursestreamReader.ReadToEnd().Trim().ToString();
                        var subjects = JsonArray.Parse(courseresponseContent);


                        List<SubjectObservable> courses = new List<SubjectObservable>();
                        List<SubjectObservable> newcourses = new List<SubjectObservable>();
                        List<int> IDs = CommonTask.SubjectIds(subjects);
                        List<int> NewSubjectIds = CommonTask.newIds(oldIDs, IDs);

                        char[] delimiter = { '.' };
                        var db = new SQLite.SQLiteConnection(Constants.dbPath);
                        var query = (db.Table<User>().Where(c => c.e_mail == username)).Single();
                        string[] SplitSubjectId = query.subjects.Split(delimiter);
                        List<string> SubjectIdList = SplitSubjectId.ToList();
                        List<int> subjectids = CommonTask.SubjectIdsConvert(SubjectIdList);
                        List<int> removedIds = CommonTask.newIds(IDs, subjectids);
                        List<SubjectObservable> CurrentSubjects = new List<SubjectObservable>();
                        List<int> remainedIDs = new List<int>();

                        if (removedIds != null)
                        {
                            remainedIDs = CommonTask.newIds(removedIds, oldIDs);
                        }
                        else
                        {
                            remainedIDs = oldIDs;
                        }
                        if (remainedIDs != null)
                        {
                            foreach (var id in remainedIDs)
                            {
                                SubjectObservable subjectremoved = CommonTask.GetSubject(id);
                                CurrentSubjects.Add(subjectremoved);
                            }
                            InstalledSubjects = CurrentSubjects;
                        }

                        if (remainedIDs == null)
                        {
                            InstalledSubjects = null;
                        }

                        if (NewSubjectIds != null)
                        {
                            //  LoadingMsg.Text = "Found Updates";
                            //  LoadingMsg.Visibility = Visibility.Visible;

                            foreach (var id in NewSubjectIds)
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
                                CurrentSubjects.Add(subject);
                                courses.Add(subject);
                                newcourses.Add(subject);
                            }


                            //if (remainedIDs != null)
                            //{

                            if (remainedIDs == null)
                            {
                                NewSubjectIds = null;
                            }
                            if (remainedIDs != null)
                            {
                                InstalledSubjects.AddRange(courses);
                                NewSubjectIds = CommonTask.newIds(IDs, remainedIDs);
                            }



                            if (NewSubjectIds != null)
                            {
                                List<int> UpdateIds = CommonTask.oldIds(IDs, remainedIDs);
                                List<SubjectObservable> oldcourses = new List<SubjectObservable>();
                                foreach (var id in UpdateIds)
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
                                    oldcourses.Add(subject);
                                    courses.Add(subject);//check here
                                }
                                List<SubjectObservable> updateable = new List<SubjectObservable>();
                                if (remainedIDs == null)
                                {
                                    updateable = null;
                                }
                                else
                                {
                                    updateable = CommonTask.UpdateableSubjects(InstalledSubjects, oldcourses);
                                }

                                if (updateable == null)
                                {
                                    List<SubjectObservable> updatedTopics = new List<SubjectObservable>();
                                    if (remainedIDs == null)
                                    {
                                        userdetails.subjects = CurrentSubjects;
                                    }
                                    else
                                    {
                                        userdetails.subjects = InstalledSubjects;
                                        updatedTopics = CommonTask.UpdateableSubjectsTopics(InstalledSubjects, oldcourses);
                                        if (updatedTopics != null)
                                        {
                                            CommonTask.InsertSubjectsUpdateAsync(updatedTopics);
                                        }

                                    }

                                    LibraryObservable newContentLibrary = CommonTask.CompareLibraries(Old_Library, Current_Library);
                                    List<Library_CategoryObservable> updatedOldContentLibrary = CommonTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);


                                    //Have this script everywhere you see the above decleration

                                   // List<Library_CategoryObservable> removeOldContentLibrary = CommonTask.Category_Update_Removal(Old_Library.categories, newContentLibrary.categories);

                                    //Then modify the method below to contain it's value
                                    //Can't do that cause i only have 2 files and so i can't see everywhere they are
                                    //Methods are ready in CommonTask only need to bne called



                                    if (newContentLibrary == null && updatedOldContentLibrary != null)
                                    {
                                        UserUpdater(userdetails, courses, null, currentUser, null, updatedOldContentLibrary);
                                    }
                                    else if (newContentLibrary == null && updatedOldContentLibrary == null)
                                    {
                                        UserUpdater(userdetails, courses, null, currentUser, null, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary == null)
                                    {
                                        UserUpdater(userdetails, courses, null, currentUser, newContentLibrary, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary != null)
                                    {
                                        UserUpdater(userdetails, courses, null, currentUser, newContentLibrary, updatedOldContentLibrary);
                                    }
                                }
                                else
                                {
                                    List<SubjectObservable> updatedTopics = new List<SubjectObservable>();
                                    userdetails.subjects = InstalledSubjects;
                                    updatedTopics = CommonTask.UpdateableSubjectsTopics(InstalledSubjects, oldcourses);
                                    if (updatedTopics != null)
                                    {
                                        CommonTask.InsertSubjectsUpdateAsync(updatedTopics);
                                    }
                                    LibraryObservable newContentLibrary = CommonTask.CompareLibraries(Old_Library, Current_Library);
                                    List<Library_CategoryObservable> updatedOldContentLibrary = CommonTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
                                    if (newContentLibrary == null && updatedOldContentLibrary != null)
                                    {
                                        UserUpdater(userdetails, courses, updateable, currentUser, null, updatedOldContentLibrary);
                                    }
                                    else if (newContentLibrary == null && updatedOldContentLibrary == null)
                                    {
                                        UserUpdater(userdetails, courses, updateable, currentUser, null, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary == null)
                                    {
                                        UserUpdater(userdetails, courses, updateable, currentUser, newContentLibrary, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary != null)
                                    {
                                        UserUpdater(userdetails, courses, updateable, currentUser, newContentLibrary, updatedOldContentLibrary);
                                    }
                                }
                            }
                            else
                            {
                                if (remainedIDs == null)
                                {
                                    userdetails.subjects = CurrentSubjects;
                                    LibraryObservable newContentLibrary = CommonTask.CompareLibraries(Old_Library, Current_Library);
                                    List<Library_CategoryObservable> updatedOldContentLibrary = CommonTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
                                    if (newContentLibrary == null && updatedOldContentLibrary != null)
                                    {
                                        UserUpdater(userdetails, newcourses, null, currentUser, null, updatedOldContentLibrary);
                                    }
                                    else if (newContentLibrary == null && updatedOldContentLibrary == null)
                                    {
                                        UserUpdater(userdetails, newcourses, null, currentUser, null, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary == null)
                                    {
                                        UserUpdater(userdetails, newcourses, null, currentUser, newContentLibrary, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary != null)
                                    {
                                        UserUpdater(userdetails, newcourses, null, currentUser, newContentLibrary, updatedOldContentLibrary);
                                    }
                                }
                                else
                                {
                                    List<int> UpdateIds = CommonTask.oldIds(remainedIDs, IDs);
                                    List<SubjectObservable> oldcourses = new List<SubjectObservable>();
                                    foreach (var id in UpdateIds)
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
                                        oldcourses.Add(subject);
                                    }
                                    List<SubjectObservable> updateable = CommonTask.UpdateableSubjects(InstalledSubjects, oldcourses);

                                    if (updateable == null)
                                    {
                                        userdetails.subjects = InstalledSubjects;
                                        List<SubjectObservable> updatedTopics = new List<SubjectObservable>();
                                        updatedTopics = CommonTask.UpdateableSubjectsTopics(InstalledSubjects, oldcourses);
                                        if (updatedTopics != null)
                                        {
                                            CommonTask.InsertSubjectsUpdateAsync(updatedTopics);
                                        }
                                        LibraryObservable newContentLibrary = CommonTask.CompareLibraries(Old_Library, Current_Library);
                                        List<Library_CategoryObservable> updatedOldContentLibrary = CommonTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
                                        if (newContentLibrary == null && updatedOldContentLibrary != null)
                                        {
                                            UserUpdater(userdetails, newcourses, null, currentUser, null, updatedOldContentLibrary);
                                        }
                                        else if (newContentLibrary == null && updatedOldContentLibrary == null)
                                        {
                                            UserUpdater(userdetails, newcourses, null, currentUser, null, null);
                                        }
                                        else if (newContentLibrary != null && updatedOldContentLibrary == null)
                                        {
                                            UserUpdater(userdetails, newcourses, null, currentUser, newContentLibrary, null);
                                        }
                                        else if (newContentLibrary != null && updatedOldContentLibrary != null)
                                        {
                                            UserUpdater(userdetails, newcourses, null, currentUser, newContentLibrary, updatedOldContentLibrary);
                                        }
                                    }
                                    else
                                    {
                                        userdetails.subjects = InstalledSubjects;
                                        List<SubjectObservable> updatedTopics = new List<SubjectObservable>();
                                        updatedTopics = CommonTask.UpdateableSubjectsTopics(InstalledSubjects, oldcourses);
                                        if (updatedTopics != null)
                                        {
                                            CommonTask.InsertSubjectsUpdateAsync(updatedTopics);
                                        }
                                        LibraryObservable newContentLibrary = CommonTask.CompareLibraries(Old_Library, Current_Library);
                                        List<Library_CategoryObservable> updatedOldContentLibrary = CommonTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
                                        if (newContentLibrary == null && updatedOldContentLibrary != null)
                                        {
                                            UserUpdater(userdetails, newcourses, updateable, currentUser, null, updatedOldContentLibrary);
                                        }
                                        else if (newContentLibrary == null && updatedOldContentLibrary == null)
                                        {
                                            UserUpdater(userdetails, newcourses, updateable, currentUser, null, null);
                                        }
                                        else if (newContentLibrary != null && updatedOldContentLibrary == null)
                                        {
                                            UserUpdater(userdetails, newcourses, updateable, currentUser, newContentLibrary, null);
                                        }
                                        else if (newContentLibrary != null && updatedOldContentLibrary != null)
                                        {
                                            UserUpdater(userdetails, newcourses, updateable, currentUser, newContentLibrary, updatedOldContentLibrary);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (remainedIDs == null)
                            {
                                userdetails.subjects = null;
                                LibraryObservable newContentLibrary = CommonTask.CompareLibraries(Old_Library, Current_Library);
                                List<Library_CategoryObservable> updatedOldContentLibrary = CommonTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
                                if (newContentLibrary == null && updatedOldContentLibrary != null)
                                {
                                    UserUpdater(userdetails, null, null, currentUser, null, updatedOldContentLibrary);
                                }
                                else if (newContentLibrary == null && updatedOldContentLibrary == null)
                                {
                                    UserUpdater(userdetails, null, null, currentUser, null, null);
                                }
                                else if (newContentLibrary != null && updatedOldContentLibrary == null)
                                {
                                    UserUpdater(userdetails, null, null, currentUser, newContentLibrary, null);
                                }
                                else if (newContentLibrary != null && updatedOldContentLibrary != null)
                                {
                                    UserUpdater(userdetails, null, null, currentUser, newContentLibrary, updatedOldContentLibrary);
                                }
                            }
                            else
                            {
                                List<int> UpdateIds = CommonTask.oldIds(remainedIDs, IDs);
                                List<SubjectObservable> oldcourses = new List<SubjectObservable>();

                                foreach (var id in UpdateIds)
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
                                    oldcourses.Add(subject);
                                    courses.Add(subject);
                                }
                                List<SubjectObservable> updateable = CommonTask.UpdateableSubjects(InstalledSubjects, oldcourses);
                                if (updateable == null)
                                {
                                    userdetails.subjects = InstalledSubjects;
                                    List<SubjectObservable> updatedTopics = CommonTask.UpdateableSubjectsTopics(InstalledSubjects, oldcourses);
                                    if (updatedTopics != null)
                                    {
                                        CommonTask.InsertSubjectsUpdateAsync(updatedTopics);
                                    }
                                    LibraryObservable newContentLibrary = CommonTask.CompareLibraries(Old_Library, Current_Library);
                                    List<Library_CategoryObservable> updatedOldContentLibrary = CommonTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
                                    if (newContentLibrary == null && updatedOldContentLibrary != null)
                                    {
                                        UserUpdater(userdetails, null, null, currentUser, null, updatedOldContentLibrary);
                                    }
                                    else if (newContentLibrary == null && updatedOldContentLibrary == null)
                                    {
                                        UserUpdater(userdetails, null, null, currentUser, null, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary == null)
                                    {
                                        UserUpdater(userdetails, null, null, currentUser, newContentLibrary, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary != null)
                                    {
                                        UserUpdater(userdetails, null, null, currentUser, newContentLibrary, updatedOldContentLibrary);
                                    }
                                }
                                else
                                {
                                    //  LoadingMsg.Text = "Updating ....";
                                    //  LoadingMsg.Visibility = Visibility.Visible;
                                    userdetails.subjects = InstalledSubjects;
                                    List<SubjectObservable> updatedTopics = CommonTask.UpdateableSubjectsTopics(InstalledSubjects, oldcourses);
                                    if (updatedTopics != null)
                                    {
                                        CommonTask.InsertSubjectsUpdateAsync(updatedTopics);
                                    }
                                    LibraryObservable newContentLibrary = CommonTask.CompareLibraries(Old_Library, Current_Library);
                                    List<Library_CategoryObservable> updatedOldContentLibrary = CommonTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
                                    if (newContentLibrary == null && updatedOldContentLibrary != null)
                                    {
                                        UserUpdater(userdetails, null, updateable, currentUser, null, updatedOldContentLibrary);
                                    }
                                    else if (newContentLibrary == null && updatedOldContentLibrary == null)
                                    {
                                        UserUpdater(userdetails, null, updateable, currentUser, null, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary == null)
                                    {
                                        UserUpdater(userdetails, null, updateable, currentUser, newContentLibrary, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary != null)
                                    {
                                        UserUpdater(userdetails, null, updateable, currentUser, newContentLibrary, updatedOldContentLibrary);
                                    }
                                }
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        string error = ex.ToString();
                        currentUser.update_status = Constants.finished_update;
                        pgBar.Visibility = Visibility.Collapsed;
                        //   LoadingMsg.Visibility = Visibility.Collapsed;
                        //var message = new MessageDialog(Message.Connection_Error /*error*/, Message.Connection_Error_Header).ShowAsync();
                        //Error Message not necessary
                    }
                }
                else
                {
                    var message = new MessageDialog(Message.Wrong_User_details, Message.Login_Header).ShowAsync();
                }
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                currentUser.update_status = Constants.finished_update;
                pgBar.Visibility = Visibility.Collapsed;
                //  LoadingMsg.Visibility = Visibility.Collapsed;
                //var message = new MessageDialog(Message.Connection_Error /*error*/, Message.Connection_Error_Header).ShowAsync();
                //Error Message not necessary on this page
            }
        }
        private async void UserUpdater(UserObservable user, List<SubjectObservable> newsubjects, List<SubjectObservable> updateableSubjects, UserObservable CurrentUser,
            LibraryObservable newlib, List<Library_CategoryObservable> updatedCategories)
        {
            await CommonTask.UpdateUserAsync(user);
            CommonTask.UpdateLibAsync(newlib); //Will be checked later // Could be awaitable
            if (updateableSubjects == null && newsubjects == null)
            {
            }
            else
            {
                if (updateableSubjects == null)
                {
                    CommonTask.InsertSubjectsAsync(newsubjects);
                }
                if (newsubjects == null)
                {
                    CommonTask.UpdateSubjectsAsync(updateableSubjects);
                }
                if (updateableSubjects != null && newsubjects != null)
                {
                    CommonTask.InsertSubjectsAsync(newsubjects);
                    CommonTask.UpdateSubjectsAsync(updateableSubjects);
                }
            }
            CurrentUser.update_status = Constants.finished_update;
            pgBar.Visibility = Visibility.Collapsed;
            //   LoadingMsg.Visibility = Visibility.Collapsed;
        }
        private void Subject_click(object sender, ItemClickEventArgs e)
        {
            try
            {
                var item = e.ClickedItem;
                SubjectObservable _subject = ((SubjectObservable)item);
                Frame.Navigate(typeof(SubjectPage), _subject);
            }
            catch
            {

            }
        }
        private void Library_Category_click(object sender, ItemClickEventArgs e)
        {
            try
            {
                var item = e.ClickedItem;
                Library_CategoryObservable lib_category = ((Library_CategoryObservable)item);
                Frame.Navigate(typeof(LibraryCategoryBooks), lib_category);
            }
            catch
            {

            }
        }
        private void Log_out(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }
        private async Task InitializeDatabase()
        {
            DbConnection oDbConnection = new DbConnection();
            await oDbConnection.InitializeDatabase();
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
            try
            {


                navigationHelper.OnNavigatedTo(e);
            }
            catch
            {

            }
            //  var user = e.Parameter as UserCredential;
            //StudentPageViewModel vm = new StudentPageViewModel(user);
            //this.DataContext = vm;


            //string user = (string)e.NavigationParameter;
            //  StudentPageViewModel vm = new StudentPageViewModel("ken");
            // this.DataContext = vm;


        }

        private void logout_btn_click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {


                Frame.Navigate(typeof(LoginPage));
                //var item = e.ClickedItem;
                //FolderObservable _folder = ((FolderObservable)item);
                //this.Frame.Navigate(typeof(LibraryCategories));
            }
            catch
            {

            }
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            try
            {
                navigationHelper.OnNavigatedFrom(e);
                // string user = (string)e.NavigationParameter;
                //StudentPageViewModel vm = new StudentPageViewModel("ken");
                //this.DataContext = vm;

            }
            catch
            {

            }
        }

        #endregion
    }
}
