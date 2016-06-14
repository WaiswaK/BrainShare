using System.Collections.Generic;
using BrainShare.Models;

namespace BrainShare.ViewModels
{
    class VideosViewModel
    {
       private List<VideoModel> _videosList;
       public List<VideoModel> VideosList
        {
            get { return _videosList; }
            set { _videosList= value; }
        }
       public VideosViewModel(CategoryModel videos)
        {
            VideosList = videos.videos;
        }
    }
}
