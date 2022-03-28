using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFSplitterWpf
{
    internal class Splitter
    {
        SplitterVM options;

        public Splitter( SplitterVM options )
        {
            this.options = options;
        }

        public Task StartSplit(IProgress<Progress> progress)
        {
            var task = new Task(() => { split(progress); });

            task.Start();
            
            return task;
        }

        private void split(IProgress<Progress> progress)
        {
            var combinedPDFPath = options.InputPath;

            if (!File.Exists(combinedPDFPath))
            {
                Console.WriteLine("Could not open file " + combinedPDFPath);
                Console.ReadKey();
                return;
            }
            
            var outputPath = options.OutputPath;
            var progressMessage = new Progress();

            try
            {

                var doc = PdfReader.Open(combinedPDFPath, PdfDocumentOpenMode.Import);

                var documentName = Path.GetFileNameWithoutExtension(combinedPDFPath);

                var dir = Path.GetDirectoryName(combinedPDFPath);


                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                progressMessage.MaxProgress = doc.PageCount;
                progressMessage.StatusText.Append("Starting");

                for (int pageNumber = 0; pageNumber < doc.PageCount; pageNumber++)
                {
                    var page = doc.Pages[pageNumber];
                    string fileName = String.Empty;

                    if (options.usePageNumbersAsFileName)
                    {
                        fileName = documentName + " Page " + (pageNumber + 1);
                    }
                    else if (options.usePageTextAsFileName)
                    {
                        var text = page.ExtractString();

                        fileName = FindTextBetween(text, options.StartText, options.EndText).Trim();

                        if (string.IsNullOrWhiteSpace(fileName))
                        {
                            progressMessage.StatusText.AppendLine("Could not find text from page " + pageNumber);
                            continue;
                        }
                    }

                    var splitdoc = new PdfSharp.Pdf.PdfDocument();
                    splitdoc.AddPage(page);
                    var filename = outputPath + "\\" + fileName + ".pdf";
                    splitdoc.Save(filename);
                    splitdoc.Close();

                    progressMessage.StatusText.AppendLine("Saved page " + filename);
                    progressMessage.CurrentProgress = pageNumber + 1;
                    progress?.Report(progressMessage);
                }

            System.Diagnostics.Process.Start(outputPath);

            progressMessage.StatusText.AppendLine("Finished");
            progressMessage.CurrentProgress = progressMessage.MaxProgress;
            progress?.Report(progressMessage);

            }
            catch (Exception ex)
            {
                progressMessage.error = ex;
                progressMessage.CurrentProgress = 0;
                progress?.Report(progressMessage);
            }
        }

        // from https://stackoverflow.com/questions/43273508/get-string-between-strings-in-c-sharp
        private string FindTextBetween(string text, string left, string right)
        {
            // TODO: Validate input arguments

            int beginIndex = text.IndexOf(left); // find occurrence of left delimiter
            if (beginIndex == -1)
                return string.Empty; // or throw exception?

            beginIndex += left.Length;

            int endIndex = text.IndexOf(right, beginIndex); // find occurrence of right delimiter
            if (endIndex == -1)
                return string.Empty; // or throw exception?

            return text.Substring(beginIndex, endIndex - beginIndex).Trim();
        }


        public class Progress
        {
            public double MaxProgress { get; internal set; }
            public double CurrentProgress { get; internal set; }
            public StringBuilder StatusText { get; internal set; } = new StringBuilder();
            public Exception error { get; set; } = null;
        }
    }
}
