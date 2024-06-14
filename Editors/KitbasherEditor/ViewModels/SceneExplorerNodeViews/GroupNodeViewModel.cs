﻿using GameWorld.Core.SceneNodes;
using Shared.Core.Misc;

namespace KitbasherEditor.ViewModels.SceneExplorerNodeViews
{
    public class GroupNodeViewModel : NotifyPropertyChangedImpl, ISceneNodeViewModel
    {
        GroupNode _node;

        public string GroupName { get => _node.Name; set { _node.Name = value; NotifyPropertyChanged(); } }

        public GroupNodeViewModel(GroupNode node)
        {
            _node = node;
        }

        public void Dispose()
        {
        }
    }
}
