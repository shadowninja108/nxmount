using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using nxmount.Frontend.ViewModels;
using nxmount.Util;
using ReactiveUI;
using ReactiveValidation;
using ReactiveValidation.Extensions;

namespace nxmount.Frontend.Model
{

    [Serializable]
    public partial class ConfigItem : ViewModelBase
    {
        [ObservableProperty]
        [Required]
        [CustomValidation(typeof(ConfigItem), nameof(ValidatePath))]
        private string _path;

        [ObservableProperty]
        [Required]
        [NotifyPropertyChangedFor(nameof(Path))]
        private bool _folderOf;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowFolderOfOption))]
        [NotifyPropertyChangedFor(nameof(Path))]
        [Required]
        private SourceType _source;
        
        public bool ShowFolderOfOption => Source != SourceType.Sd && Source != SourceType.NspOrXci;

        public static ValidationResult ValidatePath(string path, ValidationContext context)
        {
            var model = (ConfigItem)context.ObjectInstance;
            if (model.FolderOf)
            {
                if (Directory.Exists(path))
                    return ValidationResult.Success!;
                else
                    return new("Folder does not exist");
            }

            switch (model.Source)
            {
                case SourceType.Sd:
                    var dir = new DirectoryInfo(path);
                    if(!dir.Exists)
                        return new("Folder does not exist");
                    var subdir = dir.GetDirectory("Nintendo/Contents/registered");
                    if (!subdir.Exists)
                        return new("Not a valid Switch SD card path. Ensure you're pointing to the root of the SD card.");
                    break;
                case SourceType.NcaFolder:
                case SourceType.NspOrXci:
                    if (!Directory.Exists(path))
                        return new("Folder does not exist");
                    break;
                case SourceType.Nsp:
                case SourceType.Xci:
                    if (!File.Exists(path))
                        return new("File does not exist");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return ValidationResult.Success!;
        }
    }
}
