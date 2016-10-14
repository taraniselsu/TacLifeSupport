/**
* REPOSoftTech KSP Utilities
* (C) Copyright 2015, Jamie Leighton
*
* Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
* project is in no way associated with nor endorsed by Squad.
* 
*
* Licensed under the Attribution-NonCommercial-ShareAlike (CC BY-NC-SA 4.0) creative commons license. 
* See <https://creativecommons.org/licenses/by-nc-sa/4.0/> for full details (except where else specified in this file).
*
*/
using System.Collections.Generic;
using KSP.UI.Screens;
using RUI.Icons.Selectable;
using UnityEngine;

namespace Tac
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class TACEditorFilter : MonoBehaviour
    {
        // This class ass a Filter Icon to the Editor to show TACLS Parts
        private static List<AvailablePart> TacavPartItems = new List<AvailablePart>();
        public static TACEditorFilter Instance;
        internal string category = "Filter by Function";
        internal string subCategoryTitle = "TAC LS Items";
        internal string defaultTitle = "Tac";
        internal bool filter = true;
        
        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
            }
            Instance = this;
            DontDestroyOnLoad(this);
        }

        public void Setup()
        {
            this.Log("TACLS EditorFilter Setup");
            RemoveSubFilter();
            AddPartUtilitiesCat();
            GameEvents.onGUIEditorToolbarReady.Remove(SubCategories);
            
            if (!HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms>().EditorFilter)
            {
                this.Log("EditorFilter Option is Off");
                return;
            }
            
            this.Log("EditorFilter Option is On");
            TacMMCallBack();
            RemovePartUtilitiesCat();
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
            //TacMMCallBack();
            this.Log("DFEditorFilter Awake Complete");
        }

        public bool TacMMCallBack()
        {
            this.Log("TACLS EditorFilter TacMMCallBack");
            TacavPartItems.Clear();
            foreach (AvailablePart avPart in PartLoader.LoadedPartsList)
            {
                if (!avPart.partPrefab) continue;
                if (avPart.name.Contains("Tac") || avPart.name.Contains("HexCan"))
                {
                    TacavPartItems.Add(avPart);
                }
            }
            this.Log("TACLS EditorFilter TacMMCallBack end");
            return true;
        }

        private void RemoveSubFilter()
        {
            if (PartCategorizer.Instance != null)
            {
                PartCategorizer.Category Filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == category);
                if (Filter != null)
                {
                    PartCategorizer.Category subFilter = Filter.subcategories.Find(f => f.button.categoryName == subCategoryTitle);
                    if (subFilter != null)
                    {
                        subFilter.DeleteSubcategory();
                    }
                }
            }
        }

        private void RemovePartUtilitiesCat()
        {
            foreach (AvailablePart avPart in TacavPartItems)
            {
                avPart.category = PartCategories.none;
            }
        }

        private void AddPartUtilitiesCat()
        {
            foreach (AvailablePart avPart in TacavPartItems)
            {
                avPart.category = PartCategories.Utility;
            }
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
            RemoveSubFilter();
            Icon filterTacLS = new Icon("TACLSEditor", Textures.EditorCatIcon, Textures.EditorCatIcon, true);
            PartCategorizer.Category Filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == category);
            PartCategorizer.AddCustomSubcategoryFilter(Filter, subCategoryTitle, filterTacLS, p => EditorItemsFilter(p));
            //GameEvents.onGUIEditorToolbarReady.Remove(SubCategories);
        }
    }
}
