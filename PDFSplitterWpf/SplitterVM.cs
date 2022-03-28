using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PDFSplitterWpf
{
    internal class SplitterVM : INotifyPropertyChanged
    {
        // File paths

        public string InputPath { get; set; } = string.Empty;

        public string OutputPath { get; set; } = string.Empty;

        public bool CanBrowse => !IsRunning;

        // modes

        private bool _useNumbers = true;

        public bool usePageNumbersAsFileName
        { get => _useNumbers; set { _useNumbers = value; UpdateAllUI(); } }

        public bool usePageTextAsFileName
        { get => !_useNumbers; set { _useNumbers = !value; UpdateAllUI(); } }

        // Text

        private string startText = string.Empty, endText = string.Empty;

        public string StartText { get => startText; set { startText = value; UpdateAllUI(); } } 
        public string EndText { get => endText; set { endText = value; UpdateAllUI(); } }

        // Controls

        public bool IsRunning { get; private set; } = false;
        public double CurrentProgress { get; set; } = 0;
        public double MaxProgress { get; set; } = 1;
        public bool CanStart
        {
            get
            {
                if (IsRunning)
                    return false;

                if ( InputPath == string.Empty || OutputPath == string.Empty )
                    return false;

                if ( usePageTextAsFileName && ( StartText == string.Empty || EndText == string.Empty) )
                    return false;

                return true;
            }
        }

        // Property updates

        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void UpdateAllUI()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MaxProgress"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentProgress"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CanStart"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("usePageTextAsFileName"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("usePageNumbersAsFileName"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InputPath"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutputPath"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CanBrowse"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StartText"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EndText"));
        }

        // methods
        public async void Start()
        {
            // reset progress
            MaxProgress = 1;
            CurrentProgress = 0;
            
            var progress = new Progress<Splitter.Progress>(value => { 
                CurrentProgress = value.CurrentProgress;
                MaxProgress = value.MaxProgress;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MaxProgress"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentProgress"));

                if ( value.error != null )
                {
                    MessageBox.Show(value.error.Message);
                }
            });

            // lock UI
            IsRunning = true;
            UpdateAllUI();

            // asynchronously split the PDF file
            await new Splitter(this).StartSplit(progress);

            // reset UI after completion.
            IsRunning = false;
            UpdateAllUI();
        }
    }
}
