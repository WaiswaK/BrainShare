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
using Windows.Data.Json;
using BrainShare.Core;
using Windows.Storage;



// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace BrainShare.Views
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class StudentView : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        ErrorLogTask Logfile = new ErrorLogTask();

        //Settings
        private const string _noteskey = "Notes";
        private const string _libkey = "Library";
        private bool notes_on = true;
        private bool library_on = true;


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


        public StudentView()
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
            var user = e.NavigationParameter as UserModel;
            UserModel initial = user;
            List<SubjectModel> all_subjects = new List<SubjectModel>();

            //Notes and Notes Module Settings Check
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(_noteskey))
                notes_on = !(bool)ApplicationData.Current.LocalSettings.Values[_noteskey];
            if (notes_on)
            {
                char[] delimiter = { '.' };
                List<SubjectModel> subjectsNew = new List<SubjectModel>();
                var db = new SQLite.SQLiteConnection(Constant.dbPath);
                var query = (db.Table<User>().Where(c => c.e_mail == user.email)).Single();

                string[] SplitSubjectId = query.subjects.Split(delimiter);
                List<string> SubjectIdList = SplitSubjectId.ToList();
                List<int> subjectids = ModelTask.SubjectIdsConvert(SubjectIdList);
                foreach (var id in subjectids)
                {
                    SubjectModel subject = DBRetrievalTask.GetSubject(id);
                    subjectsNew.Add(subject);
                }
                user.subjects = subjectsNew;
                all_subjects = user.subjects;
                user.subjects = ModelTask.DisplayableSubjects(user.subjects);
            }
            else
            {
                user.subjects = null;
            }

            //Library and Library Module Settings Check                     
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(_libkey))
                library_on = !(bool)ApplicationData.Current.LocalSettings.Values[_libkey];
            if (library_on)
            {
                LibraryModel lib = DBRetrievalTask.GetLibrary(user.School.SchoolId);
                user.Library = lib;
            }
            else
            {
                LibraryModel library = new LibraryModel();
                user.Library = library;
            }

            StudentViewModel vm = new StudentViewModel(user);
            DataContext = vm;
            if (user.update_status == Constant.finished_update)
            {
                if (CommonTask.IsInternetConnectionAvailable())
                {
                    UpdateUser(initial.email, initial.password, DBRetrievalTask.SubjectIdsForUser(initial.email), all_subjects, user);
                    if (user.NotesImagesDownloading == false)
                    {
                        user.NotesImagesDownloading = true;
                        NotesTask.GetNotesImagesSubjectsAsync(all_subjects);
                    }
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
        private async void UpdateUser(string username, string password, List<int> oldIDs, List<SubjectModel> InstalledSubjects, UserModel currentUser)
        {
            UserModel userdetails = new UserModel();
            List<SubjectModel> final = new List<SubjectModel>();
            LibraryModel Current_Library = new LibraryModel();
            LibraryModel Old_Library = DBRetrievalTask.GetLibrary(currentUser.School.SchoolId);
            currentUser.update_status = Constant.updating;
            pgBar.Visibility = Visibility.Visible;
            try
            {
                Current_Library = await JSONTask.Current_Library(username, password, userdetails.School.SchoolId);
            }
            catch
            {
            }
            try
            {
                JsonObject loginObject = await JSONTask.LoginJsonObject(username, password);
                LoginStatus user = JSONTask.Notification(loginObject);
                if (user.statusCode.Equals("200") && user.statusDescription.Equals("Authentication was successful"))
                {
                    userdetails.email = username;
                    userdetails.password = password;
                    userdetails.School = JSONTask.GetSchool(loginObject);
                    userdetails.full_names = JSONTask.GetUsername(loginObject);
                    try
                    {
                        JsonArray subjects = await JSONTask.SubjectsJsonArray(username, password);
                        List<SubjectModel> courses = new List<SubjectModel>();
                        List<SubjectModel> newcourses = new List<SubjectModel>();
                        List<int> IDs = JSONTask.SubjectIds(subjects);
                        List<int> NewSubjectIds = ModelTask.newIds(oldIDs, IDs);

                        char[] delimiter = { '.' };
                        var db = new SQLite.SQLiteConnection(Constant.dbPath);
                        var query = (db.Table<User>().Where(c => c.e_mail == username)).Single();
                        string[] SplitSubjectId = query.subjects.Split(delimiter);
                        List<string> SubjectIdList = SplitSubjectId.ToList();
                        List<int> subjectids = ModelTask.SubjectIdsConvert(SubjectIdList);
                        List<int> removedIds = ModelTask.newIds(IDs, subjectids);
                        List<SubjectModel> CurrentSubjects = new List<SubjectModel>();
                        List<int> remainedIDs = new List<int>();

                        if (removedIds != null)
                        {
                            remainedIDs = ModelTask.newIds(removedIds, oldIDs);
                        }
                        else
                        {
                            remainedIDs = oldIDs;
                        }
                        if (remainedIDs != null)
                        {
                            foreach (var id in remainedIDs)
                            {
                                SubjectModel subjectremoved = DBRetrievalTask.GetSubject(id);
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
                            CurrentSubjects = await JSONTask.Get_New_Subjects(username, password, NewSubjectIds, subjects);
                            foreach (var sub in CurrentSubjects)
                            {
                                courses.Add(sub);
                                newcourses.Add(sub);
                            }

                            if (remainedIDs == null)
                            {
                                NewSubjectIds = null;
                            }
                            if (remainedIDs != null)
                            {
                                InstalledSubjects.AddRange(courses);
                                NewSubjectIds = ModelTask.newIds(IDs, remainedIDs);
                            }
                            if (NewSubjectIds != null)
                            {
                                List<SubjectModel> oldcourses = await JSONTask.Get_Subjects(username, password, remainedIDs, IDs, subjects);
                                foreach (var course in oldcourses)
                                {
                                    courses.Add(course);
                                }

                                List<SubjectModel> updateable = new List<SubjectModel>();
                                if (remainedIDs == null)
                                {
                                    updateable = null;
                                }
                                else
                                {
                                    updateable = ModelTask.UpdateableSubjects(InstalledSubjects, oldcourses);
                                }

                                if (updateable == null)
                                {
                                    List<SubjectModel> updatedTopics = new List<SubjectModel>();
                                    if (remainedIDs == null)
                                    {
                                        userdetails.subjects = CurrentSubjects;
                                    }
                                    else
                                    {
                                        userdetails.subjects = InstalledSubjects;
                                        updatedTopics = ModelTask.UpdateableSubjectsTopics(InstalledSubjects, oldcourses);
                                        if (updatedTopics != null)
                                        {
                                            DBInsertionTask.InsertSubjectsUpdateAsync(updatedTopics);
                                        }

                                    }

                                    LibraryModel newContentLibrary = ModelTask.CompareLibraries(Old_Library, Current_Library);
                                    List<LibCategoryModel> updatedOldContentLibrary = ModelTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);

                                    if (newContentLibrary == null && updatedOldContentLibrary != null)
                                    {
                                        ModelTask.UserUpdater(userdetails, courses, null, currentUser, null, updatedOldContentLibrary);
                                    }
                                    else if (newContentLibrary == null && updatedOldContentLibrary == null)
                                    {
                                        ModelTask.UserUpdater(userdetails, courses, null, currentUser, null, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary == null)
                                    {
                                        ModelTask.UserUpdater(userdetails, courses, null, currentUser, newContentLibrary, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary != null)
                                    {
                                        ModelTask.UserUpdater(userdetails, courses, null, currentUser, newContentLibrary, updatedOldContentLibrary);
                                    }
                                }
                                else
                                {
                                    List<SubjectModel> updatedTopics = new List<SubjectModel>();
                                    userdetails.subjects = InstalledSubjects;
                                    updatedTopics = ModelTask.UpdateableSubjectsTopics(InstalledSubjects, oldcourses);
                                    if (updatedTopics != null)
                                    {
                                        DBInsertionTask.InsertSubjectsUpdateAsync(updatedTopics);
                                    }
                                    LibraryModel newContentLibrary = ModelTask.CompareLibraries(Old_Library, Current_Library);
                                    List<LibCategoryModel> updatedOldContentLibrary = ModelTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
                                    if (newContentLibrary == null && updatedOldContentLibrary != null)
                                    {
                                        ModelTask.UserUpdater(userdetails, courses, updateable, currentUser, null, updatedOldContentLibrary);
                                    }
                                    else if (newContentLibrary == null && updatedOldContentLibrary == null)
                                    {
                                        ModelTask.UserUpdater(userdetails, courses, updateable, currentUser, null, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary == null)
                                    {
                                        ModelTask.UserUpdater(userdetails, courses, updateable, currentUser, newContentLibrary, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary != null)
                                    {
                                        ModelTask.UserUpdater(userdetails, courses, updateable, currentUser, newContentLibrary, updatedOldContentLibrary);
                                    }
                                }
                            }
                            else
                            {
                                if (remainedIDs == null)
                                {
                                    userdetails.subjects = CurrentSubjects;
                                    LibraryModel newContentLibrary = ModelTask.CompareLibraries(Old_Library, Current_Library);
                                    List<LibCategoryModel> updatedOldContentLibrary = ModelTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
                                    if (newContentLibrary == null && updatedOldContentLibrary != null)
                                    {
                                        ModelTask.UserUpdater(userdetails, newcourses, null, currentUser, null, updatedOldContentLibrary);
                                    }
                                    else if (newContentLibrary == null && updatedOldContentLibrary == null)
                                    {
                                        ModelTask.UserUpdater(userdetails, newcourses, null, currentUser, null, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary == null)
                                    {
                                        ModelTask.UserUpdater(userdetails, newcourses, null, currentUser, newContentLibrary, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary != null)
                                    {
                                        ModelTask.UserUpdater(userdetails, newcourses, null, currentUser, newContentLibrary, updatedOldContentLibrary);
                                    }
                                }
                                else
                                {
                                    List<SubjectModel> oldcourses = await JSONTask.Get_Subjects(username, password, remainedIDs, IDs, subjects);
                                    List<SubjectModel> updateable = ModelTask.UpdateableSubjects(InstalledSubjects, oldcourses);

                                    if (updateable == null)
                                    {
                                        userdetails.subjects = InstalledSubjects;
                                        List<SubjectModel> updatedTopics = new List<SubjectModel>();
                                        updatedTopics = ModelTask.UpdateableSubjectsTopics(InstalledSubjects, oldcourses);
                                        if (updatedTopics != null)
                                        {
                                            DBInsertionTask.InsertSubjectsUpdateAsync(updatedTopics);
                                        }
                                        LibraryModel newContentLibrary = ModelTask.CompareLibraries(Old_Library, Current_Library);
                                        List<LibCategoryModel> updatedOldContentLibrary = ModelTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
                                        if (newContentLibrary == null && updatedOldContentLibrary != null)
                                        {
                                            ModelTask.UserUpdater(userdetails, newcourses, null, currentUser, null, updatedOldContentLibrary);
                                        }
                                        else if (newContentLibrary == null && updatedOldContentLibrary == null)
                                        {
                                            ModelTask.UserUpdater(userdetails, newcourses, null, currentUser, null, null);
                                        }
                                        else if (newContentLibrary != null && updatedOldContentLibrary == null)
                                        {
                                            ModelTask.UserUpdater(userdetails, newcourses, null, currentUser, newContentLibrary, null);
                                        }
                                        else if (newContentLibrary != null && updatedOldContentLibrary != null)
                                        {
                                            ModelTask.UserUpdater(userdetails, newcourses, null, currentUser, newContentLibrary, updatedOldContentLibrary);
                                        }
                                    }
                                    else
                                    {
                                        userdetails.subjects = InstalledSubjects;
                                        List<SubjectModel> updatedTopics = new List<SubjectModel>();
                                        updatedTopics = ModelTask.UpdateableSubjectsTopics(InstalledSubjects, oldcourses);
                                        if (updatedTopics != null)
                                        {
                                            DBInsertionTask.InsertSubjectsUpdateAsync(updatedTopics);
                                        }
                                        LibraryModel newContentLibrary = ModelTask.CompareLibraries(Old_Library, Current_Library);
                                        List<LibCategoryModel> updatedOldContentLibrary = ModelTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
                                        if (newContentLibrary == null && updatedOldContentLibrary != null)
                                        {
                                            ModelTask.UserUpdater(userdetails, newcourses, updateable, currentUser, null, updatedOldContentLibrary);
                                        }
                                        else if (newContentLibrary == null && updatedOldContentLibrary == null)
                                        {
                                            ModelTask.UserUpdater(userdetails, newcourses, updateable, currentUser, null, null);
                                           
                                        }
                                        else if (newContentLibrary != null && updatedOldContentLibrary == null)
                                        {
                                            ModelTask.UserUpdater(userdetails, newcourses, updateable, currentUser, newContentLibrary, null);
                                        }
                                        else if (newContentLibrary != null && updatedOldContentLibrary != null)
                                        {
                                            ModelTask.UserUpdater(userdetails, newcourses, updateable, currentUser, newContentLibrary, updatedOldContentLibrary);
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
                                LibraryModel newContentLibrary = ModelTask.CompareLibraries(Old_Library, Current_Library);
                                List<LibCategoryModel> updatedOldContentLibrary = ModelTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
                                if (newContentLibrary == null && updatedOldContentLibrary != null)
                                {
                                    ModelTask.UserUpdater(userdetails, null, null, currentUser, null, updatedOldContentLibrary);
                                }
                                else if (newContentLibrary == null && updatedOldContentLibrary == null)
                                {
                                    ModelTask.UserUpdater(userdetails, null, null, currentUser, null, null);
                                }
                                else if (newContentLibrary != null && updatedOldContentLibrary == null)
                                {
                                    ModelTask.UserUpdater(userdetails, null, null, currentUser, newContentLibrary, null);
                                }
                                else if (newContentLibrary != null && updatedOldContentLibrary != null)
                                {
                                    ModelTask.UserUpdater(userdetails, null, null, currentUser, newContentLibrary, updatedOldContentLibrary);
                                }
                            }
                            else
                            {
                                List<SubjectModel> oldcourses = await JSONTask.Get_Subjects(username, password, remainedIDs, IDs, subjects);
                                foreach (var course in oldcourses)
                                {
                                    courses.Add(course); 
                                }
                                List<SubjectModel> updateable = ModelTask.UpdateableSubjects(InstalledSubjects, oldcourses);
                                if (updateable == null)
                                {
                                    userdetails.subjects = InstalledSubjects;
                                    List<SubjectModel> updatedTopics = ModelTask.UpdateableSubjectsTopics(InstalledSubjects, oldcourses);
                                    if (updatedTopics != null)
                                    {
                                        DBInsertionTask.InsertSubjectsUpdateAsync(updatedTopics);
                                    }
                                    LibraryModel newContentLibrary = ModelTask.CompareLibraries(Old_Library, Current_Library);
                                    List<LibCategoryModel> updatedOldContentLibrary = ModelTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
                                    if (newContentLibrary == null && updatedOldContentLibrary != null)
                                    {
                                        ModelTask.UserUpdater(userdetails, null, null, currentUser, null, updatedOldContentLibrary);
                                    }
                                    else if (newContentLibrary == null && updatedOldContentLibrary == null)
                                    {
                                        ModelTask.UserUpdater(userdetails, null, null, currentUser, null, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary == null)
                                    {
                                        ModelTask.UserUpdater(userdetails, null, null, currentUser, newContentLibrary, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary != null)
                                    {
                                        ModelTask.UserUpdater(userdetails, null, null, currentUser, newContentLibrary, updatedOldContentLibrary);
                                    }
                                }
                                else
                                {
                                    userdetails.subjects = InstalledSubjects;
                                    List<SubjectModel> updatedTopics = ModelTask.UpdateableSubjectsTopics(InstalledSubjects, oldcourses);
                                    if (updatedTopics != null)
                                    {
                                        DBInsertionTask.InsertSubjectsUpdateAsync(updatedTopics);
                                    }
                                    LibraryModel newContentLibrary = ModelTask.CompareLibraries(Old_Library, Current_Library);
                                    List<LibCategoryModel> updatedOldContentLibrary = ModelTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
                                    if (newContentLibrary == null && updatedOldContentLibrary != null)
                                    {
                                        ModelTask.UserUpdater(userdetails, null, updateable, currentUser, null, updatedOldContentLibrary);
                                    }
                                    else if (newContentLibrary == null && updatedOldContentLibrary == null)
                                    {
                                        ModelTask.UserUpdater(userdetails, null, updateable, currentUser, null, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary == null)
                                    {
                                        ModelTask.UserUpdater(userdetails, null, updateable, currentUser, newContentLibrary, null);
                                    }
                                    else if (newContentLibrary != null && updatedOldContentLibrary != null)
                                    {
                                        ModelTask.UserUpdater(userdetails, null, updateable, currentUser, newContentLibrary, updatedOldContentLibrary);
                                    }
                                }
                            }
                        }
                        currentUser.update_status = Constant.finished_update;
                        pgBar.Visibility = Visibility.Collapsed;
                    }

                    catch (Exception ex)
                    {
                        Logfile.Error_details = ex.ToString();
                        Logfile.Error_title = "UpdateUser Method";
                        Logfile.Location = "StudentView";
                        ErrorLogTask.LogFileSaveAsync(Logfile);
                        currentUser.update_status = Constant.finished_update;
                        pgBar.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                   
                }
            }
            catch (Exception ex)
            {
                Logfile.Error_details = ex.ToString();
                Logfile.Error_title = "UpdateUser Method";
                Logfile.Location = "StudentView";
                ErrorLogTask.LogFileSaveAsync(Logfile);
                currentUser.update_status = Constant.finished_update;
                pgBar.Visibility = Visibility.Collapsed;
            }
        }    
        private void Subject_click(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem;
            SubjectModel _subject = ((SubjectModel)item);
            Frame.Navigate(typeof(SubjectView), _subject);
        }
        private void Library_Category_click(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem;
            LibCategoryModel lib_category = ((LibCategoryModel)item);
            Frame.Navigate(typeof(LibraryCategoryBooksView), lib_category);
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
        private void logout_btn_click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ErrorLogTask.UploadLogFile();
            Frame.Navigate(typeof(LoginView));
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }
        #endregion
    }
}
