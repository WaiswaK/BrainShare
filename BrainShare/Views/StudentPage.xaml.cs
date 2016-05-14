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
using System.Net.Http;
using Windows.UI.Popups;
using Windows.Data.Json;
using BrainShare.Core;
using Windows.Storage;



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

            //Notes and Notes Module Settings Check
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(_noteskey))
                notes_on = !(bool)ApplicationData.Current.LocalSettings.Values[_noteskey];
            if (notes_on)
            {
                char[] delimiter = { '.' };
                List<SubjectObservable> subjectsNew = new List<SubjectObservable>();
                var db = new SQLite.SQLiteConnection(Constants.dbPath);
                var query = (db.Table<User>().Where(c => c.e_mail == user.email)).Single();

                string[] SplitSubjectId = query.subjects.Split(delimiter);
                List<string> SubjectIdList = SplitSubjectId.ToList();
                List<int> subjectids = ModelTask.SubjectIdsConvert(SubjectIdList);
                foreach (var id in subjectids)
                {
                    SubjectObservable subject = DatabaseOutputTask.GetSubject(id);
                    subjectsNew.Add(subject);
                }
                user.subjects = subjectsNew;
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
                LibraryObservable lib = DatabaseOutputTask.GetLibrary(user.School.SchoolId);
                user.Library = lib;
            }
            else
            {
                LibraryObservable library = new LibraryObservable();
                user.Library = library;
            }

            StudentPageViewModel vm = new StudentPageViewModel(user);
            DataContext = vm;
            if (user.update_status == Constants.finished_update)
            {
                if (CommonTask.IsInternetConnectionAvailable())
                {
                    UpdateUser(initial.email, initial.password, DatabaseOutputTask.SubjectIdsForUser(initial.email), user.subjects, user);
                    if (user.NotesImagesDownloading == false)
                    {
                        user.NotesImagesDownloading = true;
                        NotesTask.GetNotesImagesSubjectsAsync(user.subjects);
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
        private async void UpdateUser(string username, string password, List<int> oldIDs, List<SubjectObservable> InstalledSubjects, UserObservable currentUser)
        {
            UserObservable userdetails = new UserObservable();
            SubjectObservable subject = new SubjectObservable();
            List<SubjectObservable> final = new List<SubjectObservable>();
            LibraryObservable Current_Library = new LibraryObservable();
            LibraryObservable Old_Library = DatabaseOutputTask.GetLibrary(currentUser.School.SchoolId);
            currentUser.update_status = Constants.updating;
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
                        List<SubjectObservable> courses = new List<SubjectObservable>();
                        List<SubjectObservable> newcourses = new List<SubjectObservable>();
                        List<int> IDs = JSONTask.SubjectIds(subjects);
                        List<int> NewSubjectIds = ModelTask.newIds(oldIDs, IDs);

                        char[] delimiter = { '.' };
                        var db = new SQLite.SQLiteConnection(Constants.dbPath);
                        var query = (db.Table<User>().Where(c => c.e_mail == username)).Single();
                        string[] SplitSubjectId = query.subjects.Split(delimiter);
                        List<string> SubjectIdList = SplitSubjectId.ToList();
                        List<int> subjectids = ModelTask.SubjectIdsConvert(SubjectIdList);
                        List<int> removedIds = ModelTask.newIds(IDs, subjectids);
                        List<SubjectObservable> CurrentSubjects = new List<SubjectObservable>();
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
                                SubjectObservable subjectremoved = DatabaseOutputTask.GetSubject(id);
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
                                List<SubjectObservable> oldcourses = await JSONTask.Get_Subjects(username, password, remainedIDs, IDs, subjects);
                                foreach (var course in oldcourses)
                                {
                                    courses.Add(course);
                                }

                                List<SubjectObservable> updateable = new List<SubjectObservable>();
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
                                    List<SubjectObservable> updatedTopics = new List<SubjectObservable>();
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
                                            DatabaseInputTask.InsertSubjectsUpdateAsync(updatedTopics);
                                        }

                                    }

                                    LibraryObservable newContentLibrary = ModelTask.CompareLibraries(Old_Library, Current_Library);
                                    List<Library_CategoryObservable> updatedOldContentLibrary = ModelTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);


                                    //Have this script everywhere you see the above decleration

                                    // List<Library_CategoryObservable> removeOldContentLibrary = CommonTask.Category_Update_Removal(Old_Library.categories, newContentLibrary.categories);

                                    //Then modify the method below to contain it's value
                                    //Can't do that cause i only have 2 files and so i can't see everywhere they are
                                    //Methods are ready in CommonTask only need to bne called



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
                                    List<SubjectObservable> updatedTopics = new List<SubjectObservable>();
                                    userdetails.subjects = InstalledSubjects;
                                    updatedTopics = ModelTask.UpdateableSubjectsTopics(InstalledSubjects, oldcourses);
                                    if (updatedTopics != null)
                                    {
                                        DatabaseInputTask.InsertSubjectsUpdateAsync(updatedTopics);
                                    }
                                    LibraryObservable newContentLibrary = ModelTask.CompareLibraries(Old_Library, Current_Library);
                                    List<Library_CategoryObservable> updatedOldContentLibrary = ModelTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
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
                                    LibraryObservable newContentLibrary = ModelTask.CompareLibraries(Old_Library, Current_Library);
                                    List<Library_CategoryObservable> updatedOldContentLibrary = ModelTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
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
                                    List<SubjectObservable> oldcourses = await JSONTask.Get_Subjects(username, password, remainedIDs, IDs, subjects);
                                    List<SubjectObservable> updateable = ModelTask.UpdateableSubjects(InstalledSubjects, oldcourses);

                                    if (updateable == null)
                                    {
                                        userdetails.subjects = InstalledSubjects;
                                        List<SubjectObservable> updatedTopics = new List<SubjectObservable>();
                                        updatedTopics = ModelTask.UpdateableSubjectsTopics(InstalledSubjects, oldcourses);
                                        if (updatedTopics != null)
                                        {
                                            DatabaseInputTask.InsertSubjectsUpdateAsync(updatedTopics);
                                        }
                                        LibraryObservable newContentLibrary = ModelTask.CompareLibraries(Old_Library, Current_Library);
                                        List<Library_CategoryObservable> updatedOldContentLibrary = ModelTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
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
                                        List<SubjectObservable> updatedTopics = new List<SubjectObservable>();
                                        updatedTopics = ModelTask.UpdateableSubjectsTopics(InstalledSubjects, oldcourses);
                                        if (updatedTopics != null)
                                        {
                                            DatabaseInputTask.InsertSubjectsUpdateAsync(updatedTopics);
                                        }
                                        LibraryObservable newContentLibrary = ModelTask.CompareLibraries(Old_Library, Current_Library);
                                        List<Library_CategoryObservable> updatedOldContentLibrary = ModelTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
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
                                LibraryObservable newContentLibrary = ModelTask.CompareLibraries(Old_Library, Current_Library);
                                List<Library_CategoryObservable> updatedOldContentLibrary = ModelTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
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
                                List<SubjectObservable> oldcourses = await JSONTask.Get_Subjects(username, password, remainedIDs, IDs, subjects);
                                foreach (var course in oldcourses)
                                {
                                    courses.Add(course); 
                                }
                                List<SubjectObservable> updateable = ModelTask.UpdateableSubjects(InstalledSubjects, oldcourses);
                                if (updateable == null)
                                {
                                    userdetails.subjects = InstalledSubjects;
                                    List<SubjectObservable> updatedTopics = ModelTask.UpdateableSubjectsTopics(InstalledSubjects, oldcourses);
                                    if (updatedTopics != null)
                                    {
                                        DatabaseInputTask.InsertSubjectsUpdateAsync(updatedTopics);
                                    }
                                    LibraryObservable newContentLibrary = ModelTask.CompareLibraries(Old_Library, Current_Library);
                                    List<Library_CategoryObservable> updatedOldContentLibrary = ModelTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
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
                                    List<SubjectObservable> updatedTopics = ModelTask.UpdateableSubjectsTopics(InstalledSubjects, oldcourses);
                                    if (updatedTopics != null)
                                    {
                                        DatabaseInputTask.InsertSubjectsUpdateAsync(updatedTopics);
                                    }
                                    LibraryObservable newContentLibrary = ModelTask.CompareLibraries(Old_Library, Current_Library);
                                    List<Library_CategoryObservable> updatedOldContentLibrary = ModelTask.Categories_Update(Old_Library.categories, newContentLibrary.categories);
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
                        currentUser.update_status = Constants.finished_update;
                        pgBar.Visibility = Visibility.Collapsed;
                    }

                    catch (Exception ex)
                    {
                        Logfile.Error_details = ex.ToString();
                        Logfile.Error_title = "UpdateUser Method";
                        Logfile.Location = "StudentPage";
                        //await ErrorLogTask.LogFileSaveAsync(Logfile);
                        currentUser.update_status = Constants.finished_update;
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
                Logfile.Location = "StudentPage";
                //await ErrorLogTask.LogFileSaveAsync(Logfile);
                currentUser.update_status = Constants.finished_update;
                pgBar.Visibility = Visibility.Collapsed;
            }
        }
        
        private void Subject_click(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem;
            SubjectObservable _subject = ((SubjectObservable)item);
            Frame.Navigate(typeof(SubjectPage), _subject);
        }
        private void Library_Category_click(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem;
            Library_CategoryObservable lib_category = ((Library_CategoryObservable)item);
            Frame.Navigate(typeof(LibraryCategoryBooks), lib_category);

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
            Frame.Navigate(typeof(LoginPage));
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }
        #endregion
    }
}
