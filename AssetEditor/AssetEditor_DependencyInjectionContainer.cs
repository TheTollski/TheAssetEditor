﻿using AssetEditor.DevelopmentConfiguration;
using AssetEditor.Services;
using AssetEditor.UiCommands;
using AssetEditor.ViewModels;
using AssetEditor.Views;
using AssetEditor.Views.Settings;
using Microsoft.Extensions.DependencyInjection;
using SharedCore.Misc;
using SharedCore.ToolCreation;

namespace AssetEditor
{
    internal class AssetEditor_DependencyInjectionContainer : DependencyContainer
    {
        public override void Register(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<MainWindow>();
            serviceCollection.AddScoped<MainViewModel>();
            serviceCollection.AddScoped<IEditorCreator, EditorCreator>();

            serviceCollection.AddTransient<OpenEditorCommand>();
            serviceCollection.AddTransient<OpenFileInEditorCommand>();

            serviceCollection.AddTransient<SettingsWindow>();
            serviceCollection.AddScoped<SettingsViewModel>();
            serviceCollection.AddScoped<MenuBarViewModel>();

            serviceCollection.AddTransient<DevelopmentConfigurationManager>();
            RegisterAllAsInterface<IDeveloperConfiguration>(serviceCollection, ServiceLifetime.Scoped);
        }
    }
}
