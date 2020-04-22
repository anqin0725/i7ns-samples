using System;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Pkcs;

namespace iText.Samples.Signatures.Chapter02
{
    public class C2_08_SignatureMetadata
    {
        public static readonly string DEST = "results/signatures/chapter02/";

        public static readonly string KEYSTORE = "../../../resources/encryption/ks";
        public static readonly string SRC = "../../../resources/pdfs/hello_to_sign.pdf";

        public static readonly char[] PASSWORD = "password".ToCharArray();

        public static readonly String[] RESULT_FILES =
        {
            "field_metadata.pdf"
        };

        public void Sign(String src, String name, String dest, X509Certificate[] chain, ICipherParameters pk,
            String digestAlgorithm, PdfSigner.CryptoStandard subfilter, String reason, String location,
            String contact, String fullName)
        {
            PdfReader reader = new PdfReader(src);
            PdfSigner signer = new PdfSigner(reader, new FileStream(dest, FileMode.Create), new StampingProperties());

            // Create the signature appearance
            PdfSignatureAppearance appearance = signer.GetSignatureAppearance();
            appearance.SetReason(reason);
            appearance.SetLocation(location);
            appearance.SetContact(contact);

            signer.SetFieldName(name);

            // Set the signature event to allow modification of the signature dictionary.
            signer.SetSignatureEvent(new CustomISignatureEvent(fullName));

            PrivateKeySignature pks = new PrivateKeySignature(pk, digestAlgorithm);

            // Sign the document using the detached mode, CMS or CAdES equivalent.
            signer.SignDetached(pks, chain, null, null, null, 0, subfilter);
        }

        public static void Main(String[] args)
        {
            DirectoryInfo directory = new DirectoryInfo(DEST);
            directory.Create();

            Pkcs12Store pk12 = new Pkcs12Store(new FileStream(KEYSTORE, FileMode.Open, FileAccess.Read), PASSWORD);
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

            C2_08_SignatureMetadata app = new C2_08_SignatureMetadata();
            app.Sign(SRC, "Signature1", DEST + RESULT_FILES[0], chain, pk,
                DigestAlgorithms.SHA256, PdfSigner.CryptoStandard.CMS,
                "Test metadata", "Ghent", "555 123 456", "Bruno L. Specimen");
        }

        private class CustomISignatureEvent : PdfSigner.ISignatureEvent
        {
            private readonly String fullName;

            public CustomISignatureEvent(String fullName)
            {
                this.fullName = fullName;
            }

            public void GetSignatureDictionary(PdfSignature sig)
            {
                sig.Put(PdfName.Name, new PdfString(fullName));
            }
        }
    }
}