﻿using AnimationEditor.PropCreator.ViewModels;
using CommonControls.Common;
using CommonControls.FileTypes.DB;
using CommonControls.Services;

namespace AnimationEditor.TechSkeletonEditor
{
    public class TechSkeletonEditorViewModel : BaseAnimationViewModel
    {
        public TechSkeletonEditorViewModel(ToolFactory toolFactory, PackFileService pfs, SkeletonAnimationLookUpHelper skeletonHelper, SchemaManager schemaManager) : base(toolFactory, pfs, skeletonHelper, schemaManager, "not_in_use1", "not_in_use2", false)
        {
            DisplayName = "TechSkeleton Editor";
            Pfs = pfs;
        }

        public PackFileService Pfs { get; }

        public override void Initialize()
        {
            MainModelView.IsControlVisible.Value = false;
            //ReferenceModelView.IsControlVisible.Value = false;
            ReferenceModelView.Data.IsSelectable = false;

            var typedEditor = new Editor(_pfs, MainModelView.Data);
            Editor = typedEditor;

            if (MainInput == null)
                MainInput = new AnimationToolInput();

            typedEditor.Create(@"warmachines\chariot\grn_wolf_chariot\tech\grn_wolf_chariot_01.anim");
        }

        public static class TechSkeleton_Debug
        {
            //public static void CreateDamselEditor(IEditorCreator creator, IToolFactory toolFactory, PackFileService packfileService)
            //{
            //    var editorView = toolFactory.CreateEditorViewModel<TechSkeletonEditorViewModel>();
            //    editorView.MainInput = new AnimationToolInput()
            //    {
            //        Mesh = packfileService.FindFile(@"variantmeshes\variantmeshdefinitions\hef_alarielle.variantmeshdefinition"),
            //        FragmentName = @"animations/animation_tables/hu1b_alarielle_staff_and_sword.frg",
            //        AnimationSlot = AnimationSlotTypeHelper.GetfromValue("STAND")
            //    };
            //    editorView.MainInput = new AnimationToolInput()
            //    {
            //        Mesh = packfileService.FindFile(@"variantmeshes\variantmeshdefinitions\skv_throt.variantmeshdefinition"),
            //        FragmentName = @"animations/animation_tables/hu17_dlc16_throt.frg",
            //        AnimationSlot = AnimationSlotTypeHelper.GetfromValue("ATTACK_5")
            //    };
            //
            //    //editorView.MainInput = new AnimationToolInput()
            //    //{
            //    //    Mesh = packfileService.FindFile(@"warmachines\engines\emp_steam_tank\emp_steam_tank01.rigid_model_v2"),
            //    //    FragmentName = @"animations/animation_tables/wm_steam_tank01.frg",
            //    //    AnimationSlot = AnimationSlotTypeHelper.GetfromValue("STAND")
            //    //};
            //    //editorView.MainInput = new AnimationToolInput()
            //    //{
            //    //    Mesh = packfileService.FindFile(@"variantmeshes\variantmeshdefinitions\emp_state_troops_crossbowmen_ror.variantmeshdefinition"),
            //    //    FragmentName = @"animations/animation_tables/hu1_empire_sword_crossbow.frg",
            //    //    AnimationSlot = AnimationSlotTypeHelper.GetfromValue("FIRE_HIGH")
            //    //};
            //
            //    creator.CreateEmptyEditor(editorView);
            //}
        }
    }
}
