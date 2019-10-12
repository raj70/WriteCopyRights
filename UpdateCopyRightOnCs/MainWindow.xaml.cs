using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UpdateCopyRightOnCs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Browser_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select Folder";
                dialog.ShowNewFolderButton = true;
                if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FolderNameTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void GetFile_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FolderNameTextBox.Text) || !Directory.Exists(FolderNameTextBox.Text))
            {
                return;
            }

            FileListBox.Items.Clear();
            NumberOfFilesTextBlock.Text = string.Empty;

            FileService service = new FileService(FolderNameTextBox.Text, CsFile.FileExtension);
            var list = service.CollectFile();

            NumberOfFilesTextBlock.Text = $"{list.Count} Files";
            list.ForEach(x =>
            {
                FileListBox.Items.Add(x);
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string copyRightsText = $"/** {Environment.NewLine}"+ 
                                    $"* Copyright %year% rajen shrestha{Environment.NewLine}" +
                                    $"* All right are reserved.Reproduction or transmission in whole or in{Environment.NewLine}" +
                                    $"* part, in any form or by any means, electronic, mechanical or otherwise{Environment.NewLine}" +
                                    $"* is published without the prior written consent of the copyright owner.{Environment.NewLine}" +
                                    $"* {Environment.NewLine}" +
                                    $"* Author: rajen shrestha{Environment.NewLine}" +
                                    $"* Time: %DateTime%{Environment.NewLine}" +
                                    $"*/ " + Environment.NewLine;
           
            foreach(var file in FileListBox.Items)
            {
                var newText = copyRightsText;
                var vsFile = file as CsFile;
                newText = newText.Replace("%year%", vsFile.Year.ToString());
                newText = newText.Replace("%DateTime%", vsFile.FileCreatedDate.Value.ToString());
                var content = File.ReadAllText(vsFile.File);

                var newContent = newText + content;

                File.WriteAllText(vsFile.File, newContent);
            }
        }
    }

    public class CsFile
    {
        public static readonly string FileExtension = ".cs";
        /// <summary>
        /// FilePath
        /// </summary>
        public string File { get; set; }
        public int Year
        {
            get
            {
                if (FileCreatedDate != null)
                {
                    return FileCreatedDate.Value.Year;
                }
                return 2019;
            }
        }
        public string FileName
        {
            get
            {
                if (!string.IsNullOrEmpty(File))
                {
                    return System.IO.Path.GetFileName(File);
                }

                return FileExtension;
            }
        }
        public string CreatedDateTime
        {
            get
            {
                if (FileCreatedDate != null)
                {
                    return FileCreatedDate.Value.ToString("dd/MM/yyyy hh:mm:ss");
                }
                return "";
            }
        }
        public DateTime? FileCreatedDate { get; set; }

        public override string ToString()
        {
            return $"{FileName} - {Year}";
        }
    }

    public class CsFiles : List<CsFile>
    {
        
    }

    public class FileService
    {
        private string _folder;
        private string _fileExtensionForSearch;
        public FileService(string folder, string fileExtensionForSearch)
        {
            _folder = folder;
            _fileExtensionForSearch = fileExtensionForSearch;
        }

        public CsFiles CollectFile()
        {
            var list = new CsFiles();

            foreach (var file in Directory.GetFiles(_folder, "*.cs", SearchOption.AllDirectories))
            {
                if (CanExclude(file))
                {
                    continue;
                }

                if (FileContainsCopyright(file))
                {
                    continue;
                }

                var csFile = new CsFile()
                {
                    File = file,
                    FileCreatedDate = File.GetCreationTime(file)
                };

                list.Add(csFile);
            }

            return list;
        }

        public bool CanExclude(string file)
        {
            bool exclude = false;

            ExcludedFile.ForEach(x => {
                exclude = file.Contains(x);
                var f = System.IO.Path.GetFileNameWithoutExtension(file).Split('_');
                if(f.Length > 1)
                {
                    exclude = true;
                }
            });

            return exclude;
        }

        public bool FileContainsCopyright(string file)
        {
            return File.ReadAllText(file).Contains("Copyright");
        }

        public List<string> ExcludedFile
        {
            get
            {
                var list = new List<string>();

                list.Add("AssembleInfo.cs");
                list.Add("obj");

                return list;
            }
        }
    }
}
