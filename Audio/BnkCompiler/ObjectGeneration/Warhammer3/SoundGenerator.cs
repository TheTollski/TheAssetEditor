﻿using Audio.FileFormats.WWise;
using Audio.FileFormats.WWise.Hirc.V136;
using CommunityToolkit.Diagnostics;
using Shared.Core.PackFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Audio.BnkCompiler.ObjectGeneration.Warhammer3
{
    public class SoundGenerator : IWWiseHircGenerator
    {
        public string GameName => CompilerConstants.Game_Warhammer3;
        public Type AudioProjectType => typeof(GameSound);

        private readonly PackFileService _pfs;

        public SoundGenerator(PackFileService pfs)
        {
            _pfs = pfs;
        }

        public HircItem ConvertToWWise(IAudioProjectHircItem projectItem, CompilerData project)
        {
            var typedProjectItem = projectItem as GameSound;
            Guard.IsNotNull(typedProjectItem);
            return ConvertToWWise(typedProjectItem, project);
        }

        public CAkSound_v136 ConvertToWWise(GameSound inputSound, CompilerData project)
        {
            var file = _pfs.FindFile(inputSound.Path);
            var soundIdStr = Path.GetFileNameWithoutExtension(inputSound.Path).Trim();
            var soundId = uint.Parse(soundIdStr);

            var nodeBaseParams = NodeBaseParams.CreateDefault();

            var wwiseSound = new CAkSound_v136()
            {
                Id = project.GetHircItemIdFromName(inputSound.Name),
                Type = HircType.Sound,
                AkBankSourceData = new AkBankSourceData()
                {
                    PluginId = 0x00040001,  // [VORBIS]
                    StreamType = SourceType.Streaming,
                    akMediaInformation = new AkMediaInformation()
                    {
                        SourceId = soundId,
                        uInMemoryMediaSize = (uint)file.DataSource.Size,
                        uSourceBits = 0x01,
                    }
                },
                NodeBaseParams = nodeBaseParams
            };

            wwiseSound.NodeBaseParams.DirectParentID = project.GetHircItemIdFromName(inputSound.DirectParentID);

            wwiseSound.UpdateSize();
            return wwiseSound;
        }

        public List<CAkSound_v136> ConvertToWWise(IEnumerable<GameSound> inputSound, CompilerData project)
        {
            return inputSound.Select(x => ConvertToWWise(x, project)).ToList();
        }
    }
}
