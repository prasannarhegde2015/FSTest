﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by coded UI test builder.
//      Version: 15.0.0.0
//
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------

namespace ForeSiteIsntall
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using System.Windows.Input;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public partial class UIMap
    {
        
        #region Properties
        public UIWeatherfordForeSiteBWindow UIWeatherfordForeSiteBWindow
        {
            get
            {
                if ((this.mUIWeatherfordForeSiteBWindow == null))
                {
                    this.mUIWeatherfordForeSiteBWindow = new UIWeatherfordForeSiteBWindow();
                }
                return this.mUIWeatherfordForeSiteBWindow;
            }
        }
        
        public UISQLServerConnectionWindow UISQLServerConnectionWindow
        {
            get
            {
                if ((this.mUISQLServerConnectionWindow == null))
                {
                    this.mUISQLServerConnectionWindow = new UISQLServerConnectionWindow();
                }
                return this.mUISQLServerConnectionWindow;
            }
        }
        #endregion
        
        #region Fields
        private UIWeatherfordForeSiteBWindow mUIWeatherfordForeSiteBWindow;
        
        private UISQLServerConnectionWindow mUISQLServerConnectionWindow;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class UIWeatherfordForeSiteBWindow : WpfWindow
    {
        
        public UIWeatherfordForeSiteBWindow()
        {
            #region Search Criteria
            this.SearchProperties[WpfWindow.PropertyNames.Name] = "Weatherford ForeSite Bundle";
            this.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            this.WindowTitles.Add("Weatherford ForeSite Bundle");
            #endregion
        }
        
        #region Properties
        public UIItemCustom UIItemCustom
        {
            get
            {
                if ((this.mUIItemCustom == null))
                {
                    this.mUIItemCustom = new UIItemCustom(this);
                }
                return this.mUIItemCustom;
            }
        }
        
        public UIItemCustom1 UIItemCustom1
        {
            get
            {
                if ((this.mUIItemCustom1 == null))
                {
                    this.mUIItemCustom1 = new UIItemCustom1(this);
                }
                return this.mUIItemCustom1;
            }
        }
        
        public UIItemCustom2 UIItemCustom2
        {
            get
            {
                if ((this.mUIItemCustom2 == null))
                {
                    this.mUIItemCustom2 = new UIItemCustom2(this);
                }
                return this.mUIItemCustom2;
            }
        }
        
        public UIItemCustom3 UIItemCustom3
        {
            get
            {
                if ((this.mUIItemCustom3 == null))
                {
                    this.mUIItemCustom3 = new UIItemCustom3(this);
                }
                return this.mUIItemCustom3;
            }
        }
        #endregion
        
        #region Fields
        private UIItemCustom mUIItemCustom;
        
        private UIItemCustom1 mUIItemCustom1;
        
        private UIItemCustom2 mUIItemCustom2;
        
        private UIItemCustom3 mUIItemCustom3;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class UIItemCustom : WpfCustom
    {
        
        public UIItemCustom(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[WpfControl.PropertyNames.ClassName] = "Uia.SqlServerConnectionPage";
            this.WindowTitles.Add("Weatherford ForeSite Bundle");
            #endregion
        }
        
        #region Properties
        public UIServernameText UIServernameText
        {
            get
            {
                if ((this.mUIServernameText == null))
                {
                    this.mUIServernameText = new UIServernameText(this);
                }
                return this.mUIServernameText;
            }
        }
        #endregion
        
        #region Fields
        private UIServernameText mUIServernameText;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class UIServernameText : WpfText
    {
        
        public UIServernameText(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[WpfText.PropertyNames.Name] = "Server name:";
            this.WindowTitles.Add("Weatherford ForeSite Bundle");
            #endregion
        }
        
        #region Properties
        public WpfEdit UIItemEdit
        {
            get
            {
                if ((this.mUIItemEdit == null))
                {
                    this.mUIItemEdit = new WpfEdit(this);
                    #region Search Criteria
                    this.mUIItemEdit.SearchConfigurations.Add(SearchConfiguration.NextSibling);
                    this.mUIItemEdit.WindowTitles.Add("Weatherford ForeSite Bundle");
                    #endregion
                }
                return this.mUIItemEdit;
            }
        }
        
        public WpfEdit UIDBName
        {
            get
            {
                if ((this.mUIDBName == null))
                {
                    this.mUIDBName = new WpfEdit(this);
                    #region Search Criteria
                    this.mUIDBName.SearchProperties[WpfEdit.PropertyNames.Instance] = "2";
                    this.mUIDBName.SearchConfigurations.Add(SearchConfiguration.NextSibling);
                    this.mUIDBName.WindowTitles.Add("Weatherford ForeSite Bundle");
                    #endregion
                }
                return this.mUIDBName;
            }
        }
        #endregion
        
        #region Fields
        private WpfEdit mUIItemEdit;
        
        private WpfEdit mUIDBName;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class UIItemCustom1 : WpfCustom
    {
        
        public UIItemCustom1(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[WpfControl.PropertyNames.ClassName] = "Uia.DynaCardCygNetConfiguration";
            this.WindowTitles.Add("Weatherford ForeSite Bundle");
            #endregion
        }
        
        #region Properties
        public UIDomain UIDomain
        {
            get
            {
                if ((this.mUIDomain == null))
                {
                    this.mUIDomain = new UIDomain(this);
                }
                return this.mUIDomain;
            }
        }
        
        public UISite UISite
        {
            get
            {
                if ((this.mUISite == null))
                {
                    this.mUISite = new UISite(this);
                }
                return this.mUISite;
            }
        }
        
        public UIVHS UIVHS
        {
            get
            {
                if ((this.mUIVHS == null))
                {
                    this.mUIVHS = new UIVHS(this);
                }
                return this.mUIVHS;
            }
        }
        #endregion
        
        #region Fields
        private UIDomain mUIDomain;
        
        private UISite mUISite;
        
        private UIVHS mUIVHS;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class UIDomain : WpfButton
    {
        
        public UIDomain(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[WpfButton.PropertyNames.Name] = "Back";
            this.WindowTitles.Add("Weatherford ForeSite Bundle");
            #endregion
        }
        
        #region Properties
        public WpfEdit UIItemEdit
        {
            get
            {
                if ((this.mUIItemEdit == null))
                {
                    this.mUIItemEdit = new WpfEdit(this);
                    #region Search Criteria
                    this.mUIItemEdit.SearchConfigurations.Add(SearchConfiguration.NextSibling);
                    this.mUIItemEdit.WindowTitles.Add("Weatherford ForeSite Bundle");
                    #endregion
                }
                return this.mUIItemEdit;
            }
        }
        #endregion
        
        #region Fields
        private WpfEdit mUIItemEdit;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class UISite : WpfText
    {
        
        public UISite(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[WpfText.PropertyNames.Name] = "Domain Port must be between 5001 - 32767";
            this.WindowTitles.Add("Weatherford ForeSite Bundle");
            #endregion
        }
        
        #region Properties
        public WpfEdit UIItemEdit
        {
            get
            {
                if ((this.mUIItemEdit == null))
                {
                    this.mUIItemEdit = new WpfEdit(this);
                    #region Search Criteria
                    this.mUIItemEdit.SearchConfigurations.Add(SearchConfiguration.NextSibling);
                    this.mUIItemEdit.WindowTitles.Add("Weatherford ForeSite Bundle");
                    #endregion
                }
                return this.mUIItemEdit;
            }
        }
        #endregion
        
        #region Fields
        private WpfEdit mUIItemEdit;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class UIVHS : WpfText
    {
        
        public UIVHS(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[WpfText.PropertyNames.Name] = "Site Name must be between 0-8 characters\nand may contain only A-Z, 0-9, !, _, $ ";
            this.WindowTitles.Add("Weatherford ForeSite Bundle");
            #endregion
        }
        
        #region Properties
        public WpfEdit UIItemEdit
        {
            get
            {
                if ((this.mUIItemEdit == null))
                {
                    this.mUIItemEdit = new WpfEdit(this);
                    #region Search Criteria
                    this.mUIItemEdit.SearchConfigurations.Add(SearchConfiguration.NextSibling);
                    this.mUIItemEdit.WindowTitles.Add("Weatherford ForeSite Bundle");
                    #endregion
                }
                return this.mUIItemEdit;
            }
        }
        #endregion
        
        #region Fields
        private WpfEdit mUIItemEdit;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class UIItemCustom2 : WpfCustom
    {
        
        public UIItemCustom2(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[WpfControl.PropertyNames.ClassName] = "Uia.FlexServerConfiguration";
            this.WindowTitles.Add("Weatherford ForeSite Bundle");
            #endregion
        }
        
        #region Properties
        public UISpecifytheFlexLMliceText UISpecifytheFlexLMliceText
        {
            get
            {
                if ((this.mUISpecifytheFlexLMliceText == null))
                {
                    this.mUISpecifytheFlexLMliceText = new UISpecifytheFlexLMliceText(this);
                }
                return this.mUISpecifytheFlexLMliceText;
            }
        }
        #endregion
        
        #region Fields
        private UISpecifytheFlexLMliceText mUISpecifytheFlexLMliceText;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class UISpecifytheFlexLMliceText : WpfText
    {
        
        public UISpecifytheFlexLMliceText(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[WpfText.PropertyNames.Name] = "\nSpecify the FlexLM license server below:";
            this.WindowTitles.Add("Weatherford ForeSite Bundle");
            #endregion
        }
        
        #region Properties
        public WpfEdit UIItemEdit
        {
            get
            {
                if ((this.mUIItemEdit == null))
                {
                    this.mUIItemEdit = new WpfEdit(this);
                    #region Search Criteria
                    this.mUIItemEdit.SearchConfigurations.Add(SearchConfiguration.NextSibling);
                    this.mUIItemEdit.WindowTitles.Add("Weatherford ForeSite Bundle");
                    #endregion
                }
                return this.mUIItemEdit;
            }
        }
        #endregion
        
        #region Fields
        private WpfEdit mUIItemEdit;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class UIItemCustom3 : WpfCustom
    {
        
        public UIItemCustom3(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[WpfControl.PropertyNames.ClassName] = "Uia.SystemValidationPage";
            this.WindowTitles.Add("Weatherford ForeSite Bundle");
            #endregion
        }
        
        #region Properties
        public WpfButton UIInstallButton
        {
            get
            {
                if ((this.mUIInstallButton == null))
                {
                    this.mUIInstallButton = new WpfButton(this);
                    #region Search Criteria
                    this.mUIInstallButton.SearchProperties[WpfButton.PropertyNames.Name] = "Install";
                    this.mUIInstallButton.WindowTitles.Add("Weatherford ForeSite Bundle");
                    #endregion
                }
                return this.mUIInstallButton;
            }
        }
        #endregion
        
        #region Fields
        private WpfButton mUIInstallButton;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class UISQLServerConnectionWindow : WpfWindow
    {
        
        public UISQLServerConnectionWindow()
        {
            #region Search Criteria
            this.SearchProperties[WpfWindow.PropertyNames.Name] = "SQL Server Connection";
            this.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            this.WindowTitles.Add("SQL Server Connection");
            #endregion
        }
        
        #region Properties
        public WpfButton UIOKButton
        {
            get
            {
                if ((this.mUIOKButton == null))
                {
                    this.mUIOKButton = new WpfButton(this);
                    #region Search Criteria
                    this.mUIOKButton.SearchProperties[WpfButton.PropertyNames.Name] = "Ok";
                    this.mUIOKButton.WindowTitles.Add("SQL Server Connection");
                    #endregion
                }
                return this.mUIOKButton;
            }
        }
        #endregion
        
        #region Fields
        private WpfButton mUIOKButton;
        #endregion
    }
}
