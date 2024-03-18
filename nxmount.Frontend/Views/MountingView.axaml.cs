using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using nxmount.Apps;
using nxmount.Apps.Sources;
using nxmount.Frontend.Model;
using nxmount.Frontend.ViewModels;
using nxmount.Windows;
using nxmount.Util;
namespace nxmount.Frontend.Views
{
    public partial class MountingView : UserControl
    {
        private MountingViewModel Model => (MountingViewModel)DataContext;

        public MountingView()
        {
            InitializeComponent();
        }
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            var window = Model.Parent;
            new Thread(() =>
            {
                var sources = new List<IAppSource>();
                foreach (var config in window.Config.Items)
                {
                    switch (config.Source)
                    {
                        case SourceType.NcaFolder:
                            if (!config.FolderOf)
                                sources.AddNcaFolder(new DirectoryInfo(config.Path));
                            else
                                sources.AddFolderOfNcaFolders(new DirectoryInfo(config.Path));
                            break;
                        case SourceType.Nsp:
                            if(!config.FolderOf)
                                sources.AddNsp(new FileInfo(config.Path));
                            else
                                sources.AddFolderOfNsps(new DirectoryInfo(config.Path));
                            break;
                        case SourceType.Xci:
                            if (!config.FolderOf)
                                sources.AddXci(new FileInfo(config.Path));
                            else
                                sources.AddFolderOfXcis(new DirectoryInfo(config.Path));
                            break;
                        case SourceType.NspOrXci:
                            sources.AddFolderOfNspsOrXcis(new DirectoryInfo(config.Path));
                            break;
                        case SourceType.Sd:
                            sources.Add(new SdCardAppSource(new DirectoryInfo(config.Path)));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                window.AppManager = new AppManager(sources);
                window.MountService = MountDriver.Create(window.AppManager!, window.Config.PreferredLanguage);
                window.MountService!.Start();

                Dispatcher.UIThread.Post(() =>
                {
                    window.MountPoint = window.MountService.MountPoint;
                    window.TransitMounted();
                }, DispatcherPriority.Normal);
            }).Start();
        }
    }
}
