using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PdfSharp;
using PdfSharp.Pdf.IO;

namespace PDFSplitter
{
    class Program
    {
        static void Main(string[] args)
        {
            if ( ! args.Any() )
            {
                Console.WriteLine("You must give the path to a PDF file that needs to be split");
                Console.ReadKey();
                return;
            }

            var combinedPDFPath = args[0];

            if( ! File.Exists( combinedPDFPath ) )
            {
                Console.WriteLine("Could not open file " + combinedPDFPath);
                Console.ReadKey();
                return;
            }

            var doc = PdfReader.Open(combinedPDFPath, PdfDocumentOpenMode.Import);

            var documentName = Path.GetFileNameWithoutExtension(combinedPDFPath);

            var dir = Path.GetDirectoryName(combinedPDFPath);

            var outputPath = Path.Combine(dir, documentName);

            if (! Directory.Exists( outputPath ) )
            {
                Directory.CreateDirectory(outputPath);
            }

            for(int pageNumber = 0; pageNumber < doc.PageCount; pageNumber++)
            {
                var page = doc.Pages[pageNumber];

                var text = page.ExtractString();
                var name = FindTextBetween(text, "This is to certify that", "has completed").Trim();
                
                if(string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("Could not find text from page " + pageNumber);
                    continue;
                }

                var splitdoc = new PdfSharp.Pdf.PdfDocument();
                splitdoc.AddPage(page);
                var filename = outputPath + "\\" + name + ".pdf";
                splitdoc.Save(filename);
                splitdoc.Close();
                Console.WriteLine("Saved page " + filename);
            }

            Console.WriteLine("");
            Console.WriteLine("Press any key to close.");
            Console.ReadKey();
        }

        // from https://stackoverflow.com/questions/43273508/get-string-between-strings-in-c-sharp
        public static string FindTextBetween(string text, string left, string right)
        {
            // TODO: Validate input arguments

            int beginIndex = text.IndexOf(left); // find occurence of left delimiter
            if (beginIndex == -1)
                return string.Empty; // or throw exception?

            beginIndex += left.Length;

            int endIndex = text.IndexOf(right, beginIndex); // find occurence of right delimiter
            if (endIndex == -1)
                return string.Empty; // or throw exception?

            return text.Substring(beginIndex, endIndex - beginIndex).Trim();
        }
    }
}
