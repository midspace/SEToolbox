namespace SEToolbox.Controls
{
    using System;
    using System.Net;

    // Actually it's a component, but meh.

    class MyWebClient : WebClient
    {
        public Uri ResponseUri { get; private set; }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response;
            try
            {
                response = base.GetWebResponse(request);
            }
            catch
            {
                response = null;
            }

            ResponseUri = (response != null) ? response.ResponseUri : null;

            return response;
        }
    }
}
