using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using nxmount.Frontend.Model;
using nxmount.Frontend.Util;
using nxmount.Frontend.ViewModels;
using System.IO;

namespace nxmount.Frontend.Views;

public partial class ConfigView : UserControl
{
    private ConfigViewModel Model => (ConfigViewModel) DataContext;
    private MainWindowViewModel Parent => Model.Parent;


    private static readonly FilePickerFileType NspFileType =
        new("Nintendo Submission Package")
        {
            Patterns = ["*.nsp"],
            MimeTypes = ["application/nintendonsp"],
            AppleUniformTypeIdentifiers = ["data"]
        };

    private static readonly FilePickerFileType XciFileType =
        new("NX Card Image")
        {
            Patterns = ["*.xci"],
            MimeTypes = ["application/nintendoxci"],
            AppleUniformTypeIdentifiers = ["data"]
        };

    private static readonly JsonSerializerOptions ConfigSerializerOptions = new()
    {
        /* Ignore UI related state. */
        IgnoreReadOnlyProperties = true
    };
    private static readonly FilePickerFileType ConfigFileType =

        new("nxmount Config File")
        {
            Patterns = ["*.nxmc"],
            MimeTypes = ["application/nxmountconfig"],
            AppleUniformTypeIdentifiers = ["data"]
        };

    public ConfigView()
    {
        InitializeComponent();
    }

    private void OnAddSourceClicked(object? sender, RoutedEventArgs e)
    {
        Parent.Config.Items.Add(new ConfigItem());
    }

    private void OnRemoveSourceClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.DataContext is not ConfigItem model) return;

        Parent.Config.Items.Remove(model);
    }

    private async void OnBrowseForSourceClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.DataContext is not ConfigItem model) return;


        var isFolder = model.FolderOf || model.Source == SourceType.Sd || model.Source == SourceType.NspOrXci || model.Source == SourceType.NcaFolder;

        if (isFolder)
        {
            var folder = await DoOpenFolder($"Select {model.Source.ToDescription()} folder");
            if(folder == null)
                return;

            model.Path = folder.Path.LocalPath;
        }
        else
        {
            var file = model.Source switch
            {
                SourceType.Nsp => await DoOpenFile("Select NSP", NspFileType),
                SourceType.Xci => await DoOpenFile("Select XCI", XciFileType),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (file == null)
                return;

            model.Path = file.Path.LocalPath;
        }
    }

    private async void OnLoadConfigClicked(object? sender, RoutedEventArgs e)
    {
        var fileStorage = await DoOpenFile("Open nxmount Config File", ConfigFileType);
        if (fileStorage == null) return;

        var file = new FileInfo(fileStorage.Path.LocalPath);
        if(!file.Exists) return;

        var config = await JsonSerializer.DeserializeAsync<Config>(file.OpenRead(), ConfigSerializerOptions);
        if (config == null)
        {
            /* TODO: error */
            return;
        }

        Parent.Config = config;
    }

    private async void OnSaveConfigClicked(object? sender, RoutedEventArgs e)
    {
        var fileStorage = await DoSaveFile("Save nxmount Config File", ConfigFileType, "Config.nxmc", ".nxmc");
        if (fileStorage == null) return;

        var file = new FileInfo(fileStorage.Path.LocalPath);

        var config = Parent.Config;
        var document = JsonSerializer.SerializeToDocument(config, ConfigSerializerOptions);

        await using var stream = file.Create();
        await using var jsonWriter = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

        document.WriteTo(jsonWriter);
        await jsonWriter.FlushAsync();
    }

    private async Task<IStorageFile?> DoSaveFile(
        string title, 
        FilePickerFileType fileType,
        string fileName,
        string extension
    )
    {
        var topLevel = TopLevel.GetTopLevel(this);
        return await topLevel!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = title,
            FileTypeChoices = [fileType],
            SuggestedFileName = fileName,
            ShowOverwritePrompt = true,
            DefaultExtension = extension,
        });
    }

    private async Task<IStorageFile?> DoOpenFile(string title, FilePickerFileType fileType)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        return await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = [fileType]
        }).ContinueWith(x => x.IsFaulted ? null : x.Result.FirstOrDefault());
    }

    private async Task<IStorageFolder?> DoOpenFolder(string title)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        return await topLevel!.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        }).ContinueWith(x => x.IsFaulted ? null : x.Result.FirstOrDefault());
    }

    private void OnStartClicked(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}
