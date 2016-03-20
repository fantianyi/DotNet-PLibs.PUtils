using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace PLibs.PUtils
{
    public class HttpUtils
    {
        #region 私有变量
        private CookieContainer cc;
        private string contentType = "application/x-www-form-urlencoded";
        private string accept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-shockwave-flash, application/x-silverlight, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, application/x-silverlight-2-b1, */*";
        private string userAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022)";
        private NameValueCollection hearderAppend = new NameValueCollection();
        private Encoding encoding = Encoding.GetEncoding("utf-8");
        #endregion

        #region 属性
        /// <summary>
        /// Cookie容器
        /// </summary>
        public CookieContainer CookieContainer
        {
            get
            {
                return cc;
            }
        }

        /// <summary>
        /// 获取网页源码时使用的编码
        /// </summary>
        /// <value></value>
        public Encoding Encoding
        {
            get
            {
                return encoding;
            }
            set
            {
                encoding = value;
            }
        }
        

        public string ContentType
        {
            get
            {
                return contentType;
            }
            set
            {
                contentType = value;
            }
        }

        public NameValueCollection HeaderAppend
        {
            get
            {
                return hearderAppend;
            }
            set
            {
                hearderAppend = value;
            }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHelper"/> class.
        /// </summary>
        public HttpUtils()
        {
            cc = new CookieContainer();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHelper"/> class.
        /// </summary>
        /// <param name="cc">The cc.</param>
        public HttpUtils(CookieContainer cc)
        {
            this.cc = cc;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHelper"/> class.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="accept">The accept.</param>
        /// <param name="userAgent">The user agent.</param>
        public HttpUtils(string contentType, string accept, string userAgent)
        {
            this.contentType = contentType;
            this.accept = accept;
            this.userAgent = userAgent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHelper"/> class.
        /// </summary>
        /// <param name="cc">The cc.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="accept">The accept.</param>
        /// <param name="userAgent">The user agent.</param>
        public HttpUtils(CookieContainer cc, string contentType, string accept, string userAgent)
        {
            this.cc = cc;
            this.contentType = contentType;
            this.accept = accept;
            this.userAgent = userAgent;
        }
        #endregion
        

        #region Get
        public string Get(string url, int timeOut = 3000)
        {
            HttpWebRequest httpWebRequest = null;

            try
            {
                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                httpWebRequest.Timeout = timeOut;
                httpWebRequest.ServicePoint.Expect100Continue = false;
                httpWebRequest.CookieContainer = cc;
                httpWebRequest.ContentType = "application/json;charset=utf-8";
                httpWebRequest.Headers.Add(HeaderAppend);
                httpWebRequest.Referer = url;
                httpWebRequest.Accept = accept;
                httpWebRequest.UserAgent = userAgent;
                httpWebRequest.Method = "GET";

                HttpWebResponse httpWebResponse;
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, encoding);

                string html = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();

                return html;
            }
            catch
            {
                // 避免TCP出现大量CLOSE_WAIT状态的连接
                if (httpWebRequest != null) httpWebRequest.Abort();
                throw;
            }
        }
        #endregion

        #region PostForm 以Post方式提交 multipart/form-data 格式的数据
        /// <summary>
        /// 以Post方式提交 multipart/form-data 格式的数据
        /// 用于上传文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeOut">超时时间（s）</param>
        /// <param name="fileKeyName"></param>
        /// <param name="filePath"></param>
        /// <param name="stringDict"></param>
        /// <returns></returns>
        /// <remarks>
        /// Post数据格式
        /// Post提交数据的时候最重要就是把Key-Value的数据放到http请求流中，而HttpWebRequest没有提供一个属性之类的东西可以让我们自由添加Key-Value，因此就必须手工构造这个数据。
        /// 根据RFC 2045协议，一个Http Post的数据格式如下：
        /// 
        /// [html] view plaincopy
        /// Content-Type: multipart/form-data; boundary=AaB03x  
        ///   
        ///  --AaB03x  
        ///  Content-Disposition: form-data; name="submit-name"  
        ///   
        ///  Larry  
        ///  --AaB03x  
        ///  Content-Disposition: form-data; name="file"; filename="file1.dat"  
        ///  Content-Type: application/octet-stream  
        ///   
        ///  ... contents of file1.dat ...  
        ///  --AaB03x--  
        ///
        /// 详细解释如下：
        /// Content-Type: multipart/form-data; boundary=AaB03x
        /// 如上面所示，首先声明数据类型为multipart/form-data, 然后定义边界字符串AaB03x，这个边界字符串就是用来在下面来区分各个数据的，可以随便定义，但是最好是用破折号等数据中一般不会出现的字符。然后是换行符。
        /// 注意：Post中定义的换行符是\r\n
        /// 
        /// --AaB03x
        /// 这个是边界字符串，注意每一个边界符前面都需要加2个连字符“--”，然后跟上换行符。
        /// Content-Disposition: form-data; name="submit-name"
        /// 这里是Key-Value数据中字符串类型的数据。 submit-name 是这个Key-Value数据中的Key。当然也需要换行符。
        /// 
        /// Larry 
        /// 这个就是刚才Key-Value数据中的value。
        /// 
        /// --AaB03x 
        /// 边界符,表示数据结束。
        /// 
        /// Content-Disposition: form-data; name="file"; filename="file1.dat" 
        /// 这个代表另外一个数据，它的key是file，文件名是file1.dat。 注意：最后面没有分号了。
        /// 
        /// Content-Type: application/octet-stream 
        /// 这个标识文件类型。application/octet-stream表示二进制数据。
        /// 
        /// ... contents of file1.dat ... 
        /// 这个是文件内容。可以使二进制的数据。
        /// 
        /// --AaB03x-- 
        /// 数据结束后的分界符，注意因为这个后面没有数据了所以需要在后面追加一个“--”表示结束。
        /// </remarks>
        /// <seealso cref="http://www.w3.org/TR/html401/interact/forms.html#h-17.13.4." />
        /// <seealso cref="http://stackoverflow.com/questions/566462/upload-files-with-httpwebrequest-multipart-form-data" />
        public string PostForm(string url, int timeOut, string fileKeyName,
            string filePath, NameValueCollection stringDict)
        {
            var responseContent = string.Empty;
            var memStream = new MemoryStream();
            HttpWebRequest httpWebRequest = null;

            try
            {
                httpWebRequest = (HttpWebRequest) WebRequest.Create(url);
                // 头部边界符
                var boundary = "---------------" + DateTime.Now.Ticks.ToString("x");
                // 边界符
                var beginBoundary = Encoding.ASCII.GetBytes("--" + boundary + "\r\n");

                // 最后的结束符
                var endBoundary = Encoding.ASCII.GetBytes("--" + boundary + "--\r\n");

                // 设置属性
                httpWebRequest.Method = "POST";
                httpWebRequest.Proxy = null;
                httpWebRequest.Timeout = timeOut;
                httpWebRequest.CookieContainer = this.cc;
                httpWebRequest.Headers.Add(HeaderAppend);
                httpWebRequest.ContentType = "multipart/form-data; boundary=" + boundary;

                if (fileKeyName != string.Empty && filePath != string.Empty)
                {
                    var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    // 写入文件
                    const string filePartHeader =
                        "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" +
                        "Content-Type: application/octet-stream\r\n\r\n";
                    var header = string.Format(filePartHeader, "file", fileKeyName);
                    var headerbytes = Encoding.UTF8.GetBytes(header);

                    memStream.Write(beginBoundary, 0, beginBoundary.Length);
                    memStream.Write(headerbytes, 0, headerbytes.Length);

                    var buffer = new byte[1024];
                    int bytesRead; // =0

                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        memStream.Write(buffer, 0, bytesRead);
                    }
                    fileStream.Close();
                }

                // 写入字符串的Key
                var stringKeyHeader = "\r\n--" + boundary +
                                      "\r\nContent-Disposition: form-data; name=\"{0}\"" +
                                      "\r\n\r\n{1}\r\n";

                foreach (string key in stringDict.Keys)
                {
                    string formitem = string.Format(stringKeyHeader, key, stringDict[key]);
                    byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                    memStream.Write(formitembytes, 0, formitembytes.Length);
                }

                // 写入最后的结束边界符
                memStream.Write(endBoundary, 0, endBoundary.Length);

                httpWebRequest.ContentLength = memStream.Length;

                var requestStream = httpWebRequest.GetRequestStream();

                memStream.Position = 0;
                var tempBuffer = new byte[memStream.Length];
                memStream.Read(tempBuffer, 0, tempBuffer.Length);
                memStream.Close();

                requestStream.Write(tempBuffer, 0, tempBuffer.Length);
                requestStream.Close();

                var httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse();
                
                using (var httpStreamReader = new StreamReader(httpWebResponse.GetResponseStream(),
                    Encoding.UTF8))
                {
                    responseContent = httpStreamReader.ReadToEnd();
                }

                httpWebResponse.Close();
                httpWebRequest.Abort();
                return responseContent;
            }
            catch
            {
                // 避免TCP出现大量CLOSE_WAIT状态的连接
                if (httpWebRequest != null) httpWebRequest.Abort();
                throw;
            }
        }

        public string PostForm(string url, int timeOut, string fileKeyName,
            Stream fileStream, NameValueCollection stringDict)
        {
            var responseContent = string.Empty;
            var memStream = new MemoryStream();
            HttpWebRequest httpWebRequest = null;

            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                // 头部边界符
                var boundary = "---------------" + DateTime.Now.Ticks.ToString("x");
                // 边界符
                var beginBoundary = Encoding.ASCII.GetBytes("--" + boundary + "\r\n");

                // 最后的结束符
                var endBoundary = Encoding.ASCII.GetBytes("--" + boundary + "--\r\n");

                // 设置属性
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = timeOut;
                httpWebRequest.CookieContainer = this.cc;
                httpWebRequest.Headers.Add(HeaderAppend);
                httpWebRequest.ContentType = "multipart/form-data; boundary=" + boundary;

                if (fileKeyName != string.Empty && fileStream != null)
                {
                    // 写入文件
                    const string filePartHeader =
                        "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" +
                        "Content-Type: application/octet-stream\r\n\r\n";
                    var header = string.Format(filePartHeader, "file", fileKeyName);
                    var headerbytes = Encoding.UTF8.GetBytes(header);

                    memStream.Write(beginBoundary, 0, beginBoundary.Length);
                    memStream.Write(headerbytes, 0, headerbytes.Length);

                    var buffer = new byte[1024];
                    int bytesRead; // =0

                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        memStream.Write(buffer, 0, bytesRead);
                    }
                    fileStream.Close();
                }

                // 写入字符串的Key
                var stringKeyHeader = "\r\n--" + boundary +
                                      "\r\nContent-Disposition: form-data; name=\"{0}\"" +
                                      "\r\n\r\n{1}\r\n";

                foreach (string key in stringDict.Keys)
                {
                    string formitem = string.Format(stringKeyHeader, key, stringDict[key]);
                    byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                    memStream.Write(formitembytes, 0, formitembytes.Length);
                }

                // 写入最后的结束边界符
                memStream.Write(endBoundary, 0, endBoundary.Length);

                httpWebRequest.ContentLength = memStream.Length;

                var requestStream = httpWebRequest.GetRequestStream();

                memStream.Position = 0;
                var tempBuffer = new byte[memStream.Length];
                memStream.Read(tempBuffer, 0, tempBuffer.Length);
                memStream.Close();

                requestStream.Write(tempBuffer, 0, tempBuffer.Length);
                requestStream.Close();

                var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var httpStreamReader = new StreamReader(httpWebResponse.GetResponseStream(),
                    Encoding.UTF8))
                {
                    responseContent = httpStreamReader.ReadToEnd();
                }

                httpWebResponse.Close();
                httpWebRequest.Abort();
                return responseContent;
            }
            catch
            {
                // 避免TCP出现大量CLOSE_WAIT状态的连接
                if (httpWebRequest != null) httpWebRequest.Abort();
                throw;
            }
        }
        #endregion

        #region PostJson 以 application/json;charset=utf-8 方式Post数据
        public string PostJson(string url, Dictionary<string, object> jsonCollection)
        {
            var json = JsonConvert.SerializeObject(jsonCollection);
            var byteRequest = Encoding.UTF8.GetBytes(json);

            HttpWebRequest httpWebRequest = null;

            try
            {
                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                httpWebRequest.ServicePoint.Expect100Continue = false;
                httpWebRequest.CookieContainer = cc;
                httpWebRequest.ContentType = "application/json;charset=utf-8";
                httpWebRequest.Headers.Add(HeaderAppend);
                httpWebRequest.Referer = url;
                httpWebRequest.Accept = accept;
                httpWebRequest.UserAgent = userAgent;
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentLength = byteRequest.Length;

                Stream stream = httpWebRequest.GetRequestStream();
                stream.Write(byteRequest, 0, byteRequest.Length);
                stream.Close();

                HttpWebResponse httpWebResponse;
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, encoding);

                string html = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();

                return html;
            }
            catch
            {
                // 避免TCP出现大量CLOSE_WAIT状态的连接
                if (httpWebRequest != null) httpWebRequest.Abort();
                throw;
            }
        }
        #endregion

        #region Put 以 application/x-www-form-urlencoded 方式PUT数据
        public string Put(string url, string putData)
        {
            HttpWebRequest httpWebRequest = null;

            try
            {
                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                httpWebRequest.ServicePoint.Expect100Continue = false;
                httpWebRequest.CookieContainer = this.CookieContainer;
                httpWebRequest.ContentType = this.ContentType;
                httpWebRequest.Headers.Add(HeaderAppend);
                httpWebRequest.Referer = url;
                httpWebRequest.Accept = this.accept;
                httpWebRequest.UserAgent = this.userAgent;
                httpWebRequest.Method = "PUT";

                byte[] byteRequest = this.Encoding.GetBytes(putData);
                httpWebRequest.ContentLength = byteRequest.Length;
                Stream stream = httpWebRequest.GetRequestStream();
                stream.Write(byteRequest, 0, byteRequest.Length);
                stream.Close();

                HttpWebResponse httpWebResponse;
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, this.Encoding);
                string html = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();

                return html;
            }
            catch
            {
                // 避免TCP出现大量CLOSE_WAIT状态的连接
                if (httpWebRequest != null) httpWebRequest.Abort();
                throw;
            }
        }
        #endregion

        #region PutJson 以 application/json;charset=utf-8 方式Put数据
        public string PutJson(string url, Dictionary<string, object> jsonCollection)
        {
            var json = JsonConvert.SerializeObject(jsonCollection);
            var byteRequest = Encoding.UTF8.GetBytes(json);

            HttpWebRequest httpWebRequest = null;

            try
            {
                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                httpWebRequest.ServicePoint.Expect100Continue = false;
                httpWebRequest.CookieContainer = cc;
                httpWebRequest.ContentType = "application/json;charset=utf-8";
                httpWebRequest.Headers.Add(HeaderAppend);
                httpWebRequest.Referer = url;
                httpWebRequest.Accept = accept;
                httpWebRequest.UserAgent = userAgent;
                httpWebRequest.Method = "PUT";
                httpWebRequest.ContentLength = byteRequest.Length;

                Stream stream = httpWebRequest.GetRequestStream();
                stream.Write(byteRequest, 0, byteRequest.Length);
                stream.Close();

                HttpWebResponse httpWebResponse;
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, encoding);

                string html = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();

                return html;
            }
            catch
            {
                // 避免TCP出现大量CLOSE_WAIT状态的连接
                if (httpWebRequest != null) httpWebRequest.Abort();
                throw;
            }
        }
        #endregion

        #region Del
        public string Del(string url)
        {
            HttpWebRequest httpWebRequest = null;

            try
            {
                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                httpWebRequest.ServicePoint.Expect100Continue = false;
                httpWebRequest.CookieContainer = this.CookieContainer;
                httpWebRequest.ContentType = this.ContentType;
                httpWebRequest.Headers.Add(HeaderAppend);
                httpWebRequest.Referer = url;
                httpWebRequest.Accept = this.accept;
                httpWebRequest.UserAgent = this.userAgent;
                httpWebRequest.Method = "DELETE";

                HttpWebResponse httpWebResponse;
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, this.Encoding);
                string html = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();

                return html;
            }
            catch
            {
                // 避免TCP出现大量CLOSE_WAIT状态的连接
                if (httpWebRequest != null) httpWebRequest.Abort();
                throw;
            }
        }
        #endregion
    }

}
