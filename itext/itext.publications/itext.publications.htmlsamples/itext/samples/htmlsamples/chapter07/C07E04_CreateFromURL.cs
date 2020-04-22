using System;
using System.IO;
using System.Net;
using iText.Html2pdf;
using iText.License;

namespace iText.Samples.Htmlsamples.Chapter07
{
    /// <summary>
    /// Converts a simple HTML file to PDF using an InputStream and an OutputStream as arguments for the convertToPdf() method.
    /// </summary>
    public class C07E04_CreateFromURL
    {
        /// <summary>
        /// The path to the resulting PDF file.
        /// </summary>
        public static readonly String DEST = "results/htmlsamples/ch07/url2pdf_1.pdf";

        /// <summary>
        /// The target folder for the result.
        /// </summary>
        public static readonly String ADDRESS = "https://stackoverflow.com/help/on-topic";

        /// <summary>
        /// The main method of this example.
        /// </summary>
        /// <param name="args">no arguments are needed to run this example.</param>
        public static void Main(String[] args)
        {
            LicenseKey.LoadLicenseFile(Environment.GetEnvironmentVariable("ITEXT7_LICENSEKEY") +
                                       "/itextkey-html2pdf_typography.xml");
            FileInfo file = new FileInfo(DEST);
            file.Directory.Create();

            new C07E04_CreateFromURL().CreatePdf(new Uri(ADDRESS), DEST);
        }

        /// <summary>
        /// Creates the PDF file.
        /// </summary>
        /// <param name="url">the URL object for the web page</param>
        /// <param name="dest">the path to the resulting PDF</param>
        public void CreatePdf(Uri url, String dest)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType) 3072;
            var httpWebRequest = (HttpWebRequest) WebRequest.Create(url);
            var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();
            HtmlConverter.ConvertToPdf(httpResponse.GetResponseStream(),
                new FileStream(dest, FileMode.Create));
        }
    }
}