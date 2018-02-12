using MahApps.Metro.Controls;
using System;
using System.IO;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;

namespace EasyShare
{
    /// <summary>
    /// Logica di interazione per RenamingFile.xaml
    /// </summary>
    public partial class RenamingFile : MetroWindow
    {
        public string NewName { get => newName; set => newName = value; }
        public string CurrentDirectory { get => currentDirectory; set => currentDirectory = value; }
        public int Tipo { get => tipo; set => tipo = value; }
        public string Extension { get => extension; set => extension = value; }

        public RenamingFile()
        {
            InitializeComponent();
        }

        //type 0=> file, 1=> directory
        public void setFields(string fileName,string directory, int type)
        {
            NewName = String.Empty;
            Extension = Path.GetExtension(fileName);
            CurrentDirectory = directory;
            motivationTextBlock.Text = fileName;
            motivationTextBlock.ToolTip = fileName;
            actionTextBlock.Text = "Scegli un altro nome";
            Tipo = type;
        }

        private string newName, currentDirectory, extension;
        private int tipo;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NewName = newNameTextBox.Text;
            if (String.IsNullOrEmpty(NewName))
            {
                this.ShowModalMessageExternal("Ops", "Inserisci un nome valido");
                return;
            }
            if (Tipo == 0)
            {
                if (String.Compare(Extension, Path.GetExtension(NewName)) != 0)
                    NewName += Extension;

                if (File.Exists(CurrentDirectory + "\\" + NewName))
                    this.ShowModalMessageExternal("Ops", "Il file \"" + NewName + "\" esiste già");
                else if (CurrentDirectory.Length + 1 + NewName.Length >= 260)
                    this.ShowModalMessageExternal("Ops", "Il nome scelto è troppo lungo (max " + (259 - CurrentDirectory.Length) + ")");
                else Close();
            }
            else if (Tipo == 1)
            {
                if (Directory.Exists(CurrentDirectory + "\\" + NewName))
                    this.ShowModalMessageExternal("Ops", "La directory \"" + NewName + "\" esiste già");
                else if (CurrentDirectory.Length + 1 + NewName.Length >= 248)
                    this.ShowModalMessageExternal("Ops", "nome directory troppo lungo (max " + (247 - CurrentDirectory.Length) + ")");
                else Close();
            }
        }
    }
}