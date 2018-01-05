using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using KcaLibrary.Core.Extensions;
using System.Diagnostics;
using KcaLibrary.Core.Types.Interfaces;
using KcaLibrary.Core.Types.Events;
using KcaLibrary.Sftp;
using CGW_GalleryUploader.Properties;
using KcaLibrary.Sftp.Parameter;


namespace CGW_GalleryUploader {
    class Uploader : ILoggable {
        
        public event KcaLogEventHandler LogEvent;


        private ISftp sftp;

        public Uploader(string username, string password) {
            SftpServerParameter parameter = new SftpServerParameter(Settings.Default.ftpServer, 21, SftpTransferMode.Binary, 15000, ProtocolMode.Ftp);
            SftpCredentials credential = new SftpCredentials(username, password);
            sftp = new Sftp(parameter, credential);
        }

       

        public async Task<bool> DoThisEvilStuff(string localDir) {

            string tempFolder = Path.Combine(Path.GetTempPath(), "cgwUploader", Guid.NewGuid().ToString());

            if (!Directory.Exists(tempFolder)) {
                Directory.CreateDirectory(tempFolder);
            }

            string[] files = Directory.GetFiles(localDir, "*.jpg");

            foreach (string file in files) {
                string tmpName = Path.Combine(tempFolder, Path.GetFileName(file));
                File.Copy(file, tmpName);

                using (Image img = Image.FromFile(tmpName)) {
                    this.log($"File {file} wird verarbeitet");
                    try {
                        if (img.PropertyItems.Any() && 
                            img.PropertyItems.Any(d => d.Id == 40962) && 
                            img.PropertyItems.Any(d => d.Id == 40963) && 
                            img.PropertyItems.Any(d => d.Id == 274)) {

                            short width = BitConverter.ToInt16(img.PropertyItems.First(d => d.Id == 40962).Value, 0);
                            short height = BitConverter.ToInt16(img.PropertyItems.First(d => d.Id == 40963).Value, 0);
                            short orientation = BitConverter.ToInt16(img.PropertyItems.First(d => d.Id == 274).Value, 0);
                            if (orientation != 1 && width > height) {
                                if (orientation == 8) {
                                    await Task.Run(() => img.RotateFlip(RotateFlipType.Rotate270FlipNone));
                                }
                                if (orientation == 6) {
                                    await Task.Run(() => img.RotateFlip(RotateFlipType.Rotate90FlipNone));
                                }
                                this.log($"File {file} wurde gedreht");
                                await Task.Run(() => img.Save(tmpName, System.Drawing.Imaging.ImageFormat.Jpeg));
                            }
                        }
                    }
                    finally {
                        await uploadFile(tmpName);
                    }
                }
            }
            this.log("Verarbeitung abgeschlossen");
            Directory.Delete(tempFolder, true);
            return true;

        }

        private async Task uploadFile(string filename) {
            SftpFileDefinition fileDefinition = new SftpFileDefinition(filename, Settings.Default.ftpPath, true, false);
            this.log($"File {filename} wird hochgeladen");
            try {
                await sftp.UploadAsync(fileDefinition);                
            }catch(Exception ex) {
                this.log(ex.Message);
            }
        }
        
        private void log(string message) {
            if(LogEvent != null) {
                LogEvent(this, new KcaLogEventArgs(message));
            }
        }

    }
}
