namespace NP.Lti13Platform
{
    public class ContentItem
    {
        public required string Type { get; set; }

        // type = link
        public string? Url { get; set; } // *
        public string? Title { get; set; }
        public string? Text { get; set; }
        public ContentItemIcon? Icon { get; set; }
        public ContentItemThumbnail? Thumbnail { get; set; }
        public ContentItemWindow? Window { get; set; }
        public ContentItemLInkIframe? Iframe { get; set; }
        public ContentItemEmbed? Embed { get; set; }

        // type = ltiresourcelink
        // url
        // title
        // text
        // icon
        // thumbnail
        // window
        //public ContentItemResourceLinkIframe? Iframe { get; set; }
        public IDictionary<string, string>? Custom { get; set; }
        public ContentItemLineItem? LineItem { get; set; }
        public ContentItemAvailable? Available { get; set; }
        public ContentItemSubmission? Submission { get; set; }

        // type = file
        // url *
        // title
        // text
        // icon
        // thumbnail
        public DateTime? ExpiresAt { get; set; }

        // type = html
        public string? Html { get; set; } // *
        // title
        // text

        // type = image
        // url *
        // icon
        // thumbnail
        public int? Width { get; set; }
        public int? Height { get; set; }
    }

    public class ContentItemSubmission
    {
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
    }

    public class ContentItemAvailable
    {
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
    }

    public class ContentItemLineItem
    {
        public string? Label { get; set; }
        public decimal ScoreMaximum { get; set; }
        public string? ResourceId { get; set; }
        public string? Tag { get; set; }
        public bool? GradesReleased { get; set; }
    }

    public class ContentItemResourceLinkIframe
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class ContentItemLInkIframe
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Src { get; set; }
    }

    public class ContentItemWindow
    {
        public string TargetName { get; set; }
        public string WindowFeatures { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class ContentItemEmbed
    {
        public string Html { get; set; }
    }

    public class ContentItemThumbnail
    {
        public string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class ContentItemIcon
    {
        public string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

}