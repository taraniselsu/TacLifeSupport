using System.Collections.Generic;
using KSP.UI.Screens;
using RUI.Icons.Selectable;
using UnityEngine;


namespace Tac
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class TACEditorFilter : MonoBehaviour
    {
        // This class ass a Filter Icon to the Editor to show TACLS Parts
        private static List<AvailablePart> TacavPartItems = new List<AvailablePart>();

        internal string category = "Filter by Function";
        internal string subCategoryTitle = "TAC LS Items";
        internal string defaultTitle = "Tac";

        //internal string iconName = "R&D_node_icon_evatech";
        //create and the icons
        
        internal bool filter = true;

        private void Awake()
        {
            Debug.Log("TACLS EditorFilter Awake");
            GameEvents.onGUIEditorToolbarReady.Add(SubCategories);
            /*
            //Attempt to add Module Manager callback  - find the base type
            System.Type MMType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "ModuleManager.MMPatchLoader");
            if (MMType != null)
            {
                MethodInfo MMPatchLoaderInstanceMethod = MMType.GetMethod("get_Instance", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
                if (MMPatchLoaderInstanceMethod != null)
                {
                    object actualMM = MMPatchLoaderInstanceMethod.Invoke(null,
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static, null, null, null);
                    MethodInfo MMaddPostPatchCallbackMethod = MMType.GetMethod("addPostPatchCallback", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
                    if (actualMM != null && MMaddPostPatchCallbackMethod != null)
                        MMaddPostPatchCallbackMethod.Invoke(actualMM, new object[] { this.DFMMCallBack() });
                }
                
            }*/
            TacMMCallBack();
            
            Debug.Log("DFEditorFilter Awake Complete");
        }

        public bool TacMMCallBack()
        {
            Debug.Log("TACLS EditorFilter TacMMCallBack");
            TacavPartItems.Clear();
            foreach (AvailablePart avPart in PartLoader.LoadedPartsList)
            {
                if (!avPart.partPrefab) continue;
                if (avPart.name.Contains("Tac") || avPart.name.Contains("HexCan"))
                {
                    TacavPartItems.Add(avPart);
                }
            }
            Debug.Log("TACLS EditorFilter TacMMCallBack end");
            return true;
        }

        private bool EditorItemsFilter(AvailablePart avPart)
        {
            if (TacavPartItems.Contains(avPart))
            {
                return true;
            }
            return false;
        }

        private void SubCategories()
        {
            Icon filterTacLS = new Icon("TACLSEditor", Textures.EditorCatIcon, Textures.EditorCatIcon, true);
            PartCategorizer.Category Filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == category);
            PartCategorizer.AddCustomSubcategoryFilter(Filter, subCategoryTitle, filterTacLS, p => EditorItemsFilter(p));
            //RUIToggleButtonTyped button = Filter.button.activeButton;
            //button.SetFalse(button, RUIToggleButtonTyped.ClickType.FORCED);
            //button.SetTrue(button, RUIToggleButtonTyped.ClickType.FORCED);
        }
    }
}
