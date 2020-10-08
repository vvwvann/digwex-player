using System;
using System.IO;
using System.Net;

namespace DigWex.Model
{
    public class ResponseMessage
    {
        public ResponseMessage()
        {
            Content = "";
            Cookies = "";
            Location = "";
        }

        public string Content { get; private set; }

        public string Cookies { get; private set; }

        public HttpStatusCode StatusCode { get; private set; }

        public string Location { get; private set; }

        public void SetResponse(HttpWebResponse response)
        {
            if (response == null)
                return;
            StatusCode = response.StatusCode;
            Location = response.Headers?["location"];
            Cookies = response.Headers?["set-cookie"];
            SetContent(response);
        }

        public void SetStatusCode(HttpStatusCode code)
        {
            StatusCode = code;
        }

        private void SetContent(HttpWebResponse response)
        {
            try
            {
                Stream stream = response.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                Content = sr.ReadToEnd();
                stream.Close();
                sr.Close();
            }
            catch { }
        }
    }
}
