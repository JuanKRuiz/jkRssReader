using RSSJuanK4Blog.Common;
using System;

namespace RSSJuanK4Blog.Model
{
    /// <summary>Article from an RSS Feed</summary>
    public class Article : BindableBase
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                SetProperty(ref _title, value);
            }
        }


        private string _summary;
        public string Summary
        {
            get { return _summary; }
            set
            {
                SetProperty(ref _summary, value);
            }
        }

        private string _content;
        public string Content
        {
            get { return _content; }
            set
            {
                SetProperty(ref _content, value);
            }
        }

        private Uri _imgUri;
        public Uri ImgUri
        {
            get { return _imgUri; }
            set
            {
                SetProperty(ref _imgUri, value);
            }
        }
    }
}
