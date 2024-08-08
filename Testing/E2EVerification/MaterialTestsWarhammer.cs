﻿using GameWorld.Core.Rendering.Shading.Capabilities;
using GameWorld.Core.Rendering.Shading.Factories;
using GameWorld.Core.Rendering.Shading.Shaders;
using GameWorld.Core.Services.SceneSaving.Material.Strategies;
using GameWorld.WpfWindow.ResourceHandling;
using Shared.Core.PackFiles;
using Shared.Core.PackFiles.Models;
using Shared.Core.Services;
using Shared.GameFormats.RigidModel;
using Shared.GameFormats.WsModel;

namespace E2EVerification
{




    internal class MaterialTestsWarhammer
    {
        ResourceLibrary _resourceLib;
        PackFileService _pfs;
        ApplicationSettingsService _appSettings;
        PackFileContainer _outputPackfile;

        [SetUp]
        public void Setup()
        {
            _appSettings = new ApplicationSettingsService();
            _pfs = new PackFileService(new PackFileDataBase(false), _appSettings, new GameInformationFactory(), null, null, null);
            _resourceLib = new ResourceLibrary(_pfs);
            _pfs.LoadFolderContainer(@"C:/Users/ole_k/source/repos/TheAssetEditor/Data/Karl_and_celestialgeneral_Pack");

            _outputPackfile = _pfs.CreateNewPackFileContainer("output", PackFileCAType.MOD, true);
        }

        [Test]
        public void CreateMaterial_Wh3_Default()
        {
            var meshPackFile = _pfs.FindFile(@"variantmeshes\wh_variantmodels\hu1\emp\emp_props\emp_karl_franz_hammer_2h_01.rigid_model_v2");
            var rmv2 = ModelFactory.Create().Load(meshPackFile!.DataSource.ReadData());

            var abstractMaterialFactory = new AbstractMaterialFactory(_appSettings, _pfs, _resourceLib);
            var material = abstractMaterialFactory.CreateFactory(GameTypeEnum.Warhammer3).Create(rmv2.ModelList[0][0], null);

            Assert.That(material, Is.TypeOf<DefaultMetalRoughPbrMaterial>());

            var defaultCapabiliy = material.TryGetCapability<DefaultCapabilityMetalRough>();
            Assert.That(defaultCapabiliy, Is.Not.Null);
            Assert.That(defaultCapabiliy.MaterialMap.TexturePath, Is.EqualTo(@"variantmeshes/wh_variantmodels/hu1/emp/emp_props/tex/emp_karl_franz_hammer_2h_01_material_map.dds"));
            Assert.That(defaultCapabiliy.MaterialMap.UseTexture, Is.True);
            Assert.That(defaultCapabiliy.UseAlpha, Is.False);
        }



        //public void CreateMaterial_Wh3_Emissive()
        //public void CreateMaterial_Wh2_Default()
        //public void CreateMaterial_Rome2_Default()
        //public void CreateMaterial_Rome2_Decals()

        [Test]
        public void GenerateWsModel_Wh3_MetalRoughPbr_Default()
        {
            var abstractMaterialFactory = new AbstractMaterialFactory(_appSettings, _pfs, _resourceLib);
            var materialWithoutAlpha = abstractMaterialFactory.CreateFactory(GameTypeEnum.Warhammer3).CreateMaterial(CapabilityMaterialsEnum.MetalRoughPbr_Default);
            var materialWithAlpha = abstractMaterialFactory.CreateFactory(GameTypeEnum.Warhammer3).CreateMaterial(CapabilityMaterialsEnum.MetalRoughPbr_Default);

            var defaultMaterielWithoutAlpha = materialWithoutAlpha as DefaultMetalRoughPbrMaterial;
            Assert.That(defaultMaterielWithoutAlpha, Is.Not.Null);

            defaultMaterielWithoutAlpha.GetCapability<DefaultCapabilityMetalRough>().NormalMap.TexturePath = "customFolder/customfilename.dds";
            materialWithAlpha.GetCapability<DefaultCapabilityMetalRough>().UseAlpha = true;

            List<WsModelGeneratorInput> input =  
            [
                new WsModelGeneratorInput(0, 0, "MesheshThatShareMaterial", UiVertexFormat.Cinematic, defaultMaterielWithoutAlpha),
                new WsModelGeneratorInput(0, 1, "MesheshThatShareMaterial", UiVertexFormat.Cinematic, defaultMaterielWithoutAlpha),
                new WsModelGeneratorInput(1, 0, "Mesh", UiVertexFormat.Weighted, defaultMaterielWithoutAlpha),
                new WsModelGeneratorInput(2, 0, "Mesh", UiVertexFormat.Static, defaultMaterielWithoutAlpha),
                new WsModelGeneratorInput(2, 0, "Mesh_WithAlpha", UiVertexFormat.Static, materialWithAlpha)
            ];

            var capabilityMaterialBuilder = CapabilityMaterialFactory.GetBuilder(_appSettings.CurrentSettings.CurrentGame);
            var wsModelGenerator = new WsModelGeneratorService(_pfs);
            var result = wsModelGenerator.GenerateWsModel(@"variantmeshes\wh_variantmodels\hu1\emp\emp_props\emp_karl_franz_hammer_2h_01.rigid_model_v2", input, capabilityMaterialBuilder);

            var wsModelPackFile = _pfs.FindFile(result.CreatedFilePath);
            var wsModel = new WsModelFile(wsModelPackFile);

            var wsMatrial = _pfs.FindFile("Material");
        }

        [Test]
        public void GenerateMaterial_Rome_SpecGlossPbr_Default()
        { 
        
        }



    }
}
