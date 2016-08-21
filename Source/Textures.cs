
// The following Class is derived from Kerbal Alarm Clock mod. Which is licensed under:
// The MIT License(MIT) Copyright(c) 2014, David Tregoning
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Tac
{
    internal static class Textures
    {
        //Icons
        internal static Texture2D GrnApplauncherIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        internal static Texture2D YlwApplauncherIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        internal static Texture2D RedApplauncherIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);

        //Toolbar Icons
        internal static Texture2D GrnToolbarIcon = new Texture2D(24, 24, TextureFormat.ARGB32, false);
        internal static Texture2D YlwToolbarIcon = new Texture2D(24, 24, TextureFormat.ARGB32, false);
        internal static Texture2D RedToolbarIcon = new Texture2D(24, 24, TextureFormat.ARGB32, false);

        //Button Icons
        internal static Texture2D TooltipBox = new Texture2D(10, 10, TextureFormat.ARGB32, false);
        internal static Texture2D BtnRedCross = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        internal static Texture2D BtnResize = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        internal static Texture2D BtnResizeHeight = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        internal static Texture2D BtnResizeWidth = new Texture2D(16, 16, TextureFormat.ARGB32, false);

        internal static String PathIconsPath = System.IO.Path.Combine(Textures.AssemblyFolder, "Textures").Replace("\\", "/");
        internal static String PathToolbarIconsPath = PathIconsPath.Substring(PathIconsPath.ToLower().IndexOf("/gamedata/") + 10);



        internal static void LoadIconAssets()
        {
            try
            {
                LoadImageFromFile(ref GrnApplauncherIcon, "TACgreenIconAL.png", PathIconsPath);
                LoadImageFromFile(ref YlwApplauncherIcon, "TACyellowIconAL.png", PathIconsPath);
                LoadImageFromFile(ref RedApplauncherIcon, "TACredIconAL.png", PathIconsPath);
                LoadImageFromFile(ref GrnToolbarIcon, "TACgreenIconTB.png", PathIconsPath);
                LoadImageFromFile(ref YlwToolbarIcon, "TACyellowIconTB.png", PathIconsPath);
                LoadImageFromFile(ref RedToolbarIcon, "TACredIconTB.png", PathIconsPath);
                LoadImageFromFile(ref TooltipBox, "TACToolTipBox.png", PathIconsPath);
                LoadImageFromFile(ref BtnRedCross, "TACbtnRedCross.png", PathIconsPath);
                LoadImageFromFile(ref BtnResize, "TACbtnResize.png", PathIconsPath);
                LoadImageFromFile(ref BtnResizeHeight, "TACbtnResizeHeight.png", PathIconsPath);
                LoadImageFromFile(ref BtnResizeWidth, "TACbtnResizeWidth.png", PathIconsPath);
            }
            catch (Exception)
            {
                Debug.Log("TAC - LS Failed to Load Textures - are you missing a file?");
            }
        }

        public static Boolean LoadImageFromFile(ref Texture2D tex, String fileName, String folderPath = "")
        {            
            Boolean blnReturn = false;
            try
            {
                if (folderPath == "") folderPath = PathIconsPath;

                //File Exists check
                if (System.IO.File.Exists(String.Format("{0}/{1}", folderPath, fileName)))
                {
                    try
                    {                        
                        tex.LoadImage(System.IO.File.ReadAllBytes(String.Format("{0}/{1}", folderPath, fileName)));
                        blnReturn = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("TAC - LS  Failed to load the texture:" + folderPath + "(" + fileName + ")");
                        Debug.Log(ex.Message);
                    }
                }
                else
                {
                    Debug.Log("TAC - LS  Cannot find texture to load:" + folderPath + "(" + fileName + ")");                    
                }


            }
            catch (Exception ex)
            {
                Debug.Log("TAC - LS  Failed to load (are you missing a file):" + folderPath + "(" + fileName + ")");
                Debug.Log(ex.Message);                
            }
            return blnReturn;
        }

        internal static GUIStyle ResizeStyle, ClosebtnStyle;
        internal static GUIStyle sectionTitleStyle, subsystemButtonStyle, statusStyle, warningStyle, PartListStyle, PartListPartStyle;
        internal static GUIStyle scrollStyle, resizeStyle;

        internal static bool StylesSet = false;

        internal static void SetupStyles()
        {
            GUI.skin = HighLogic.Skin;

            //Init styles

            RSTUtils.Utilities._TooltipStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                stretchHeight = true,
                wordWrap = true,
                border = new RectOffset(3, 3, 3, 3),
                padding = new RectOffset(4, 4, 6, 4),
                alignment = TextAnchor.MiddleCenter
            };
            RSTUtils.Utilities._TooltipStyle.normal.background = TooltipBox;
            RSTUtils.Utilities._TooltipStyle.normal.textColor = new Color32(207, 207, 207, 255);
            RSTUtils.Utilities._TooltipStyle.hover.textColor = Color.blue;

            ClosebtnStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = 20,
                fixedHeight = 20,
                fontSize = 14,
                fontStyle = FontStyle.Normal
            };
            ClosebtnStyle.active.background = GUI.skin.toggle.onNormal.background;
            ClosebtnStyle.onActive.background = ClosebtnStyle.active.background;
            ClosebtnStyle.padding = RSTUtils.Utilities.SetRectOffset(ClosebtnStyle.padding, 3);

            ResizeStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = 20,
                fixedHeight = 20,
                fontSize = 14,
                fontStyle = FontStyle.Normal
            };
            ResizeStyle.onActive.background = ClosebtnStyle.active.background;
            ResizeStyle.padding = RSTUtils.Utilities.SetRectOffset(ClosebtnStyle.padding, 3);

            //Init styles
            sectionTitleStyle = new GUIStyle(GUI.skin.label);
            sectionTitleStyle.alignment = TextAnchor.MiddleCenter;
            sectionTitleStyle.stretchWidth = true;
            sectionTitleStyle.fontStyle = FontStyle.Bold;

            statusStyle = new GUIStyle(GUI.skin.label);
            statusStyle.alignment = TextAnchor.MiddleLeft;
            statusStyle.stretchWidth = true;
            statusStyle.normal.textColor = Color.white;

            warningStyle = new GUIStyle(GUI.skin.label);
            warningStyle.alignment = TextAnchor.MiddleLeft;
            warningStyle.stretchWidth = true;
            warningStyle.fontStyle = FontStyle.Bold;
            warningStyle.normal.textColor = Color.red;

            subsystemButtonStyle = new GUIStyle(GUI.skin.toggle);
            subsystemButtonStyle.margin.top = 0;
            subsystemButtonStyle.margin.bottom = 0;
            subsystemButtonStyle.padding.top = 0;
            subsystemButtonStyle.padding.bottom = 0;

            scrollStyle = new GUIStyle(GUI.skin.scrollView);

            PartListStyle = new GUIStyle(GUI.skin.label);
            PartListStyle.alignment = TextAnchor.MiddleLeft;
            PartListStyle.stretchWidth = false;
            PartListStyle.normal.textColor = Color.yellow;

            PartListPartStyle = new GUIStyle(GUI.skin.label);
            PartListPartStyle.alignment = TextAnchor.LowerLeft;
            PartListPartStyle.stretchWidth = false;
            PartListPartStyle.normal.textColor = Color.white;

            resizeStyle = new GUIStyle(GUI.skin.button);
            resizeStyle.alignment = TextAnchor.MiddleCenter;
            resizeStyle.padding = new RectOffset(1, 1, 1, 1);

            StylesSet = true;

        }

        #region Assembly/Class Information

        /// <summary>
        /// Name of the Assembly that is running this MonoBehaviour
        /// </summary>
        internal static String AssemblyName
        { get { return Assembly.GetExecutingAssembly().GetName().Name; } }

        /// <summary>
        /// Full Path of the executing Assembly
        /// </summary>
        internal static String AssemblyLocation
        { get { return Assembly.GetExecutingAssembly().Location.Replace("\\", "/"); } }

        /// <summary>
        /// Folder containing the executing Assembly
        /// </summary>
        internal static String AssemblyFolder
        { get { return Path.GetDirectoryName(AssemblyLocation).Replace("\\", "/"); } }

        #endregion Assembly/Class Information
    }
}
