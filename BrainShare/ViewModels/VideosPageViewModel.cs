using System.Collections.Generic;
using BrainShare.Models;

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
