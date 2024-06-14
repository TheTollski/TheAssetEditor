﻿using Audio.BnkCompiler;
using Audio.BnkCompiler.ObjectGeneration;
using Audio.BnkCompiler.ObjectGeneration.Warhammer3;
using Audio.Presentation.AudioExplorer;
using Audio.Presentation.Compiler;
using Audio.Storage;
using Audio.Utility;
using Editors.Audio.Presentation.AudioEditor;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.DependencyInjection;
using Shared.Core.PackFiles;
using Shared.Core.ToolCreation;
using Shared.GameFormats.WWise;
using System.IO;
using System.Linq;

namespace Editors.Audio
{
    public class DependencyInjectionContainer : DependencyContainer
    {
        public override void Register(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<VgStreamWrapper>();

            serviceCollection.AddScoped<AudioExplorerView>();
            serviceCollection.AddScoped<AudioExplorerViewModel>();

            serviceCollection.AddScoped<CompilerView>();
            serviceCollection.AddScoped<CompilerViewModel>();

            serviceCollection.AddScoped<AudioEditorView>();
            serviceCollection.AddScoped<AudioEditorViewModel>();

            serviceCollection.AddScoped<RepositoryProvider, CreateRepositoryFromAllPackFiles>();
            serviceCollection.AddScoped<IAudioRepository, AudioRepository>();

            serviceCollection.AddTransient<WWiseBnkLoader>();
            serviceCollection.AddTransient<WWiseNameLoader>();
            serviceCollection.AddTransient<BnkParser>();
            serviceCollection.AddTransient<SoundPlayer>();

            serviceCollection.AddScoped<IWWiseHircGenerator, ActionGenerator>();
            serviceCollection.AddScoped<IWWiseHircGenerator, EventGenerator>();
            serviceCollection.AddScoped<IWWiseHircGenerator, SoundGenerator>();
            serviceCollection.AddScoped<IWWiseHircGenerator, ActorMixerGenerator>();
            serviceCollection.AddScoped<IWWiseHircGenerator, RandomContainerGenerator>();
            serviceCollection.AddScoped<IWWiseHircGenerator, DialogueEventGenerator>();
            serviceCollection.AddScoped<HircBuilder>();
            serviceCollection.AddScoped<BnkHeaderBuilder>();
            serviceCollection.AddScoped<CompilerService>();
            serviceCollection.AddScoped<ProjectLoader>();
            serviceCollection.AddScoped<AudioFileImporter>();
            serviceCollection.AddScoped<Compiler>();
            serviceCollection.AddScoped<ResultHandler>();
        }

        public override void RegisterTools(IToolFactory factory)
        {
            factory.RegisterTool<AudioExplorerViewModel, AudioExplorerView>();
            factory.RegisterTool<CompilerViewModel, CompilerView>();// ( new ExtentionToTool( EditorEnums.AudioCompiler_Editor,  new[] { ".audio_json"}));
            factory.RegisterTool<AudioEditorViewModel, AudioEditorView>();
        }
    }

    public static class AudioTool_Debug
    {
        public static void CreateOvnCompilerProject(PackFileService pfs)
        {
            PackFileUtil.LoadFilesFromDisk(pfs, new PackFileUtil.FileRef(packFilePath: @"audioprojects", systemPath: @"C:\Users\ole_k\source\repos\TheAssetEditor\AudioResearch\Data\OvnExample\ProjectSimple.json"));

            // Load all wems
            var wemReferences = Directory.GetFiles(@"D:\Research\Audio\Working pack\audio_ovn\wwise\english(uk)")
                .Where(x => Path.GetExtension(x) == ".wem")
                .Select(x => new PackFileUtil.FileRef(packFilePath: @"audio\wwise", systemPath: x))
                .ToList();
            PackFileUtil.LoadFilesFromDisk(pfs, wemReferences);
        }
    }



}