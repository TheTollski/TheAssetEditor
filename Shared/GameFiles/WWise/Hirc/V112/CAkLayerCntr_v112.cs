﻿using Shared.Core.ByteParsing;

namespace Shared.GameFormats.WWise.Hirc.V112
{
    public class CAkLayerCntr_v112 : HircItem, ICAkLayerCntr
    {
        public NodeBaseParams NodeBaseParams { get; set; }
        public Children Children { get; set; }
        public List<CAkLayer> LayerList { get; set; } = [];
        public byte BIsContinuousValidation { get; set; }
        public uint GetDirectParentId() => NodeBaseParams.DirectParentId;

        protected override void CreateSpecificData(ByteChunk chunk)
        {
            NodeBaseParams = NodeBaseParams.Create(chunk);
            Children = Children.Create(chunk);

            var layerCount = chunk.ReadUInt32();
            for (var i = 0; i < layerCount; i++)
                LayerList.Add(CAkLayer.Create(chunk));

            BIsContinuousValidation = chunk.ReadByte();
        }

        public override void UpdateSize() => throw new NotImplementedException();
        public override byte[] GetAsByteArray() => throw new NotImplementedException();
        public List<uint> GetChildren() => Children.ChildIdList;
    }

    public class CAkLayer
    {
        public uint UlLayerIr { get; set; }
        public InitialRtpc InitialRtpc { get; set; }
        public uint RtpcId { get; set; }
        public AkRtpcType RtpcType { get; set; }
        public List<CAssociatedChildData> CAssociatedChildDataList { get; set; } = [];

        public static CAkLayer Create(ByteChunk chunk)
        {
            var instance = new CAkLayer();
            instance.UlLayerIr = chunk.ReadUInt32();
            instance.InitialRtpc = InitialRtpc.Create(chunk);
            instance.RtpcId = chunk.ReadUInt32();
            instance.RtpcType = (AkRtpcType)chunk.ReadByte();
            var ulNumAssoc = chunk.ReadUInt32();
            for (var i = 0; i < ulNumAssoc; i++)
                instance.CAssociatedChildDataList.Add(CAssociatedChildData.Create(chunk));
            return instance;
        }
    }

    public class CAssociatedChildData
    {
        public uint UlAssociatedChildId { get; set; }
        public uint UlCurveSize { get; set; }
        public byte UnknownCustom1 { get; set; }
        public List<AkRtpcGraphPoint> AkRtpcGraphPointList { get; set; } = [];

        public static CAssociatedChildData Create(ByteChunk chunk)
        {
            var instance = new CAssociatedChildData();
            instance.UlAssociatedChildId = chunk.ReadUInt32();
            instance.UlCurveSize = chunk.ReadUInt32();
            for (var i = 0; i < instance.UlCurveSize; i++)
                instance.AkRtpcGraphPointList.Add(AkRtpcGraphPoint.Create(chunk));
            return instance;
        }
    }
}
