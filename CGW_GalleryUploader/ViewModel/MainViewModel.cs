using System.Diagnostics;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using KcaLibrary.Core.Types.Enumerations;
using System.Text;

namespace CGW_GalleryUploader.ViewModel {
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase {
        private string localDirectory;
        private string userName;
        private string password;
        private int bottomIndex;
        private string logFile;

        private System.Collections.ObjectModel.ObservableCollection<string> logList;

        private RelayCommand selectDirectoryCmd;
        private RelayCommand startUploadCmd;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel() {
            if (IsInDesignMode) {

            }
            else {
                this.selectDirectoryCmd = new RelayCommand(SelectDirectory);
                this.startUploadCmd = new RelayCommand(startUpload);
                this.logList = new System.Collections.ObjectModel.ObservableCollection<string>();
                this.logList.CollectionChanged += LogList_CollectionChanged;

                logFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CGW_GalleryUploader.log");
                if (File.Exists(logFile)) {

                    try {
                        File.Delete(logFile);
                    }
                    catch (Exception ex) {
                        log(KcaLogLevel.Error, "Logfile konnte nicht gelöscht werden.");
                        log(KcaLogLevel.Error, ex.Message);
                    }
                }

            }
        }

        private void LogList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            this.RaisePropertyChanged("LogList");
            this.BottomIndex = this.LogList.Count - 1;
        }

        public string LocalDirectory {
            get { return localDirectory; }
            set { this.Set("LocalDirectory", ref localDirectory, value); }
        }

        public string UserName {
            get { return userName; }
            set { this.Set("UserName", ref userName, value); }
        }

        public string Password {
            get { return password; }
            set { this.Set("Password", ref password, value); }
        }

        public int BottomIndex {
            get {
                return bottomIndex;
            }
            set {
                this.Set("BottomIndex", ref bottomIndex, value);
            }
        }

        public ObservableCollection<string> LogList {
            get {
                return logList;
            }
        }

        public RelayCommand SelectDirectoryCmd {
            get { return selectDirectoryCmd; }
        }
        public RelayCommand StartUploadCmd {
            get { return startUploadCmd; }
        }

        public void SelectDirectory() {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog dialog =
                new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value) {
                this.LocalDirectory = dialog.SelectedPath;
            }
        }


        private async void startUpload() {
            try {
                string msg = null;
                if (string.IsNullOrWhiteSpace(this.localDirectory) || !Directory.Exists(localDirectory)) {
                    msg = "Der angegebene lokale Ordner ist ungültig.";
                }
                if (string.IsNullOrWhiteSpace(this.UserName)) {
                    msg = "Der angegebene Benutzername ist ungültig.";
                }
                if (string.IsNullOrWhiteSpace(this.Password)) {
                    msg = "Das angegeben Passwort ist ungültig";
                }
                if (msg != null) {
                    MessageBox.Show(msg, "Ungültige Angaben", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else {
                    try {
                        Uploader uplidupli = new Uploader(UserName, Password);
                        uplidupli.LogEvent += Uplidupli_LogEvent;
                        await uplidupli.DoThisEvilStuff(LocalDirectory);
                    }
                    catch (Exception ex) {
                        do {
                            Uplidupli_LogEvent(this, new KcaLibrary.Core.Types.Events.KcaLogEventArgs(ex.Message));
                            ex = ex.InnerException;
                        } while (ex.InnerException != null);
                    }
                }
            }
            catch(Exception ex) {
                StringBuilder sb = new StringBuilder();
                do {
                    sb.AppendLine(ex.Message);
                    ex = ex.InnerException;
                } while (ex != null);
                MessageBox.Show(sb.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Uplidupli_LogEvent(object sender, KcaLibrary.Core.Types.Events.KcaLogEventArgs e) {
            log(e.LogLevel, e.Message);
        }

        private void log(KcaLogLevel loglevel, string message) {
            try {
                using (StreamWriter wrtr =
                    new StreamWriter(this.logFile, true)) {
                    wrtr.WriteLine($"{loglevel}\t{DateTime.Now}\t{message}");
                }
            }
            finally {
                this.LogList.Add(message);
            }
        }
    }
}