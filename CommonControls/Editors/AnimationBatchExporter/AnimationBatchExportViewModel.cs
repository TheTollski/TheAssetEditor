﻿using Common;
using CommonControls.Common;
using CommonControls.ErrorListDialog;
using CommonControls.Services;
using CommonControls.Simple;
using Filetypes.Animation;
using FileTypes.PackFiles.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using static CommonControls.ErrorListDialog.ErrorListViewModel;

namespace CommonControls.Editors.AnimationBatchExporter
{
    public class AnimationBatchExportViewModel
    {
        ILogger _logger = Logging.Create<AnimationBatchExportViewModel>();
        PackFileService _pfs;

        public ObservableCollection<PackFileListItem> PackfileList { get; set; } = new ObservableCollection<PackFileListItem>();
        public ObservableCollection<uint> PossibleOutputFormats { get; set; } = new ObservableCollection<uint>();
        public NotifyAttr<uint> SelectedOutputFormat { get; set; } = new NotifyAttr<uint>(7);

        public AnimationBatchExportViewModel(PackFileService pfs)
        {
            _pfs = pfs;

            var containers = _pfs.GetAllPackfileContainers();
            foreach (var item in containers)
            {
                if (item == _pfs.GetEditablePack())
                    continue;
                PackfileList.Add(new PackFileListItem(item));
            }

            PossibleOutputFormats.Add(5);
            PossibleOutputFormats.Add(6);
            PossibleOutputFormats.Add(7);
        }

        public void Process()
        {
            var outputPack = _pfs.GetEditablePack();
            if (outputPack == null)
            {
                MessageBox.Show("No output packfile selectd. Please set Editable pack before running the converter", "Error");
                return;
            }

            if (MessageBox.Show("The converter will preplace any file with overlapping names in the output folder. Are you sure?", "", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            var errorList = new ErrorList();

            using (new WaitCursor())
            {
                foreach (var packfile in PackfileList)
                {
                    if (packfile.Process.Value == false)
                        continue;

                    _logger.Here().Information($"Processing packfile container {packfile.Name}");

                    var animFiles = _pfs.FindAllWithExtention(".anim", packfile.Container);

                    _logger.Here().Information($"Converting animations {animFiles.Count}");
                    var convertedAnimFiles = ConvertAnimFiles(animFiles, SelectedOutputFormat.Value, errorList);

                    _logger.Here().Information($"saving animation files");
                    _pfs.AddFilesToPack(_pfs.GetEditablePack(), 
                        convertedAnimFiles.Select(x => x.directory).ToList(), 
                        convertedAnimFiles.Select(x => x.file).ToList());

                    _logger.Here().Information($"Saving inv matix files");
                    var invMatrixFileList = _pfs.FindAllWithExtention(".bone_inv_trans_mats", packfile.Container);
                    foreach (var invMatrixFile in invMatrixFileList)
                        _pfs.CopyFileFromOtherPackFile(packfile.Container, _pfs.GetFullPath(invMatrixFile), _pfs.GetEditablePack());
                }
            }

            ErrorListWindow.ShowDialog("Bach result", errorList, true);
        }

        List<(PackFile file, string directory)> ConvertAnimFiles(List<PackFile> packFiles, uint outputAnimationFormat, ErrorList errorList)
        {
            var output = new List<(PackFile file, string directory)>();

            foreach (var file in packFiles)
            {
                try
                {
                    var animationFile = AnimationFile.Create(file);
                    animationFile.Header.AnimationFormat = outputAnimationFormat;

                    var bytes = AnimationFile.GetBytes(animationFile);
                    var newPackFile = new PackFile(file.Name, new MemorySource(bytes));

                    var path = _pfs.GetFullPath(file);
                    var directoryPath = Path.GetDirectoryName(path);

                    output.Add((newPackFile, directoryPath));
                }
                catch(Exception e)
                {
                    var path = _pfs.GetFullPath(file);
                    errorList.Error(path, e.Message);
                }
            }

            return output;
        }

        public static void ShowWindow(PackFileService pfs)
        {
            var window = new ControllerHostWindow(true)
            {
                DataContext = new AnimationBatchExportViewModel(pfs),
                Title = "Animation batch converter",
                Content = new AnimationBatchExportView(),
                Width = 400,
                Height = 300,
            };
            window.Show();
        }

        public class PackFileListItem
        {
            public PackFileContainer Container { get; private set; }

            public PackFileListItem(PackFileContainer item)
            {
                Name.Value = item.Name;
                Container = item;
            }

            public NotifyAttr<bool> Process { get; set; } = new NotifyAttr<bool>(true);
            public NotifyAttr<string> Name { get; set; } = new NotifyAttr<string>("");
         
        }

    }
}
