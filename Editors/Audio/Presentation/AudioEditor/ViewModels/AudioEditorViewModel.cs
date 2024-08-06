﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using CommonControls.PackFileBrowser;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Editors.Audio.Presentation.AudioEditor.Views;
using Editors.Audio.Storage;
using Newtonsoft.Json;
using Serilog;
using Shared.Core.ErrorHandling;
using Shared.Core.Misc;
using Shared.Core.PackFiles;
using Shared.Core.PackFiles.Models;
using Shared.Core.ToolCreation;
using Shared.Ui.BaseDialogs.WindowHandling;
using static Editors.Audio.Presentation.AudioEditor.AudioEditorViewModelHelpers;

namespace Editors.Audio.Presentation.AudioEditor.ViewModels
{
    public partial class CustomStatesDataGridProperties : ObservableObject
    {
        [ObservableProperty] private string _customVOActor;
        [ObservableProperty] private string _customVOCulture;
        [ObservableProperty] private string _customVOBattleSelection;
        [ObservableProperty] private string _customVOBattleSpecialAbility;
        [ObservableProperty] private string _customVOFactionLeader;
    }

    public enum DialogueEventsPreset
    {
        None,
        All,
        Essential
    }

    public partial class AudioEditorViewModel : ObservableObject, IEditorViewModel
    {
        private readonly IAudioRepository _audioRepository;
        private readonly PackFileService _packFileService;
        private readonly IWindowFactory _windowFactory;
        readonly ILogger _logger = Logging.Create<AudioEditorViewModel>();

        public NotifyAttr<string> DisplayName { get; set; } = new NotifyAttr<string>("Audio Editor");

        // Audio Project settings properties:
        [ObservableProperty] private string _audioProjectFileName = "my_audio_project";
        [ObservableProperty] private string _customStatesFileName = "my_custom_states";
        [ObservableProperty] private string _selectedAudioProjectEventType;
        [ObservableProperty] private string _selectedAudioProjectEventSubtype;
        [ObservableProperty] private DialogueEventsPreset _selectedAudioProjectEventsPreset;

        // Properties for the Audio Editor DataGrid:
        [ObservableProperty] private string _selectedAudioProjectEvent;
        [ObservableProperty] private bool _showCustomStatesOnly;

        // Audio Project settings:
        [ObservableProperty] private List<string> _audioProjectEventType = AudioEditorSettings.EventType;
        [ObservableProperty] private ObservableCollection<string> _audioProjectSubtypes = []; // Determined according to what Event Type is selected

        // Audio Editor DataGrid stuff:
        [ObservableProperty] private ObservableCollection<string> _audioProjectDialogueEvents = []; // The list of events in the Audio Project.

        // DataGrid data objects:
        public ObservableCollection<Dictionary<string, object>> AudioEditorDataGridItems { get; set; } = [];
        public ObservableCollection<CustomStatesDataGridProperties> CustomStatesDataGridItems { get; set; } = [];
        public static Dictionary<string, List<Dictionary<string, object>>> EventsData => AudioEditorData.Instance.EventsData; // Data storage for AudioEditorDataGridItems - managed in a single instance for ease of access.

        public AudioEditorViewModel(IAudioRepository audioRepository, PackFileService packFileService, IWindowFactory windowFactory)
        {
            _audioRepository = audioRepository;
            _packFileService = packFileService;
            _windowFactory = windowFactory;
        }

        partial void OnSelectedAudioProjectEventTypeChanged(string value)
        {
            // Update the ComboBox for EventSubType upon EventType selection.
            UpdateAudioProjectEventSubType(this);
        }

        partial void OnSelectedAudioProjectEventChanged(string value)
        {
            AudioEditorData.Instance.SelectedAudioProjectEvent = value;

            // Load the Event upon selection.
            LoadEvent(this, _audioRepository, ShowCustomStatesOnly);
        }

        partial void OnShowCustomStatesOnlyChanged(bool value)
        {
            // Load the Event again to reset the ComboBoxes in the DataGrid.
            LoadEvent(this, _audioRepository, ShowCustomStatesOnly);
        }

        [RelayCommand] public void CreateAudioProject()
        {
            // Remove any pre-existing data.
            EventsData.Clear();
            AudioEditorDataGridItems.Clear();
            SelectedAudioProjectEvent = "";

            // Create the object for State Groups with qualifiers so that their keys in the EventsData dictionary are unique.
            AddQualifiersToStateGroups(_audioRepository.DialogueEventsWithStateGroups);

            // Initialise EventsData according to the Audio Project settings selected.
            InitialiseEventsData(this);

            // Add the Audio Project with empty events to the PackFile.
            AudioProjectData.AddAudioProjectToPackFile(_packFileService, EventsData, AudioProjectFileName);

            // Load the custom States so that they can be referenced when the Event is loaded.
            PrepareCustomStatesForComboBox(this);
        }

        [RelayCommand] public void NewAudioProject()
        {
            var window = _windowFactory.Create<AudioEditorSettingsViewModel, AudioEditorSettingsView>("Audio Editor Settings", 550, 500);
            window.AlwaysOnTop = false;
            window.ShowWindow();
        }

        [RelayCommand] public void LoadAudioProject()
        {
            using var browser = new PackFileBrowserWindow(_packFileService, [".json"]);

            if (browser.ShowDialog())
            {
                // Remove any pre-existing data otherwise DataGrid isn't happy.
                EventsData.Clear();
                AudioEditorDataGridItems.Clear();
                SelectedAudioProjectEvent = "";

                // Create the object for State Groups with qualifiers so that their keys in the EventsData dictionary are unique.
                AddQualifiersToStateGroups(_audioRepository.DialogueEventsWithStateGroups);

                var filePath = _packFileService.GetFullPath(browser.SelectedFile);
                var file = _packFileService.FindFile(filePath);
                var bytes = file.DataSource.ReadData();
                var audioProjectJson = Encoding.UTF8.GetString(bytes);
                var eventData = AudioProjectData.ConvertAudioProjectToEventsData(_audioRepository, audioProjectJson);
                AudioEditorData.Instance.EventsData = eventData;
                _logger.Here().Information($"Loaded Audio Project file: {file.Name}");

                // Create the list of Events used in the Events ComboBox.
                CreateAudioProjectEventsListFromAudioProject(this, EventsData);

                // Load the object which stores the custom States for use in the States ComboBox.
                PrepareCustomStatesForComboBox(this);
            }
        }

        [RelayCommand] public void SaveAudioProject()
        {
            UpdateEventDataWithCurrentEvent(this);

            AudioProjectData.AddAudioProjectToPackFile(_packFileService, EventsData, AudioProjectFileName);
        }

        [RelayCommand] public void LoadCustomStates()
        {
            using var browser = new PackFileBrowserWindow(_packFileService, [".json"]);

            if (browser.ShowDialog())
            {
                // Remove any pre-existing data otherwise DataGrid isn't happy.
                CustomStatesDataGridItems.Clear();

                var filePath = _packFileService.GetFullPath(browser.SelectedFile);
                var file = _packFileService.FindFile(filePath);
                var bytes = file.DataSource.ReadData();
                var str = Encoding.UTF8.GetString(bytes);
                var customStatesFileData = JsonConvert.DeserializeObject<List<CustomStatesDataGridProperties>>(str);
                _logger.Here().Information($"Loaded Custom States file: {file.Name}");

                foreach (var customState in customStatesFileData)
                    CustomStatesDataGridItems.Add(customState);

                // Load the object which stores the custom States for use in the States ComboBox.
                PrepareCustomStatesForComboBox(this);

                // Reload the selected Event so the ComboBoxes are updated.
                LoadEvent(this, _audioRepository, ShowCustomStatesOnly);
            }
        }

        [RelayCommand] public void SaveCustomStates()
        {
            var dataGridItemsJson = JsonConvert.SerializeObject(CustomStatesDataGridItems, Formatting.Indented);
            var pack = _packFileService.GetEditablePack();
            var byteArray = Encoding.ASCII.GetBytes(dataGridItemsJson);
            _packFileService.AddFileToPack(pack, "AudioProjects", new PackFile($"{CustomStatesFileName}.json", new MemorySource(byteArray)));
            _logger.Here().Information($"Saved Custom States file: {CustomStatesFileName}");
        }

        [RelayCommand] public void AddStatePath()
        {
            if (string.IsNullOrEmpty(SelectedAudioProjectEvent))
                return;

            var newRow = new Dictionary<string, object>();

            var stateGroupsWithQualifiers = DialogueEventsWithStateGroupsWithQualifiers[SelectedAudioProjectEvent];

            foreach (var stateGroupWithQualifier in stateGroupsWithQualifiers)
            {
                var stateGroupKey = AddExtraUnderScoresToStateGroup(stateGroupWithQualifier);
                newRow[stateGroupKey] = "";
            }

            newRow["AudioFilesDisplay"] = "";

            AudioEditorDataGridItems.Add(newRow);

            UpdateEventDataWithCurrentEvent(this);
        }

        public void RemoveStatePath(Dictionary<string, object> rowToRemove)
        {
            AudioEditorDataGridItems.Remove(rowToRemove);

            UpdateEventDataWithCurrentEvent(this);
        }

        [RelayCommand] public void AddCustomStatesRow()
        {
            var newRow = new CustomStatesDataGridProperties();
            CustomStatesDataGridItems.Add(newRow);
        }

        [RelayCommand] public void RemoveCustomStatesRow(CustomStatesDataGridProperties item)
        {
            if (item != null && CustomStatesDataGridItems.Contains(item))
                CustomStatesDataGridItems.Remove(item);
        }

        public static void AddAudioFiles(Dictionary<string, object> dataGridRow, System.Windows.Controls.TextBox textBox)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Multiselect = true,
                Filter = "WAV files (*.wav)|*.wav"
            };

            if (dialog.ShowDialog() == true)
            {
                var filePaths = dialog.FileNames;
                var eventsData = AudioEditorData.Instance.EventsData;

                if (eventsData.ContainsKey(AudioEditorData.Instance.SelectedAudioProjectEvent))
                {
                    var eventList = eventsData[AudioEditorData.Instance.SelectedAudioProjectEvent];

                    // Find the matching row to insert the AudioFiles data.
                    var matchingRow = eventList.FirstOrDefault(context =>
                        DictionaryEqualityComparer<string, object>.Default.Equals(context, dataGridRow));

                    if (matchingRow != null)
                    {
                        var fileNames = filePaths.Select(filePath => $"\"{Path.GetFileName(filePath)}\"");
                        var fileNamesString = string.Join(", ", fileNames);
                        var filePathsString = string.Join(", ", filePaths.Select(filePath => $"\"{filePath}\""));

                        matchingRow["AudioFilesDisplay"] = filePaths.ToList();
                        matchingRow["AudioFiles"] = filePaths.ToList();

                        textBox.Text = fileNamesString;
                        textBox.ToolTip = filePathsString;
                    }
                }
            }
        }

        public void Close()
        {
        }

        public bool Save() => true;

        public PackFile MainFile { get; set; }

        public bool HasUnsavedChanges { get; set; } = false;
    }
}
