﻿using AssetEditor.Services;
using GameWorld.Core.Components.Input;
using GameWorld.WpfWindow;
using GameWorld.WpfWindow.ResourceHandling;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shared.Core.DependencyInjection;
using Shared.Core.Events;
using Shared.Core.PackFiles;
using Shared.Core.PackFiles.Models;

namespace E2EVerification.Shared
{
    public class AssetEditorTestRunner
    {
        private readonly IServiceProvider _serviceProvider;

        public IServiceScope EditorServiceProvider { get; private set; }
        public PackFileService PackFileService { get; private set; }
        public PackFileContainer? OutputPackFile { get; private set; }
        public IUiCommandFactory CommandFactory { get; private set; }
        public ScopeRepository ScopeRepository { get; private set; }

        public AssetEditorTestRunner()
        {
            _serviceProvider = new DependencyInjectionConfig().Build(MockServices);
            EditorServiceProvider = _serviceProvider.CreateScope();

            var game = EditorServiceProvider.ServiceProvider.GetRequiredService<IWpfGame>();
            var resourceLibrary = EditorServiceProvider.ServiceProvider.GetRequiredService<ResourceLibrary>();
            resourceLibrary.Initialize(game);

            PackFileService = EditorServiceProvider.ServiceProvider.GetRequiredService<PackFileService>();
            CommandFactory = EditorServiceProvider.ServiceProvider.GetRequiredService<IUiCommandFactory>();
            ScopeRepository = EditorServiceProvider.ServiceProvider.GetRequiredService<ScopeRepository>();
        }

        public PackFileContainer? LoadPackFile(string path, bool createOutputPackFile = true)
        {
            PackFileService.Load(path, false, true);
            if (createOutputPackFile)
                OutputPackFile = PackFileService.CreateNewPackFileContainer("TestOutput", PackFileCAType.MOD, true);
            return OutputPackFile;
        }

        void MockServices(IServiceCollection services)
        {
            // Find a way to disable the whole rendering loop, we dont want it! 

            var gameDescriptor = new ServiceDescriptor(typeof(IWpfGame), typeof(WpfGame), ServiceLifetime.Scoped);
            services.Remove(gameDescriptor);
            services.AddScoped<IWpfGame, GameMock>();

            var KeyboardDescriptor = new ServiceDescriptor(typeof(IKeyboardComponent), typeof(KeyboardComponent), ServiceLifetime.Scoped);
            services.Remove(KeyboardDescriptor);
            services.AddScoped(x => new Mock<IKeyboardComponent>().Object);

            var mouseDescriptor = new ServiceDescriptor(typeof(IMouseComponent), typeof(MouseComponent), ServiceLifetime.Scoped);
            services.Remove(mouseDescriptor);
            services.AddScoped(x => new Mock<IMouseComponent>().Object);
        }
    }
}
