namespace SEToolbox.Controls
{
    using System;
    using System.Net;

    // Actually it's a component, but meh.

    class MyWebClient : WebClient
    {
        Uri _responseUri;

        public Uri ResponseUri
        {
            get { return _responseUri; }
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = null;
            try
            {
                response = base.GetWebResponse(request);
            }
            catch { }

            _responseUri = (response != null) ? response.ResponseUri : null;

            return response;
        }
    }
}
