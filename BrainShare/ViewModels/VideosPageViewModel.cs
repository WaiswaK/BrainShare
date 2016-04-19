using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using BrainShare.Models;
using BrainShare.Database;

namespace BrainShare.ViewModels
{
    class VideosPageViewModel
    {
       private List<VideoObservable> _videosList;
       public List<VideoObservable> VideosList
        {
            get { return _videosList; }
            set { _videosList= value; }
        }
       public VideosPageViewModel(CategoryObservable videos)
        {
            VideosList = videos.videos;
        }
    }
}
