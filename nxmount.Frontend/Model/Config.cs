using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using nxmount.Frontend.ViewModels;
using nxmount.Util;

namespace nxmount.Frontend.Model
{
    public partial class Config : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<ConfigItem> _items = new();

        [ObservableProperty]
        private ApplicationLanguage _preferredLanguage = ApplicationLanguage.AmericanEnglish;
    }
}
