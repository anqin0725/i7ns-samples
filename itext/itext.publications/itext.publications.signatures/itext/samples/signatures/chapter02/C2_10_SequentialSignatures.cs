/*

This file is part of the iText (R) project.
Copyright (c) 1998-2020 iText Group NV

*/
/*
* This class is part of the white paper entitled
* "Digital Signatures for PDF documents"
* written by Bruno Lowagie
*
* For more info, go to: http://itextpdf.com/learn
*/

using System;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Renderer;
using iText.Signatures;
using Org.BouncyCastle.Pkcs;

namespace iText.Samples.Signatures.Chapter02
{
    public class C2_10_SequentialSignatures
    {
        public static readonly string DEST = "results/signatures/chapter02/";
        public static readonly string FORM = "results/signatures/chapter02/multiple_signatures.pdf";

        public static readonly string ALICE = "../../../resources/encryption/alice";
        public static readonly string BOB = "../../../resources/encryption/bob";
        public static readonly string CAROL = "../../../resources/encryption/carol";
        public static readonly string KEYSTORE = "../../../resources/encryption/ks";

        public static readonly char[] PASSWORD = "password".ToCharArray();

        public static readonly String[] RESULT_FILES =
        {
            "signed_by_alice.pdf", "signed_by_bob.pdf",
            "signed_by_carol.pdf", "signed_by_alice2.pdf",
            "signed_by_bob2.pdf", "signed_by_carol2.pdf",
            "signed_by_alice3.pdf", "signed_by_bob3.pdf",
            "signed_by_carol3.pdf", "signed_by_alice4.pdf",
            "signed_by_bob4.pdf", "signed_by_carol4.pdf",
        };

        public void CreateForm()
        {
            PdfDocument pdfDoc = new PdfDocument(new PdfWriter(FORM));
            Document doc = new Document(pdfDoc);

            Table table = new Table(1).UseAllAvailableWidth();
            table.AddCell("Signer 1: Alice");
            table.AddCell(CreateSignatureFieldCell("sig1"));
            table.AddCell("Signer 2: Bob");
            table.AddCell(CreateSignatureFieldCell("sig2"));
            table.AddCell("Signer 3: Carol");
            table.AddCell(CreateSignatureFieldCell("sig3"));
            doc.Add(table);

            doc.Close();
        }

        protected Cell CreateSignatureFieldCell(String name)
        {
            Cell cell = new Cell();
            cell.SetHeight(50);
            cell.SetNextRenderer(new SignatureFieldCellRenderer(cell, name));
            return cell;
        }

        public void Sign(String keystore, int level, String src, String name, String dest)
        {
            Pkcs12Store pk12 = new Pkcs12Store(new FileStream(keystore, FileMode.Open, FileAccess.Read), PASSWORD);
            string alias = null;
            foreach (var a in pk12.Aliases)
            {
                alias = ((string) a);
                if (pk12.IsKeyEntry(alias))
                    break;
            }

            ICipherParameters pk = pk12.GetKey(alias).Key;
            X509CertificateEntry[] ce = pk12.GetCertificateChain(alias);
            X509Certificate[] chain = new X509Certificate[ce.Length];
            for (int k = 0; k < ce.Length; ++k)
            {
                chain[k] = ce[k].Certificate;
            }

            PdfReader reader = new PdfReader(src);
            PdfSigner signer = new PdfSigner(reader, new FileStream(dest, FileMode.Create),
                new StampingProperties().UseAppendMode());

            // Set the signer options
            signer.SetFieldName(name);
            signer.SetCertificationLevel(level);

            IExternalSignature pks = new PrivateKeySignature(pk, DigestAlgorithms.SHA256);

            // Sign the document using the detached mode, CMS or CAdES equivalent.
            signer.SignDetached(pks, chain, null, null, null, 0,
                PdfSigner.CryptoStandard.CMS);
        }

        public static void Main(String[] args)
        {
            DirectoryInfo directory = new DirectoryInfo(DEST);
            directory.Create();

            C2_10_SequentialSignatures app = new C2_10_SequentialSignatures();
            app.CreateForm();

            /* Alice signs certification signature (allowing form filling),
             * then Bob and Carol sign approval signature (not certified).
             */
            app.Sign(ALICE, PdfSigner.CERTIFIED_FORM_FILLING, FORM,
                "sig1", DEST + RESULT_FILES[0]);
            app.Sign(BOB, PdfSigner.NOT_CERTIFIED, DEST + RESULT_FILES[0],
                "sig2", DEST + RESULT_FILES[1]);
            app.Sign(CAROL, PdfSigner.NOT_CERTIFIED, DEST + RESULT_FILES[1],
                "sig3", DEST + RESULT_FILES[2]);

            /* Alice signs approval signatures (not certified), so does Bob
             * and then Carol signs certification signature allowing form filling.
             */
            app.Sign(ALICE, PdfSigner.NOT_CERTIFIED, FORM,
                "sig1", DEST + RESULT_FILES[3]);
            app.Sign(BOB, PdfSigner.NOT_CERTIFIED, DEST + RESULT_FILES[3],
                "sig2", DEST + RESULT_FILES[4]);
            app.Sign(CAROL, PdfSigner.CERTIFIED_FORM_FILLING, DEST + RESULT_FILES[4],
                "sig3", DEST + RESULT_FILES[5]);

            /* Alice signs approval signatures (not certified), so does Bob
             * and then Carol signs certification signature forbidding any changes to the document.
             */
            app.Sign(ALICE, PdfSigner.NOT_CERTIFIED, FORM,
                "sig1", DEST + RESULT_FILES[6]);
            app.Sign(BOB, PdfSigner.NOT_CERTIFIED, DEST + RESULT_FILES[6],
                "sig2", DEST + RESULT_FILES[7]);
            app.Sign(CAROL, PdfSigner.CERTIFIED_NO_CHANGES_ALLOWED, DEST + RESULT_FILES[7],
                "sig3", DEST + RESULT_FILES[8]);

            /* Alice signs certification signature (allowing form filling), then Bob signs approval
             * signatures (not certified) and then Carol signs certification signature allowing form filling.
             */
            app.Sign(ALICE, PdfSigner.CERTIFIED_FORM_FILLING, FORM,
                "sig1", DEST + RESULT_FILES[9]);
            app.Sign(BOB, PdfSigner.NOT_CERTIFIED, DEST + RESULT_FILES[9],
                "sig2", DEST + RESULT_FILES[10]);
            app.Sign(CAROL, PdfSigner.CERTIFIED_FORM_FILLING, DEST + RESULT_FILES[10],
                "sig3", DEST + RESULT_FILES[11]);
        }

        private class SignatureFieldCellRenderer : CellRenderer
        {
            public String name;

            public SignatureFieldCellRenderer(Cell modelElement, String name)
                : base(modelElement)
            {
                this.name = name;
            }

            public override void Draw(DrawContext drawContext)
            {
                base.Draw(drawContext);
                PdfFormField field = PdfFormField.CreateSignature(drawContext.GetDocument(),
                    GetOccupiedAreaBBox());
                field.SetFieldName(name);
                field.GetWidgets()[0].SetHighlightMode(PdfAnnotation.HIGHLIGHT_INVERT);
                field.GetWidgets()[0].SetFlags(PdfAnnotation.PRINT);
                PdfAcroForm.GetAcroForm(drawContext.GetDocument(), true).AddField(field);
            }
        }
    }
}