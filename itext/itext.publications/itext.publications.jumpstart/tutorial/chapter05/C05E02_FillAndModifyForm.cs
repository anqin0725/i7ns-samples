/*
* This example is part of the iText 7 tutorial.
*/
using System;
using System.Collections.Generic;
using System.IO;
using iText.Forms;
using iText.Forms.Fields;
using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;

namespace Tutorial.Chapter05 {
    /// <summary>Simple filling out form example.</summary>
    public class C05E02_FillAndModifyForm {
        public const String SRC = "resources/pdf/job_application.pdf";

        public const String DEST = "results/chapter05/filled_out_job_application.pdf";

        /// <exception cref="System.IO.IOException"/>
        public static void Main(String[] args) {
            FileInfo file = new FileInfo(DEST);
            file.Directory.Create();
            new C05E02_FillAndModifyForm().ManipulatePdf(SRC, DEST);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual void ManipulatePdf(String src, String dest) {
            //Initialize PDF document
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(src), new PdfWriter(dest));
            PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);
            IDictionary<String, PdfFormField> fields = form.GetFormFields();
            PdfFormField toSet;
            fields.TryGetValue("name", out toSet);
            toSet.SetValue("James Bond").SetBackgroundColor(Color.ORANGE);
            fields.TryGetValue("experience1", out toSet);
            toSet.SetValue("Yes");
            fields.TryGetValue("experience2", out toSet);
            toSet.SetValue("Yes");
            fields.TryGetValue("experience3", out toSet);
            toSet.SetValue("Yes");
            IList<PdfObject> options = new List<PdfObject>();
            options.Add(new PdfString("Any"));
            options.Add(new PdfString("8.30 am - 12.30 pm"));
            options.Add(new PdfString("12.30 pm - 4.30 pm"));
            options.Add(new PdfString("4.30 pm - 8.30 pm"));
            options.Add(new PdfString("8.30 pm - 12.30 am"));
            options.Add(new PdfString("12.30 am - 4.30 am"));
            options.Add(new PdfString("4.30 am - 8.30 am"));
            PdfArray arr = new PdfArray(options);
            fields.TryGetValue("shift", out toSet);
            toSet.SetOptions(arr);
            toSet.SetValue("Any");
            PdfFont courier = PdfFontFactory.CreateFont(FontConstants.COURIER);
            fields.TryGetValue("info", out toSet);
            toSet.SetValue("I was 38 years old when I became an MI6 agent.", courier, 7f);
            pdfDoc.Close();
        }
    }
}