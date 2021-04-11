﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EcoCivicsImportExportMod.Bundler.ViewModel
{
    public class CivicBundle : INotifyPropertyChanged
    {
        private CivicObject selectedCivicObject;
        private bool incomingDrop;

        public Context Context { get; }

        public Model.CivicBundle UnderlyingCivicBundle { get => Context.CivicBundle; }

        public string Name { get => string.IsNullOrEmpty(Context.FilePath) ? "Untitled Bundle" : System.IO.Path.GetFileNameWithoutExtension(Context.FilePath); }

        public ObservableCollection<CivicBundle> RootObjects { get; } = new ObservableCollection<CivicBundle>();

        public ObservableCollection<CivicObject> CivicObjects { get; } = new ObservableCollection<CivicObject>();

        public CivicObject SelectedCivicObject
        {
            get => selectedCivicObject;
            set
            {
                if (value == selectedCivicObject) { return; }
                selectedCivicObject = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCivicObject)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowUnselectedHint)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowObjectDetails)));
            }
        }

        public Visibility ShowUnselectedHint
        {
            get => SelectedCivicObject != null ? Visibility.Collapsed : Visibility.Visible;
        }

        public Visibility ShowObjectDetails
        {
            get => SelectedCivicObject == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public bool IncomingDrop
        {
            get => incomingDrop;
            set
            {
                if (value == incomingDrop) { return; }
                incomingDrop = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IncomingDrop)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TreeViewBorderSize)));
            }
        }

        public Thickness TreeViewBorderSize
        {
            get => incomingDrop ? new Thickness(3, 3, 3, 3) : new Thickness(0, 0, 0, 0);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CivicBundle(Context context)
        {
            Context = context;
            RootObjects.Add(this);
            context.OnCivicBundleChange += Context_OnCivicBundleChange;
            context.OnFilePathChange += Context_OnFilePathChange;
            UpdateCivicObjects();
        }

        private void Context_OnCivicBundleChange(object sender, EventArgs e)
        {
            UpdateCivicObjects();
        }

        private void Context_OnFilePathChange(object sender, EventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }

        private void UpdateCivicObjects()
        {
            if (UnderlyingCivicBundle == null)
            {
                CivicObjects.Clear();
                return;
            }
            var civics = UnderlyingCivicBundle.Civics
                .OrderBy(c => c.Name);
            int i = 0;
            foreach (var civic in civics)
            {
                CivicObject civicObject;
                if (i >= CivicObjects.Count)
                {
                    civicObject = new CivicObject(this, civic);
                    CivicObjects.Add(civicObject);
                }
                else
                {
                    civicObject = CivicObjects[i];
                    civicObject.BundledCivic = civic;
                }
                ++i;
            }
            while (i < CivicObjects.Count)
            {
                CivicObjects.RemoveAt(i);
            }
        }
    }
}
